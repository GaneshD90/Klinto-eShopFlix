using System;

namespace OrderService.Application.DTOs
{
    public class CreateShipmentResultDto
    {
        public Guid? ShipmentId { get; set; }
        public string ShipmentNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
