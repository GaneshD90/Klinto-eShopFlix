using System;

namespace OrderService.Application.DTOs
{
    public class OrderMetricDto
    {
        public Guid MetricId { get; set; }
        public Guid OrderId { get; set; }
        public int? TimeToConfirmation { get; set; }
        public int? TimeToFirstShipment { get; set; }
        public int? TimeToFullFulfillment { get; set; }
        public int? TimeToDelivery { get; set; }
        public decimal? CustomerLifetimeValue { get; set; }
        public bool IsRepeatCustomer { get; set; }
        public decimal? OrderProcessingCost { get; set; }
        public decimal? ProfitMargin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
