namespace Contracts.Events.V2;

/// <summary>
/// Raised when inventory reservation fails for an order (compensation trigger).
/// StockService publishes this when it cannot reserve stock for OrderCreatedV2.
/// </summary>
public record InventoryReservationFailedV2(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string Reason,
    DateTime OccurredAt
);

/// <summary>
/// Raised when stock should be released as compensation (e.g., payment failed).
/// OrderService or PaymentService publishes this to trigger StockService compensation.
/// </summary>
public record ReleaseInventoryRequestedV2(
    Guid OrderId,
    string OrderNumber,
    string Reason,
    DateTime OccurredAt
);
