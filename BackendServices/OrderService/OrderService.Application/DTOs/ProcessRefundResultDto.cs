using System;

namespace OrderService.Application.DTOs
{
    public class ProcessRefundResultDto
    {
        public Guid? RefundId { get; set; }
        public string RefundNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
