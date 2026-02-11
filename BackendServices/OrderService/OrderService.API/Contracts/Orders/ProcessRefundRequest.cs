using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class ProcessRefundRequest
    {
        public Guid? ReturnId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundType { get; set; } = string.Empty;
        public string RefundMethod { get; set; } = string.Empty;
        public string RefundReason { get; set; } = string.Empty;
    }
}
