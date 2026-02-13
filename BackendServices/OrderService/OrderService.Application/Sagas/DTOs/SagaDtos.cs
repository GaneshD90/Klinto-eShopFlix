namespace OrderService.Application.Sagas.DTOs;

/// <summary>
/// DTO for saga state summary.
/// </summary>
public record SagaStateSummaryDto(
    Guid CorrelationId,
    string SagaType,
    string CurrentState,
    Guid OrderId,
    string OrderNumber,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? FailureReason
);

/// <summary>
/// DTO for checkout saga details.
/// </summary>
public record CheckoutSagaDetailsDto(
    Guid CorrelationId,
    string CurrentState,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    Guid CartId,
    decimal TotalAmount,
    string CurrencyCode,
    int ItemCount,
    DateTime? StartedAt,
    DateTime? InventoryReservedAt,
    DateTime? PaymentAuthorizedAt,
    DateTime? OrderConfirmedAt,
    DateTime? CartDeactivatedAt,
    DateTime? CompletedAt,
    string? FailureReason,
    string? FailedStep
);

/// <summary>
/// DTO for cancellation saga details.
/// </summary>
public record CancellationSagaDetailsDto(
    Guid CorrelationId,
    string CurrentState,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    decimal OrderAmount,
    string CancellationType,
    string CancellationReason,
    DateTime? RequestedAt,
    DateTime? StockReleasedAt,
    DateTime? RefundInitiatedAt,
    DateTime? CompletedAt,
    decimal? RefundAmount,
    string? RefundTransactionId,
    string? FailureReason,
    string? FailedStep
);

/// <summary>
/// DTO for return/refund saga details.
/// </summary>
public record ReturnRefundSagaDetailsDto(
    Guid CorrelationId,
    string CurrentState,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    string ReturnNumber,
    string ReturnType,
    string ReturnReason,
    int TotalItemsToReturn,
    decimal RefundAmount,
    DateTime? RequestedAt,
    DateTime? ValidatedAt,
    DateTime? RestockedAt,
    DateTime? RefundProcessedAt,
    DateTime? CompletedAt,
    string? RefundMethod,
    string? FailureReason,
    string? FailedStep
);

/// <summary>
/// DTO for saga statistics.
/// </summary>
public record SagaStatisticsDto(
    string SagaType,
    int TotalCount,
    int CompletedCount,
    int FailedCount,
    int InProgressCount,
    double AverageDurationSeconds,
    double SuccessRate
);
