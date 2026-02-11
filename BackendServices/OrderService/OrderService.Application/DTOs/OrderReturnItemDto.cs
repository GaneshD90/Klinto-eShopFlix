using System;

namespace OrderService.Application.DTOs
{
    public class OrderReturnItemDto
    {
        public Guid ReturnItemId { get; set; }
        public Guid ReturnId { get; set; }
        public Guid OrderItemId { get; set; }
        public int ReturnQuantity { get; set; }
        public string ReturnReason { get; set; } = string.Empty;
        public string ItemCondition { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal RestockingFee { get; set; }
        public bool IsApproved { get; set; }
        public string InspectionNotes { get; set; } = string.Empty;
        public string DispositionAction { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
