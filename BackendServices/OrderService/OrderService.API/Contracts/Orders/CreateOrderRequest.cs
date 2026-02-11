using System;
using System.Collections.Generic;

namespace OrderService.API.Contracts.Orders
{
    public sealed class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string OrderType { get; set; } = "Standard";
        public string OrderSource { get; set; } = "Web";
        public string CurrencyCode { get; set; } = "USD";
        public bool IsGuestCheckout { get; set; }
        public string? CustomerNotes { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
        public CreateOrderAddressRequest? BillingAddress { get; set; }
        public CreateOrderAddressRequest? ShippingAddress { get; set; }
    }

    public sealed class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public Guid? VariationId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public bool IsGift { get; set; }
        public string? GiftMessage { get; set; }
        public string? CustomizationDetails { get; set; }
        public string? ProductSnapshot { get; set; }
    }

    public sealed class CreateOrderAddressRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string StateProvince { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
