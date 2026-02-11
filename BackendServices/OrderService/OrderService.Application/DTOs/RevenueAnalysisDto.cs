using System;

namespace OrderService.Application.DTOs
{
    public class RevenueAnalysisDto
    {
        public int? OrderYear { get; set; }
        public int? OrderMonth { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public string OrderSource { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public int? TotalOrders { get; set; }
        public int? UniqueCustomers { get; set; }
        public decimal? GrossRevenue { get; set; }
        public decimal? TotalDiscounts { get; set; }
        public decimal? TotalTaxes { get; set; }
        public decimal? TotalShipping { get; set; }
        public decimal? NetRevenue { get; set; }
        public decimal? AverageOrderValue { get; set; }
        public decimal? LostRevenue { get; set; }
        public decimal? RepeatCustomerRate { get; set; }
    }
}
