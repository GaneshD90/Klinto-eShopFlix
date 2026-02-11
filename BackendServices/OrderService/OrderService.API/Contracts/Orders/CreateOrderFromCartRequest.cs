using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class CreateOrderFromCartRequest
    {
        public Guid CartId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string OrderSource { get; set; } = "Web";
        public string? BillingAddressJson { get; set; }
        public string? ShippingAddressJson { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
