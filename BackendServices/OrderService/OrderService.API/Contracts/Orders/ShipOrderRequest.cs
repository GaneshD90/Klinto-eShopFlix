using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class ShipOrderRequest
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string TrackingUrl { get; set; } = string.Empty;
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
