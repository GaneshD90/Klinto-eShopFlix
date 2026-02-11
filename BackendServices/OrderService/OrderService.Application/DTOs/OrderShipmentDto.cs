using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs
{
    public class OrderShipmentDto
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public string ShipmentNumber { get; set; } = string.Empty;
        public Guid? WarehouseId { get; set; }
        public string ShipmentStatus { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
        public string CarrierCode { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string TrackingUrl { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public decimal? PackageWeight { get; set; }
        public string PackageDimensions { get; set; } = string.Empty;
        public int PackageCount { get; set; }
        public string LabelUrl { get; set; } = string.Empty;
        public bool SignatureRequired { get; set; }
        public string DeliverySignature { get; set; } = string.Empty;
        public string DeliveryProofImage { get; set; } = string.Empty;
        public string DeliveryInstructions { get; set; } = string.Empty;
        public int DeliveryAttempts { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IReadOnlyList<OrderShipmentItemDto> Items { get; set; } = [];
    }
}
