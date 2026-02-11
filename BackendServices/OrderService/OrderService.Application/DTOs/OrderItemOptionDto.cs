using System;

namespace OrderService.Application.DTOs
{
    public class OrderItemOptionDto
    {
        public Guid OrderItemOptionId { get; set; }
        public Guid OrderItemId { get; set; }
        public string OptionName { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
