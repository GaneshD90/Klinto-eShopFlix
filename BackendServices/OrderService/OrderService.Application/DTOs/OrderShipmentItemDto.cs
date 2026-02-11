using System;

namespace OrderService.Application.DTOs
{
    public class OrderShipmentItemDto
    {
        public Guid ShipmentItemId { get; set; }
        public Guid ShipmentId { get; set; }
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }
        public string SerialNumbers { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
