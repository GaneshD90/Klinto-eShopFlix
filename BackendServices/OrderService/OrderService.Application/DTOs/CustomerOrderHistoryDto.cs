using System;

namespace OrderService.Application.DTOs
{
    public class CustomerOrderHistoryDto
    {
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public int? TotalOrders { get; set; }
        public decimal? TotalSpent { get; set; }
        public decimal? AverageOrderValue { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? FirstOrderDate { get; set; }
        public int? CompletedOrders { get; set; }
        public int? CancelledOrders { get; set; }
        public string CustomerSegment { get; set; } = string.Empty;
    }
}
