using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// MassTransit Automatonymous State Machine for the Checkout Saga.
/// 
/// Flow:
/// 1. CheckoutStarted ? Send ReserveInventoryForCheckout ? AwaitingInventory
/// 2. InventoryReservedForCheckout ? Send AuthorizePaymentForCheckout ? AwaitingPayment
/// 3. PaymentAuthorizedForCheckout ? Send ConfirmOrderForCheckout ? AwaitingConfirmation
/// 4. OrderConfirmedForCheckout ? Send DeactivateCartForCheckout ? Completed
/// 
/// Compensation:
/// - InventoryReservationFailedForCheckout ? CancelOrderForCheckout ? Failed
/// - PaymentFailedForCheckout ? ReleaseInventoryForCheckout ? AwaitingCompensation ? CancelOrderForCheckout ? Failed
/// </summary>
public sealed class CheckoutSagaStateMachine : MassTransitStateMachine<CheckoutSagaState>
{
    private readonly ILogger<CheckoutSagaStateMachine> _logger;

    // ============ States ============
    public State AwaitingInventory { get; private set; } = null!;
    public State AwaitingPayment { get; private set; } = null!;
    public State AwaitingConfirmation { get; private set; } = null!;
    public State AwaitingCartDeactivation { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    public State CompensatingInventory { get; private set; } = null!;
    public State CompensatingOrder { get; private set; } = null!;

    // ============ Events ============
    public Event<CheckoutStarted> CheckoutStarted { get; private set; } = null!;
    public Event<InventoryReservedForCheckout> InventoryReserved { get; private set; } = null!;
    public Event<InventoryReservationFailedForCheckout> InventoryReservationFailed { get; private set; } = null!;
    public Event<PaymentAuthorizedForCheckout> PaymentAuthorized { get; private set; } = null!;
    public Event<PaymentFailedForCheckout> PaymentFailed { get; private set; } = null!;
    public Event<OrderConfirmedForCheckout> OrderConfirmed { get; private set; } = null!;
    public Event<OrderConfirmationFailedForCheckout> OrderConfirmationFailed { get; private set; } = null!;
    public Event<CartDeactivatedForCheckout> CartDeactivated { get; private set; } = null!;
    public Event<InventoryReleasedForCheckout> InventoryReleased { get; private set; } = null!;
    public Event<OrderCancelledForCheckout> OrderCancelled { get; private set; } = null!;

    public CheckoutSagaStateMachine(ILogger<CheckoutSagaStateMachine> logger)
    {
        _logger = logger;

        // Configure correlation
        InstanceState(x => x.CurrentState);

        // Configure event correlation
        Event(() => CheckoutStarted, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReserved, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReservationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentAuthorized, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PaymentFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderConfirmed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderConfirmationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CartDeactivated, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReleased, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCancelled, e => e.CorrelateById(m => m.Message.CorrelationId));

        // ============ Initial State ============
        Initially(
            When(CheckoutStarted)
                .Then(context =>
                {
                    var msg = context.Message;
                    context.Saga.OrderNumber = msg.OrderNumber;
                    context.Saga.CustomerId = msg.CustomerId;
                    context.Saga.CustomerEmail = msg.CustomerEmail;
                    context.Saga.TotalAmount = msg.TotalAmount;
                    context.Saga.CurrencyCode = msg.CurrencyCode;
                    context.Saga.ItemCount = msg.ItemCount;
                    context.Saga.CartId = msg.CartId;
                    context.Saga.StartedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Checkout saga started: OrderId={OrderId}, OrderNumber={OrderNumber}",
                        msg.OrderId, msg.OrderNumber);
                })
                .SendAsync(context => GetDestinationAddress("stock-service"),
                    context => context.Init<ReserveInventoryForCheckout>(new ReserveInventoryForCheckout(
                        context.Saga.CorrelationId,
                        context.Saga.CorrelationId, // OrderId = CorrelationId
                        context.Saga.OrderNumber,
                        context.Saga.CustomerId,
                        context.Saga.CartId,
                        context.Message.Lines
                    )))
                .TransitionTo(AwaitingInventory)
        );

        // ============ Awaiting Inventory Reservation ============
        During(AwaitingInventory,
            When(InventoryReserved)
                .Then(context =>
                {
                    context.Saga.InventoryReservedAt = DateTime.UtcNow;
                    context.Saga.ReservationId = context.Message.ReservationId;

                    _logger.LogInformation(
                        "Inventory reserved for OrderId={OrderId}, ReservationId={ReservationId}",
                        context.Message.OrderId, context.Message.ReservationId);
                })
                .SendAsync(context => GetDestinationAddress("payment-service"),
                    context => context.Init<AuthorizePaymentForCheckout>(new AuthorizePaymentForCheckout(
                        context.Saga.CorrelationId,
                        context.Saga.CorrelationId,
                        context.Saga.OrderNumber,
                        context.Saga.CustomerId,
                        context.Saga.CustomerEmail,
                        context.Saga.TotalAmount,
                        context.Saga.CurrencyCode,
                        "RazorPay" // Default payment method
                    )))
                .TransitionTo(AwaitingPayment),

            When(InventoryReservationFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.FailedStep = "InventoryReservation";

                    _logger.LogWarning(
                        "Inventory reservation failed for OrderId={OrderId}: {Reason}",
                        context.Message.OrderId, context.Message.Reason);
                })
                .SendAsync(context => GetDestinationAddress("order-service"),
                    context => context.Init<CancelOrderForCheckout>(new CancelOrderForCheckout(
                        context.Saga.CorrelationId,
                        context.Saga.CorrelationId,
                        context.Saga.OrderNumber,
                        $"Inventory reservation failed: {context.Message.Reason}"
                    )))
                .TransitionTo(CompensatingOrder)
        );

