using System;

namespace OrderService.Application.DTOs
{
    public class OrderSummaryDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string FulfillmentStatus { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string OrderSource { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? TotalItems { get; set; }
        public int? TotalQuantity { get; set; }
        public int? DaysSinceOrder { get; set; }
        public string OrderStage { get; set; } = string.Empty;
    }
}
