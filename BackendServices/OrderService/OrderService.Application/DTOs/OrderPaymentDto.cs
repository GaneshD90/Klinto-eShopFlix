using System;

namespace OrderService.Application.DTOs
{
    public class OrderPaymentDto
    {
        public Guid OrderPaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentProvider { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal RefundedAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string AuthorizationCode { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string CardLastFour { get; set; } = string.Empty;
        public int? CardExpiryMonth { get; set; }
        public int? CardExpiryYear { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? AuthorizedAt { get; set; }
        public DateTime? CapturedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string PaymentGatewayResponse { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public decimal? RiskScore { get; set; }
        public string ThreeDsecureStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
