namespace Contracts.Sagas.ReturnRefund;

/// <summary>
/// Events that drive the return/refund saga state transitions.
/// </summary>

/// <summary>
/// Event that triggers the return/refund saga.
/// Published when a return is requested.
/// </summary>
public record ReturnRequested(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    Guid? ReturnId,
    string ReturnNumber,
    string ReturnType, // Refund, Exchange, StoreCredit
    string ReturnReason,
    string? CustomerComments,
    Guid? PaymentId,
    decimal RefundAmount,
    string CurrencyCode,
    IEnumerable<ReturnLineItem> Items,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating return request was validated.
/// </summary>
public record ReturnValidated(
    Guid CorrelationId,
    Guid OrderId,
    Guid? ReturnId,
    bool IsApproved,
    string? ValidationNotes,
    bool RequiresInspection,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating return validation failed.
/// </summary>
public record ReturnValidationFailed(
    Guid CorrelationId,
    Guid OrderId,
    Guid? ReturnId,
    string Reason,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating returned items were restocked.
/// </summary>
public record ItemsRestockedEvent(
    Guid CorrelationId,
    Guid OrderId,
    Guid? ReturnId,
    int RestockedCount,
    Guid? WarehouseId,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating restocking failed.
/// </summary>
public record RestockingFailed(
    Guid CorrelationId,
    Guid OrderId,
    Guid? ReturnId,
    string Reason,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating refund was processed for return.
/// </summary>
public record ReturnRefundProcessed(
    Guid CorrelationId,
    Guid OrderId,
    Guid? ReturnId,
    Guid RefundId,
    string TransactionId,
    decimal RefundAmount,
    string RefundMethod,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating refund failed for return (participant level).
/// </summary>
public record ReturnRefundProcessingFailed(
    Guid CorrelationId,
    Guid OrderId,
    Guid? ReturnId,
    string Reason,
    string? ErrorCode,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating return was finalized.
/// </summary>
public record ReturnFinalized(
    Guid CorrelationId,
    Guid OrderId,
    Guid? ReturnId,
    string ReturnNumber,
    string FinalStatus,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating return/refund saga completed successfully.
/// </summary>
public record ReturnRefundCompleted(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string ReturnNumber,
    decimal RefundAmount,
    string RefundMethod,
    DateTime CompletedAt
);

/// <summary>
/// Event indicating return/refund saga failed (saga level).
/// </summary>
public record ReturnRefundSagaFailed(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string? ReturnNumber,
    string FailureReason,
    string FailedStep,
    DateTime FailedAt
);
