using System;

namespace OrderService.Application.DTOs
{
    public class ShipmentTrackingDto
    {
        public Guid ShipmentId { get; set; }
        public string ShipmentNumber { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShipmentStatus { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string TrackingUrl { get; set; } = string.Empty;
        public DateTime? ShippedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public int? DaysInTransit { get; set; }
        public string DeliveryStatus { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingCountry { get; set; } = string.Empty;
    }
}
