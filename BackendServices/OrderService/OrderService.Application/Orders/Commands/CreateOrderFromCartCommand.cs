using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class CreateOrderFromCartCommand : ICommand<CreateOrderFromCartResultDto>
    {
        public Guid CartId { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerEmail { get; init; } = string.Empty;
        public string OrderSource { get; init; } = "Web";
        public string? BillingAddressJson { get; init; }
        public string? ShippingAddressJson { get; init; }
        public string? PaymentMethod { get; init; }
        public string? IpAddress { get; init; }
    }
}
