using System;

namespace OrderService.Application.DTOs
{
    public class FulfillmentPerformanceDto
    {
        public Guid WarehouseId { get; set; }
        public int? TotalOrders { get; set; }
        public int? TotalItems { get; set; }
        public int? AvgHoursToPickup { get; set; }
        public int? AvgHoursToPack { get; set; }
        public int? AvgHoursToComplete { get; set; }
        public int? ShippedItems { get; set; }
        public int? PendingItems { get; set; }
        public decimal? FulfillmentRate { get; set; }
        public decimal? OnTimeRate { get; set; }
    }
}
