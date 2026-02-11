using System;

namespace OrderService.Application.DTOs
{
    public class OrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariationId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public string ItemStatus { get; set; } = string.Empty;
        public bool IsGift { get; set; }
        public string? GiftMessage { get; set; }
        public string? CustomizationDetails { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
