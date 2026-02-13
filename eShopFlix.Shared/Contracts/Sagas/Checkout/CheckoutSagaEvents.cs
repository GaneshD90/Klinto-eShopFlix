namespace Contracts.Sagas.Checkout;

/// <summary>
/// Events published by participant services in response to saga commands.
/// The saga state machine reacts to these events to progress or compensate.
/// </summary>

/// <summary>
/// Event indicating checkout saga should start.
/// Published by OrderService when an order is created from a cart.
/// </summary>
public record CheckoutStarted(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerEmail,
    decimal TotalAmount,
    string CurrencyCode,
    int ItemCount,
    Guid? CartId,
    IEnumerable<CheckoutLineItem> Lines,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating inventory was successfully reserved for checkout.
/// Published by StockService.
/// </summary>
public record InventoryReservedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    Guid ReservationId,
    int TotalQuantityReserved,
    DateTime ExpiresAt,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating inventory reservation failed.
/// Published by StockService when stock is insufficient.
/// </summary>
public record InventoryReservationFailedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string Reason,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating payment was successfully authorized.
/// Published by PaymentService.
/// </summary>
public record PaymentAuthorizedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    Guid PaymentId,
    string TransactionId,
    decimal Amount,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating payment authorization failed.
/// Published by PaymentService.
/// </summary>
public record PaymentFailedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string Reason,
    string? ErrorCode,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating order was successfully confirmed.
/// Published by OrderService.
/// </summary>
public record OrderConfirmedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    DateTime ConfirmedAt,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating order confirmation failed.
/// Published by OrderService.
/// </summary>
public record OrderConfirmationFailedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    string Reason,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating cart was successfully deactivated.
/// Published by CartService.
/// </summary>
public record CartDeactivatedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    Guid CartId,
    DateTime OccurredAt
);

// ============ Compensation Events ============

/// <summary>
/// Event indicating inventory was released (compensation complete).
/// Published by StockService.
/// </summary>
public record InventoryReleasedForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating order was cancelled (compensation complete).
/// Published by OrderService.
/// </summary>
public record OrderCancelledForCheckout(
    Guid CorrelationId,
    Guid OrderId,
    DateTime OccurredAt
);

/// <summary>
/// Event indicating the checkout saga completed successfully.
/// </summary>
public record CheckoutCompleted(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    DateTime CompletedAt
);

/// <summary>
/// Event indicating the checkout saga failed and compensation is complete.
/// </summary>
public record CheckoutFailed(
    Guid CorrelationId,
    Guid OrderId,
    string OrderNumber,
    string FailureReason,
    string FailedStep,
    DateTime FailedAt
);
