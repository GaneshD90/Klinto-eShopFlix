using Contracts.Sagas.Cancellation;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// MassTransit Automatonymous State Machine for Order Cancellation.
/// 
/// Flow:
/// 1. CancellationRequested ? Send ReleaseInventoryForCancellation ? ReleasingStock
/// 2. InventoryReleasedForCancellation ? Send ProcessRefundForCancellation ? RefundingPayment
/// 3. RefundProcessedForCancellation ? Send FinalizeOrderCancellation ? FinalizingOrder
/// 4. OrderCancellationFinalized ? Completed
/// 
/// Compensation:
/// - RefundFailed ? Mark order as "CancellationPending" for manual review
/// </summary>
public sealed class CancellationSagaStateMachine : MassTransitStateMachine<CancellationSagaState>
{
    private readonly ILogger<CancellationSagaStateMachine> _logger;

    // ============ States ============
    public State ReleasingStock { get; private set; } = null!;
    public State RefundingPayment { get; private set; } = null!;
    public State FinalizingOrder { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // ============ Events ============
    public Event<CancellationRequested> CancellationRequested { get; private set; } = null!;
    public Event<InventoryReleasedForCancellation> InventoryReleased { get; private set; } = null!;
    public Event<RefundProcessedForCancellation> RefundProcessed { get; private set; } = null!;
    public Event<RefundFailedForCancellation> RefundFailed { get; private set; } = null!;
    public Event<OrderCancellationFinalized> OrderFinalized { get; private set; } = null!;

    public CancellationSagaStateMachine(ILogger<CancellationSagaStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        // Configure event correlation
        Event(() => CancellationRequested, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => InventoryReleased, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => RefundProcessed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => RefundFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderFinalized, e => e.CorrelateById(m => m.Message.CorrelationId));

        // ============ Initial State ============
        Initially(
            When(CancellationRequested)
                .Then(context =>
                {
                    var msg = context.Message;
                    context.Saga.OrderId = msg.OrderId;
                    context.Saga.OrderNumber = msg.OrderNumber;
                    context.Saga.CustomerId = msg.CustomerId;
                    context.Saga.CustomerEmail = msg.CustomerEmail;
                    context.Saga.OrderAmount = msg.OrderAmount;
                    context.Saga.CurrencyCode = msg.CurrencyCode;
                    context.Saga.PaymentId = msg.PaymentId;
                    context.Saga.CancellationType = msg.CancellationType;
                    context.Saga.CancellationReason = msg.CancellationReason;
                    context.Saga.CancelledBy = msg.CancelledBy;
                    context.Saga.RequestedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Cancellation saga started: OrderId={OrderId}, Reason={Reason}",
                        msg.OrderId, msg.CancellationReason);
                })
                .SendAsync(context => new Uri("queue:stock-service"),
                    context => context.Init<ReleaseInventoryForCancellation>(new ReleaseInventoryForCancellation(
                        context.Saga.CorrelationId,
                        context.Saga.OrderId,
                        context.Saga.OrderNumber,
                        context.Saga.CancellationReason
                    )))
                .TransitionTo(ReleasingStock)
        );

        // ============ Releasing Stock ============
        During(ReleasingStock,
            When(InventoryReleased)
                .Then(context =>
                {
                    context.Saga.StockReleasedAt = DateTime.UtcNow;
                    _logger.LogInformation(
                        "Inventory released for OrderId={OrderId}",
                        context.Saga.OrderId);
                })
                .IfElse(
                    context => context.Saga.PaymentId.HasValue && context.Saga.OrderAmount > 0,
                    thenBinder => thenBinder
                        .SendAsync(context => new Uri("queue:payment-service"),
                            context => context.Init<ProcessRefundForCancellation>(new ProcessRefundForCancellation(
                                context.Saga.CorrelationId,
                                context.Saga.OrderId,
                                context.Saga.OrderNumber,
                                context.Saga.CustomerId,
                                context.Saga.CustomerEmail,
                                context.Saga.PaymentId,
                                context.Saga.OrderAmount,
                                context.Saga.CurrencyCode,
                                context.Saga.CancellationReason
                            )))
                        .TransitionTo(RefundingPayment),
                    elseBinder => elseBinder
                        .Then(context =>
                        {
                            _logger.LogInformation(
                                "No payment to refund for OrderId={OrderId}, proceeding to finalize",
                                context.Saga.OrderId);
                        })
                        .SendAsync(context => new Uri("queue:order-service"),
                            context => context.Init<FinalizeOrderCancellation>(new FinalizeOrderCancellation(
                                context.Saga.CorrelationId,
                                context.Saga.OrderId,
                                context.Saga.OrderNumber,
                                context.Saga.CancellationType,
                                context.Saga.CancellationReason,
                                context.Saga.CancelledBy
                            )))
                        .TransitionTo(FinalizingOrder)
                )
        );

        // ============ Refunding Payment ============
        During(RefundingPayment,
            When(RefundProcessed)
                .Then(context =>
                {
                    context.Saga.RefundInitiatedAt = DateTime.UtcNow;
                    context.Saga.RefundId = context.Message.RefundId;
                    context.Saga.RefundAmount = context.Message.RefundAmount;
                    context.Saga.RefundTransactionId = context.Message.TransactionId;

                    _logger.LogInformation(
                        "Refund processed for OrderId={OrderId}, RefundId={RefundId}, Amount={Amount}",
                        context.Saga.OrderId, context.Message.RefundId, context.Message.RefundAmount);
                })
                .SendAsync(context => new Uri("queue:order-service"),
                    context => context.Init<FinalizeOrderCancellation>(new FinalizeOrderCancellation(
                        context.Saga.CorrelationId,
                        context.Saga.OrderId,
                        context.Saga.OrderNumber,
                        context.Saga.CancellationType,
                        context.Saga.CancellationReason,
                        context.Saga.CancelledBy
                    )))
                .TransitionTo(FinalizingOrder),

            When(RefundFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.FailedStep = "RefundProcessing";
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogWarning(
                        "Refund failed for OrderId={OrderId}: {Reason}",
                        context.Saga.OrderId, context.Message.Reason);
                })
                .PublishAsync(context => context.Init<CancellationFailed>(new CancellationFailed(
                    context.Saga.CorrelationId,
                    context.Saga.OrderId,
                    context.Saga.OrderNumber,
                    context.Message.Reason,
                    "RefundProcessing",
                    DateTime.UtcNow
                )))
                .TransitionTo(Failed)
                .Finalize()
        );

        // ============ Finalizing Order ============
        During(FinalizingOrder,
            When(OrderFinalized)
                .Then(context =>
                {
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Cancellation saga completed: OrderId={OrderId}, RefundAmount={RefundAmount}",
                        context.Saga.OrderId, context.Saga.RefundAmount);
                })
                .PublishAsync(context => context.Init<CancellationCompleted>(new CancellationCompleted(
                    context.Saga.CorrelationId,
                    context.Saga.OrderId,
                    context.Saga.OrderNumber,
                    context.Saga.RefundAmount,
                    DateTime.UtcNow
                )))
                .TransitionTo(Completed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
