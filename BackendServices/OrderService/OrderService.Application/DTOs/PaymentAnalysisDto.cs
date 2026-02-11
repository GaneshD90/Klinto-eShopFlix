using System;

namespace OrderService.Application.DTOs
{
    public class PaymentAnalysisDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentProvider { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public int? TotalTransactions { get; set; }
        public int? TotalOrders { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? AverageTransactionAmount { get; set; }
        public decimal? SuccessfulAmount { get; set; }
        public decimal? FailedAmount { get; set; }
        public decimal? TotalRefunded { get; set; }
        public int? SuccessfulTransactions { get; set; }
        public int? FailedTransactions { get; set; }
        public decimal? SuccessRate { get; set; }
        public decimal? AverageRiskScore { get; set; }
    }
}
