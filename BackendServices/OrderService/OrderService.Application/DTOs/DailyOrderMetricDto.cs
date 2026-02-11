using System;

namespace OrderService.Application.DTOs
{
    public class DailyOrderMetricDto
    {
        public DateOnly? OrderDay { get; set; }
        public int? TotalOrders { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? AverageOrderValue { get; set; }
        public int? CompletedOrders { get; set; }
        public int? CancelledOrders { get; set; }
        public int? SubscriptionOrders { get; set; }
        public decimal? CancellationRate { get; set; }
    }
}
