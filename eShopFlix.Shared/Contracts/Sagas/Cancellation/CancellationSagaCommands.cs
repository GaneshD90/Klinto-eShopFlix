namespace Contracts.Sagas.Cancellation;

/// <summary>
/// Commands sent by the cancellation saga orchestrator to participant services.
/// </summary>

/// <summary>
/// Command to release all inventory reservations for a cancelled order.
/// </summary>
public record ReleaseInventoryForCancellation(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string Reason
);

/// <summary>
/// Command to process refund for a cancelled order.
/// </summary>
public record ProcessRefundForCancellation(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    Guid? PaymentId,
    decimal RefundAmount,
    string CurrencyCode,
    string RefundReason
);

/// <summary>
/// Command to finalize order cancellation status.
/// </summary>
public record FinalizeOrderCancellation(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string CancellationType,
    string CancellationReason,
    Guid? CancelledBy
);

/// <summary>
/// Command to notify customer about cancellation.
/// </summary>
public record NotifyCustomerOfCancellation(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string CustomerEmail,
    string CancellationReason,
    decimal? RefundAmount
);
