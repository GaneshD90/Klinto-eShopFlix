using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs
{
    public class OrderReturnDto
    {
        public Guid ReturnId { get; set; }
        public Guid OrderId { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string ReturnStatus { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public string ReturnReason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal RestockingFee { get; set; }
        public decimal ReturnShippingCost { get; set; }
        public string CustomerComments { get; set; } = string.Empty;
        public string InternalNotes { get; set; } = string.Empty;
        public string ReturnShippingLabel { get; set; } = string.Empty;
        public string ReturnTrackingNumber { get; set; } = string.Empty;
        public string QualityCheckStatus { get; set; } = string.Empty;
        public string QualityCheckNotes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IReadOnlyList<OrderReturnItemDto> Items { get; set; } = [];
    }
}
