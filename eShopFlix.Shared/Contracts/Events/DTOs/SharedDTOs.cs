namespace Contracts.DTOs
{
    public record CartItemDto(int ItemId, int Quantity, decimal UnitPrice, string? SKU = null);
    public record ProductSnapshotDto(int ProductId, string SKU, string Name, decimal Price, string? PrimaryImageUrl = null);
    public record OrderLineDto(int ProductId, int Quantity, decimal UnitPrice);
    public record UserDto(long UserId, string? Email = null, string? FullName = null);
    public record AddressDto(string Street, string City, string State, string PostalCode, string Country);

    /// <summary>V2 order line with Guid-based IDs for OrderService compatibility.</summary>
    public record OrderLineV2Dto(
        Guid ProductId,
        Guid? VariationId,
        string ProductName,
        string Sku,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice);
}
