using System;

namespace OrderService.Application.DTOs
{
    public class PendingOrderActionDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string FulfillmentStatus { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int? HoursPending { get; set; }
        public string RequiredAction { get; set; } = string.Empty;
        public string PriorityLevel { get; set; } = string.Empty;
    }
}
