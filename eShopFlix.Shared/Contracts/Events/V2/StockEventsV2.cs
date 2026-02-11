namespace Contracts.Events.V2
{
    /// <summary>Raised when stock is reserved for an order (richer than V1).</summary>
    public record InventoryReservedV2(
        Guid ReservationId,
        Guid OrderId,
        Guid ProductId,
        Guid? VariationId,
        int ReservedQuantity,
        Guid? WarehouseId,
        string? Sku,
        DateTime ExpiresAt,
        DateTime OccurredAt
    );

    /// <summary>Raised when reserved stock is committed (order finalized).</summary>
    public record InventoryCommittedV2(
        Guid ReservationId,
        Guid OrderId,
        Guid ProductId,
        int Quantity,
        Guid? WarehouseId,
        DateTime OccurredAt
    );

    /// <summary>Raised when reserved stock is released (cancellation / expiry).</summary>
    public record InventoryReleasedV2(
        Guid ReservationId,
        Guid OrderId,
        Guid ProductId,
        int Quantity,
        string ReleaseReason,
        DateTime OccurredAt
    );
}
