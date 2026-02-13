using PaymentService.Application.CQRS;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Payments.Queries;

/// <summary>
/// Query to get payment by ID.
/// </summary>
public record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto?>;

/// <summary>
/// Query to get payment by order ID.
/// </summary>
public record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<PaymentDto?>;

/// <summary>
/// Query to get all payments for a customer.
/// </summary>
public record GetPaymentsByCustomerQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<PaymentDto>>;

/// <summary>
/// Query to get refund history for a payment.
/// </summary>
public record GetRefundHistoryQuery(Guid PaymentId) : IQuery<IEnumerable<RefundDto>>;

// ============ Result DTOs ============

public record PaymentDto(
    Guid PaymentId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    decimal Amount,
    string CurrencyCode,
    string Status,
    string PaymentMethod,
    string? TransactionId,
    string? ProviderOrderId,
    DateTime CreatedAt,
    DateTime? AuthorizedAt,
    DateTime? CapturedAt
);

public record RefundDto(
    Guid RefundId,
    Guid PaymentId,
    decimal Amount,
    string Status,
    string Reason,
    string? TransactionId,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
