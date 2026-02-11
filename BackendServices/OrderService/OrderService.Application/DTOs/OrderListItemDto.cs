using System;

namespace OrderService.Application.DTOs
{
    public class OrderListItemDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string OrderSource { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string FulfillmentStatus { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
