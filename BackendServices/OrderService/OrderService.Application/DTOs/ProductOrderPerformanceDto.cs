using System;

namespace OrderService.Application.DTOs
{
    public class ProductOrderPerformanceDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariationId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int? TotalOrders { get; set; }
        public int? TotalQuantitySold { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? AveragePrice { get; set; }
        public decimal? TotalDiscounts { get; set; }
        public int? CompletedOrders { get; set; }
        public int? ReturnedItems { get; set; }
        public decimal? ReturnRate { get; set; }
    }
}
