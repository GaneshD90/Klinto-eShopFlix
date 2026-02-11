using System;

namespace OrderService.Application.DTOs
{
    public class OrderAdjustmentDto
    {
        public Guid AdjustmentId { get; set; }
        public Guid OrderId { get; set; }
        public string AdjustmentType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Guid? AppliedBy { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
