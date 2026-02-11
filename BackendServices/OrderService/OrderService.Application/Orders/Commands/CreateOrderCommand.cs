using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class CreateOrderCommand : ICommand<OrderDetailDto>
    {
        public Guid CustomerId { get; init; }
        public string CustomerEmail { get; init; } = string.Empty;
        public string? CustomerPhone { get; init; }
        public string OrderType { get; init; } = "Standard";
        public string OrderSource { get; init; } = "Web";
        public string CurrencyCode { get; init; } = "USD";
        public bool IsGuestCheckout { get; init; }
        public string? CustomerNotes { get; init; }
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public List<CreateOrderItemInput> Items { get; init; } = new();
        public CreateOrderAddressInput? BillingAddress { get; init; }
        public CreateOrderAddressInput? ShippingAddress { get; init; }
    }

    public sealed class CreateOrderItemInput
    {
        public Guid ProductId { get; init; }
        public Guid? VariationId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal OriginalPrice { get; init; }
        public bool IsGift { get; init; }
        public string? GiftMessage { get; init; }
        public string? CustomizationDetails { get; init; }
        public string? ProductSnapshot { get; init; }
    }

    public sealed class CreateOrderAddressInput
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? CompanyName { get; init; }
        public string AddressLine1 { get; init; } = string.Empty;
        public string? AddressLine2 { get; init; }
        public string City { get; init; } = string.Empty;
        public string StateProvince { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
        public string CountryCode { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public string? Email { get; init; }
    }
}
