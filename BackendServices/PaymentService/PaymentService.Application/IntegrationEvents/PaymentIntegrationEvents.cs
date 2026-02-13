namespace PaymentService.Application.IntegrationEvents;

/// <summary>
/// Integration event published when a payment order is created.
/// </summary>
public record PaymentOrderCreatedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string CurrencyCode,
    string ProviderOrderId
);

/// <summary>
/// Integration event published when a payment is authorized.
/// </summary>
public record PaymentAuthorizedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string TransactionId
);

/// <summary>
/// Integration event published when a payment is captured.
/// </summary>
public record PaymentCapturedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string TransactionId
);

/// <summary>
/// Integration event published when a payment fails.
/// </summary>
public record PaymentFailedIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    string Reason,
    string? ErrorCode
);

/// <summary>
/// Integration event published when a refund is processed.
/// </summary>
public record PaymentRefundedIntegrationEvent(
    Guid RefundId,
    Guid PaymentId,
    Guid OrderId,
    string OrderNumber,
    decimal RefundAmount,
    string TransactionId
);
