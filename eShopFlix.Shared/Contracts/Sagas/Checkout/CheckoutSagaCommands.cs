namespace Contracts.Sagas.Checkout;

/// <summary>
/// Commands sent by the saga orchestrator to participant services.
/// </summary>

/// <summary>
/// Command to reserve inventory for checkout.
/// Sent by saga to StockService after order is created.
/// </summary>
public record ReserveInventoryForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    Guid? CartId,
    IEnumerable<CheckoutLineItem> Lines
);

/// <summary>
/// Command to authorize payment for checkout.
/// Sent by saga to PaymentService after inventory is reserved.
/// </summary>
public record AuthorizePaymentForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    decimal Amount,
    string CurrencyCode,
    string PaymentMethod
);

/// <summary>
/// Command to confirm order after payment is authorized.
/// Sent by saga to OrderService.
/// </summary>
public record ConfirmOrderForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid PaymentId,
    string TransactionId
);

/// <summary>
/// Command to deactivate cart after successful checkout.
/// Sent by saga to CartService.
/// </summary>
public record DeactivateCartForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    Guid CartId
);

// ============ Compensation Commands ============

/// <summary>
/// Command to release inventory reservation (compensation).
/// Sent when payment fails or order creation fails.
/// </summary>
public record ReleaseInventoryForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid? ReservationId,
    string Reason
);

/// <summary>
/// Command to cancel order (compensation).
/// Sent when downstream steps fail.
/// </summary>
public record CancelOrderForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string CancellationReason
);

/// <summary>
/// Line item for checkout commands.
/// </summary>
public record CheckoutLineItem(
    Guid ProductId,
    Guid? VariationId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice
);
