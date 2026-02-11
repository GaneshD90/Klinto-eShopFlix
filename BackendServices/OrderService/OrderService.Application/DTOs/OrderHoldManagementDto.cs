using System;

namespace OrderService.Application.DTOs
{
    public class OrderHoldManagementDto
    {
        public Guid HoldId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string HoldType { get; set; } = string.Empty;
        public string HoldReason { get; set; } = string.Empty;
        public string HoldStatus { get; set; } = string.Empty;
        public DateTime PlacedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? HoldDurationHours { get; set; }
        public string CurrentHoldStatus { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty;
    }
}
