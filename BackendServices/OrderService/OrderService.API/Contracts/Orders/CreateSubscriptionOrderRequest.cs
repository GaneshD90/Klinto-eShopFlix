using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class CreateSubscriptionOrderRequest
    {
        public Guid CustomerId { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public int? TotalOccurrences { get; set; }
        public Guid? PaymentMethodId { get; set; }
    }
}
