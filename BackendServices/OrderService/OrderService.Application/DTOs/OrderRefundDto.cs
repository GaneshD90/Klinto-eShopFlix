using System;

namespace OrderService.Application.DTOs
{
    public class OrderRefundDto
    {
        public Guid RefundId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ReturnId { get; set; }
        public Guid? OrderPaymentId { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public string RefundStatus { get; set; } = string.Empty;
        public string RefundType { get; set; } = string.Empty;
        public string RefundMethod { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
