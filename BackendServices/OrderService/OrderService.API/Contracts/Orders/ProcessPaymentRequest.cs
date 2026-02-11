using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class ProcessPaymentRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentProvider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string AuthorizationCode { get; set; } = string.Empty;
        public string PaymentGatewayResponse { get; set; } = string.Empty;
    }
}
