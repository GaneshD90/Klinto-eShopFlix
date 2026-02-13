using Contracts.Sagas.ReturnRefund;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// MassTransit Automatonymous State Machine for Return & Refund processing.
/// 
/// Flow:
/// 1. ReturnRequested ? Send ValidateReturnRequest ? Validating
/// 2. ReturnValidated ? Send RestockReturnedItems ? RestockingInventory
/// 3. ItemsRestockedEvent ? Send ProcessReturnRefund ? ProcessingRefund
/// 4. ReturnRefundProcessed ? Send FinalizeReturn ? Finalizing
/// 5. ReturnFinalized ? Completed
/// 
/// Compensation:
/// - ValidationFailed ? Failed (no compensation needed)
/// - RestockingFailed ? Retry or manual intervention
/// - RefundFailed ? Mark for manual refund processing
/// </summary>
public sealed class ReturnRefundSagaStateMachine : MassTransitStateMachine<ReturnRefundSagaState>
{
    private readonly ILogger<ReturnRefundSagaStateMachine> _logger;

    // ============ States ============
    public State Validating { get; private set; } = null!;
    public State RestockingInventory { get; private set; } = null!;
    public State ProcessingRefund { get; private set; } = null!;
    public State Finalizing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // ============ Events ============
    public Event<ReturnRequested> ReturnRequested { get; private set; } = null!;
    public Event<ReturnValidated> ReturnValidated { get; private set; } = null!;
    public Event<ReturnValidationFailed> ValidationFailed { get; private set; } = null!;
    public Event<ItemsRestockedEvent> ItemsRestocked { get; private set; } = null!;
    public Event<RestockingFailed> RestockingFailed { get; private set; } = null!;
    public Event<ReturnRefundProcessed> RefundProcessed { get; private set; } = null!;
    public Event<ReturnRefundProcessingFailed> RefundFailed { get; private set; } = null!;
    public Event<ReturnFinalized> ReturnFinalized { get; private set; } = null!;

    public ReturnRefundSagaStateMachine(ILogger<ReturnRefundSagaStateMachine> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        // Configure event correlation
        Event(() => ReturnRequested, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ReturnValidated, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ValidationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ItemsRestocked, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => RestockingFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => RefundProcessed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => RefundFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ReturnFinalized, e => e.CorrelateById(m => m.Message.CorrelationId));

        // ============ Initial State ============
        Initially(
            When(ReturnRequested)
                .Then(context =>
                {
                    var msg = context.Message;
                    context.Saga.OrderId = msg.OrderId;
                    context.Saga.OrderNumber = msg.OrderNumber;
                    context.Saga.CustomerId = msg.CustomerId;
                    context.Saga.CustomerEmail = msg.CustomerEmail;
                    context.Saga.ReturnId = msg.ReturnId;
                    context.Saga.ReturnNumber = msg.ReturnNumber;
                    context.Saga.ReturnType = msg.ReturnType;
                    context.Saga.ReturnReason = msg.ReturnReason;
                    context.Saga.CustomerComments = msg.CustomerComments;
                    context.Saga.PaymentId = msg.PaymentId;
                    context.Saga.RefundAmount = msg.RefundAmount;
                    context.Saga.CurrencyCode = msg.CurrencyCode;
                    context.Saga.TotalItemsToReturn = msg.Items?.Count() ?? 0;
                    context.Saga.RequestedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Return/Refund saga started: OrderId={OrderId}, ReturnNumber={ReturnNumber}, Type={Type}",
                        msg.OrderId, msg.ReturnNumber, msg.ReturnType);
                })
                .SendAsync(context => new Uri("queue:order-service"),
                    context => context.Init<ValidateReturnRequest>(new ValidateReturnRequest(
                        context.Saga.CorrelationId,
                        context.Saga.OrderId,
                        context.Saga.OrderNumber,
                        context.Saga.ReturnId,
                        context.Saga.ReturnType,
                        context.Saga.ReturnReason,
                        context.Message.Items
                    )))
                .TransitionTo(Validating)
        );

