namespace Contracts.Events.V2
{
    /// <summary>Raised when a payment is authorized (richer than V1).</summary>
    public record PaymentAuthorizedV2(
        Guid PaymentId,
        Guid OrderId,
        string OrderNumber,
        decimal Amount,
        string CurrencyCode,
        string PaymentMethod,
        string PaymentProvider,
        string TransactionId,
        string? AuthorizationCode,
        string Status,
        DateTime OccurredAt
    );

    /// <summary>Raised when an authorized payment is captured.</summary>
    public record PaymentCapturedV2(
        Guid PaymentId,
        Guid OrderId,
        string OrderNumber,
        decimal Amount,
        string CurrencyCode,
        string TransactionId,
        string Status,
        DateTime OccurredAt
    );

    /// <summary>Raised when a payment fails.</summary>
    public record PaymentFailedV2(
        Guid PaymentId,
        Guid OrderId,
        string OrderNumber,
        string Reason,
        string? ErrorCode,
        string PaymentMethod,
        DateTime OccurredAt
    );

    /// <summary>Raised when a refund payment is processed.</summary>
    public record PaymentRefundedV2(
        Guid RefundId,
        Guid PaymentId,
        Guid OrderId,
        string OrderNumber,
        decimal RefundAmount,
        string CurrencyCode,
        string RefundMethod,
        string TransactionId,
        DateTime OccurredAt
    );
}
