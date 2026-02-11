using System;

namespace OrderService.Application.DTOs
{
    public class ReturnManagementDto
    {
        public Guid ReturnId { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string ReturnStatus { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public string ReturnReason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal RefundAmount { get; set; }
        public string QualityCheckStatus { get; set; } = string.Empty;
        public int? TotalReturnItems { get; set; }
        public int? TotalReturnQuantity { get; set; }
        public int? ProcessingDays { get; set; }
        public string ProcessStage { get; set; } = string.Empty;
    }
}
