using System;

namespace OrderService.Application.DTOs
{
    public class FraudRiskDashboardDto
    {
        public Guid FraudCheckId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string CheckStatus { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CheckedAt { get; set; }
        public string FraudStatus { get; set; } = string.Empty;
        public int? HoursPendingReview { get; set; }
    }
}
