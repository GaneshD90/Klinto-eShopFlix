using System;

namespace OrderService.Application.DTOs
{
    public class OrderStatusHistoryDto
    {
        public Guid StatusHistoryId { get; set; }
        public Guid OrderId { get; set; }
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public Guid? ChangedBy { get; set; }
        public string? ChangeReason { get; set; }
        public string? Notes { get; set; }
        public bool IsCustomerNotified { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
