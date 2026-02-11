using System;

namespace OrderService.Application.DTOs
{
    public class OrderCancellationDto
    {
        public Guid CancellationId { get; set; }
        public Guid OrderId { get; set; }
        public string CancellationType { get; set; } = string.Empty;
        public string CancellationReason { get; set; } = string.Empty;
        public Guid? CancelledBy { get; set; }
        public string CancelledByType { get; set; } = string.Empty;
        public DateTime CancelledAt { get; set; }
        public bool RefundInitiated { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CancellationFee { get; set; }
        public bool CustomerNotified { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
