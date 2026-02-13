namespace Contracts.Sagas.Cancellation;

/// <summary>
/// Events that drive the cancellation saga state transitions.
/// </summary>

/// <summary>
/// Event that triggers the cancellation saga.
/// Published when a cancellation is requested.
/// </summary>
public record CancellationRequested(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    decimal OrderAmount,
    string CurrencyCode,
    Guid? PaymentId,
    string CancellationType, // Customer, Admin, System
    string CancellationReason,
    Guid? CancelledBy,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating inventory was released for cancellation.
/// </summary>
public record InventoryReleasedForCancellation(
    Guid CorrelationId,
    Guid OrderId,
    int ReleasedQuantity,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating refund was processed successfully.
/// </summary>
public record RefundProcessedForCancellation(
    Guid CorrelationId,
    Guid OrderId,
    Guid RefundId,
    string TransactionId,
    decimal RefundAmount,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating refund failed.
/// </summary>
public record RefundFailedForCancellation(
    Guid CorrelationId,
    Guid OrderId,
    string Reason,
    string? ErrorCode,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating order cancellation was finalized.
/// </summary>
public record OrderCancellationFinalized(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    DateTime CancelledAt,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating customer was notified.
/// </summary>
public record CustomerNotifiedOfCancellation(
    Guid CorrelationId,
    Guid OrderId,
    DateTime NotifiedAt,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating cancellation saga completed successfully.
/// </summary>
public record CancellationCompleted(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    decimal? RefundAmount,
    DateTime CompletedAt
);

/// <summary>
/// Event indicating cancellation saga failed.
/// </summary>
public record CancellationFailed(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string FailureReason,
    string FailedStep,
    DateTime FailedAt
);