        // ============ Validating ============
        During(Validating,
            When(ReturnValidated)
                .Then(context =>
                {
                    context.Saga.ValidatedAt = DateTime.UtcNow;
                    context.Saga.IsValidated = context.Message.IsApproved;
                    context.Saga.ValidationNotes = context.Message.ValidationNotes;
                    context.Saga.RequiresInspection = context.Message.RequiresInspection;

                    _logger.LogInformation(
                        "Return validated for OrderId={OrderId}, Approved={Approved}",
                        context.Saga.OrderId, context.Message.IsApproved);
                })
                .IfElse(
                    context => context.Message.IsApproved,
                    thenBinder => thenBinder
                        .SendAsync(context => new Uri("queue:stock-service"),
                            context => context.Init<RestockReturnedItems>(new RestockReturnedItems(
                                context.Saga.CorrelationId,
                                context.Saga.OrderId,
                                context.Saga.OrderNumber,
                                context.Saga.ReturnId,
                                Array.Empty<ReturnLineItem>(), // Items from original request
                                context.Saga.WarehouseId
                            )))
                        .TransitionTo(RestockingInventory),
                    elseBinder => elseBinder
                        .Then(context =>
                        {
                            context.Saga.FailureReason = context.Saga.ValidationNotes ?? "Return not approved";
                            context.Saga.FailedStep = "Validation";
                            context.Saga.CompletedAt = DateTime.UtcNow;
                        })
                        .PublishAsync(context => context.Init<ReturnRefundSagaFailed>(new ReturnRefundSagaFailed(
                            context.Saga.CorrelationId,
                            context.Saga.OrderId,
                            context.Saga.OrderNumber,
                            context.Saga.ReturnNumber,
                            context.Saga.ValidationNotes ?? "Return not approved",
                            "Validation",
                            DateTime.UtcNow
                        )))
                        .TransitionTo(Failed)
                        .Finalize()
                ),

            When(ValidationFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.FailedStep = "Validation";
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogWarning(
                        "Return validation failed for OrderId={OrderId}: {Reason}",
                        context.Saga.OrderId, context.Message.Reason);
                })
                .PublishAsync(context => context.Init<ReturnRefundSagaFailed>(new ReturnRefundSagaFailed(
                    context.Saga.CorrelationId,
                    context.Saga.OrderId,
                    context.Saga.OrderNumber,
                    context.Saga.ReturnNumber,
                    context.Message.Reason,
                    "Validation",
                    DateTime.UtcNow
                )))
                .TransitionTo(Failed)
                .Finalize()
        );

        // ============ Restocking Inventory ============
        During(RestockingInventory,
            When(ItemsRestocked)
                .Then(context =>
                {
                    context.Saga.RestockedAt = DateTime.UtcNow;
                    context.Saga.ItemsRestocked = context.Message.RestockedCount;
                    context.Saga.WarehouseId = context.Message.WarehouseId;

                    _logger.LogInformation(
                        "Items restocked for OrderId={OrderId}, Count={Count}",
                        context.Saga.OrderId, context.Message.RestockedCount);
                })
                .IfElse(
                    context => context.Saga.RefundAmount > 0,
                    thenBinder => thenBinder
                        .SendAsync(context => new Uri("queue:payment-service"),
                            context => context.Init<ProcessReturnRefund>(new ProcessReturnRefund(
                                context.Saga.CorrelationId,
                                context.Saga.OrderId,
                                context.Saga.OrderNumber,
                                context.Saga.CustomerId,
                                context.Saga.CustomerEmail,
                                context.Saga.PaymentId,
                                context.Saga.RefundAmount,
                                context.Saga.CurrencyCode,
                                context.Saga.RefundMethod ?? "OriginalPayment",
                                context.Saga.ReturnReason
                            )))
                        .TransitionTo(ProcessingRefund),
                    elseBinder => elseBinder
                        .Then(context =>
                        {
                            _logger.LogInformation(
                                "No refund needed for OrderId={OrderId}, proceeding to finalize",
                                context.Saga.OrderId);
                        })
                        .SendAsync(context => new Uri("queue:order-service"),
                            context => context.Init<FinalizeReturn>(new FinalizeReturn(
                                context.Saga.CorrelationId,
                                context.Saga.OrderId,
                                context.Saga.OrderNumber,
                                context.Saga.ReturnId,
                                context.Saga.ReturnNumber,
                                "Completed"
                            )))
                        .TransitionTo(Finalizing)
                ),

            When(RestockingFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.FailedStep = "RestockingInventory";
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogWarning(
                        "Restocking failed for OrderId={OrderId}: {Reason}",
                        context.Saga.OrderId, context.Message.Reason);
                })
                .PublishAsync(context => context.Init<ReturnRefundSagaFailed>(new ReturnRefundSagaFailed(
                    context.Saga.CorrelationId,
                    context.Saga.OrderId,
                    context.Saga.OrderNumber,
                    context.Saga.ReturnNumber,
                    context.Message.Reason,
                    "RestockingInventory",
                    DateTime.UtcNow
                )))
                .TransitionTo(Failed)
                .Finalize()
        );

        // ============ Processing Refund ============
        During(ProcessingRefund,
            When(RefundProcessed)
                .Then(context =>
                {
                    context.Saga.RefundProcessedAt = DateTime.UtcNow;
                    context.Saga.RefundId = context.Message.RefundId;
                    context.Saga.RefundTransactionId = context.Message.TransactionId;
                    context.Saga.RefundMethod = context.Message.RefundMethod;

                    _logger.LogInformation(
                        "Refund processed for OrderId={OrderId}, RefundId={RefundId}, Amount={Amount}",
                        context.Saga.OrderId, context.Message.RefundId, context.Message.RefundAmount);
                })
                .SendAsync(context => new Uri("queue:order-service"),
                    context => context.Init<FinalizeReturn>(new FinalizeReturn(
                        context.Saga.CorrelationId,
                        context.Saga.OrderId,
                        context.Saga.OrderNumber,
                        context.Saga.ReturnId,
                        context.Saga.ReturnNumber,
                        "Completed"
                    )))
                .TransitionTo(Finalizing),

            When(RefundFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.FailedStep = "ProcessingRefund";
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogWarning(
                        "Refund failed for return OrderId={OrderId}: {Reason}",
                        context.Saga.OrderId, context.Message.Reason);
                })
                .PublishAsync(context => context.Init<ReturnRefundSagaFailed>(new ReturnRefundSagaFailed(
                    context.Saga.CorrelationId,
                    context.Saga.OrderId,
                    context.Saga.OrderNumber,
                    context.Saga.ReturnNumber,
                    context.Message.Reason,
                    "ProcessingRefund",
                    DateTime.UtcNow
                )))
                .TransitionTo(Failed)
                .Finalize()
        );

        // ============ Finalizing ============
        During(Finalizing,
            When(ReturnFinalized)
                .Then(context =>
                {
                    context.Saga.CompletedAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Return/Refund saga completed: OrderId={OrderId}, ReturnNumber={ReturnNumber}",
                        context.Saga.OrderId, context.Saga.ReturnNumber);
                })
                .PublishAsync(context => context.Init<ReturnRefundCompleted>(new ReturnRefundCompleted(
                    context.Saga.CorrelationId,
                    context.Saga.OrderId,
                    context.Saga.OrderNumber,
                    context.Saga.ReturnNumber,
                    context.Saga.RefundAmount,
                    context.Saga.RefundMethod ?? "None",
                    DateTime.UtcNow
                )))
                .TransitionTo(Completed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
