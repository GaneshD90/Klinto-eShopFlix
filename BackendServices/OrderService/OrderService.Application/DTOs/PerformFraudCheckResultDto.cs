using System;

namespace OrderService.Application.DTOs
{
    public class PerformFraudCheckResultDto
    {
        public Guid? FraudCheckId { get; set; }
        public decimal? RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