        // ============ Awaiting Payment Authorization ============
        During(AwaitingPayment,
            When(PaymentAuthorized)
                .Then(context =>
                {
                    context.Saga.PaymentAuthorizedAt = DateTime.UtcNow;
                    context.Saga.PaymentId = context.Message.PaymentId;
                    context.Saga.TransactionId = context.Message.TransactionId;

                    _logger.LogInformation(
                        "Payment authorized for OrderId={OrderId}, PaymentId={PaymentId}",
                        context.Message.OrderId, context.Message.PaymentId);
                })
                .SendAsync(context => GetDestinationAddress("order-service"),
                    context => context.Init<ConfirmOrderForCheckout>(new ConfirmOrderForCheckout(
                        context.Saga.CorrelationId,
                        context.Saga.CorrelationId,
                        context.Saga.OrderNumber,
                        context.Saga.PaymentId!.Value,
                        context.Saga.TransactionId!
                    )))
                .TransitionTo(AwaitingConfirmation),

            When(PaymentFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.FailedStep = "PaymentAuthorization";

                    _logger.LogWarning(
                        "Payment failed for OrderId={OrderId}: {Reason}",
                        context.Message.OrderId, context.Message.Reason);
                })
                .SendAsync(context => GetDestinationAddress("stock-service"),
                    context => context.Init<ReleaseInventoryForCheckout>(new ReleaseInventoryForCheckout(
                        context.Saga.CorrelationId,
                        context.Saga.CorrelationId,
                        context.Saga.OrderNumber,
                        context.Saga.ReservationId,
                        $"Payment failed: {context.Message.Reason}"
                    )))
                .TransitionTo(CompensatingInventory)
        );

        // ============ Awaiting Order Confirmation ============
        During(AwaitingConfirmation,
            When(OrderConfirmed)
                .Then(context =>
                {
                    context.Saga.ConfirmedAt = context.Message.ConfirmedAt;

                    _logger.LogInformation(
                        "Order confirmed for OrderId={OrderId}",
                        context.Message.OrderId);
                })
                .IfElse(
                    context => context.Saga.CartId.HasValue,
                    thenBinder => thenBinder
                        .SendAsync(context => GetDestinationAddress("cart-service"),
                            context => context.Init<DeactivateCartForCheckout>(new DeactivateCartForCheckout(
                                context.Saga.CorrelationId,
                                context.Saga.CorrelationId,
                                context.Saga.CartId!.Value
                            )))
                        .TransitionTo(AwaitingCartDeactivation),
                    elseBinder => elseBinder
                        .Then(context =>
                        {
                            context.Saga.CompletedAt = DateTime.UtcNow;
                            _logger.LogInformation(
                                "Checkout saga completed (no cart): OrderId={OrderId}",
                                context.Saga.CorrelationId);
                        })
                        .PublishAsync(context => context.Init<CheckoutCompleted>(new CheckoutCompleted(
                            context.Saga.CorrelationId,
                            context.Saga.CorrelationId,
                            context.Saga.OrderNumber,
                            DateTime.UtcNow
                        )))
                        .Finalize()
                ),

            When(OrderConfirmationFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.FailedStep = "OrderConfirmation";

                    _logger.LogWarning(
                        "Order confirmation failed for OrderId={OrderId}: {Reason}",
                        context.Message.OrderId, context.Message.Reason);
                })
                // Need to release inventory and (optionally) refund payment
                .SendAsync(context => GetDestinationAddress("stock-service"),
                    context => context.Init<ReleaseInventoryForCheckout>(new ReleaseInventoryForCheckout(
                        context.Saga.CorrelationId,
                        context.Saga.CorrelationId,
                        context.Saga.OrderNumber,
                        context.Saga.ReservationId,
                        $"Order confirmation failed: {context.Message.Reason}"
                    )))
                .TransitionTo(CompensatingInventory)
        );

        // ============ Awaiting Cart Deactivation ============
        During(AwaitingCartDeactivation,
            When(CartDeactivated)
                .Then(context =>
                {
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Checkout saga completed: OrderId={OrderId}, OrderNumber={OrderNumber}",
                        context.Saga.CorrelationId, context.Saga.OrderNumber);
                })
                .PublishAsync(context => context.Init<CheckoutCompleted>(new CheckoutCompleted(
                    context.Saga.CorrelationId,
                    context.Saga.CorrelationId,
                    context.Saga.OrderNumber,
                    DateTime.UtcNow
                )))
                .Finalize()
        );

        // ============ Compensation States ============
        During(CompensatingInventory,
            When(InventoryReleased)
                .Then(context =>
                {
                    context.Saga.CompensationStepsExecuted++;
                    _logger.LogInformation(
                        "Inventory released for OrderId={OrderId} (compensation)",
                        context.Message.OrderId);
                })
                .SendAsync(context => GetDestinationAddress("order-service"),
                    context => context.Init<CancelOrderForCheckout>(new CancelOrderForCheckout(
                        context.Saga.CorrelationId,
                        context.Saga.CorrelationId,
                        context.Saga.OrderNumber,
                        context.Saga.FailureReason ?? "Checkout saga compensation"
                    )))
                .TransitionTo(CompensatingOrder)
        );

        During(CompensatingOrder,
            When(OrderCancelled)
                .Then(context =>
                {
                    context.Saga.CompensationStepsExecuted++;
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Checkout saga failed and compensated: OrderId={OrderId}, Reason={Reason}",
                        context.Message.OrderId, context.Saga.FailureReason);
                })
                .PublishAsync(context => context.Init<CheckoutFailed>(new CheckoutFailed(
                    context.Saga.CorrelationId,
                    context.Saga.CorrelationId,
                    context.Saga.OrderNumber,
                    context.Saga.FailureReason ?? "Unknown",
                    context.Saga.FailedStep ?? "Unknown",
                    DateTime.UtcNow
                )))
                .Finalize()
        );

        // Mark completed sagas for cleanup
        SetCompletedWhenFinalized();
    }

    /// <summary>
    /// Gets the destination address for a service.
    /// In production, this would read from configuration or service discovery.
    /// </summary>
    private static Uri GetDestinationAddress(string service)
    {
        // MassTransit uses queue names for Send operations
        // The queue name is based on the consumer's endpoint
        return new Uri($"queue:{service}");
    }
}
