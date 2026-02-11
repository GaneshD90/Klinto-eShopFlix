using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs
{
    public class OrderDetailDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid? CartId { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string OrderSource { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string FulfillmentStatus { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public bool IsGuestCheckout { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string? CustomerNotes { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderAddressDto> Addresses { get; set; } = new();
    }
}
