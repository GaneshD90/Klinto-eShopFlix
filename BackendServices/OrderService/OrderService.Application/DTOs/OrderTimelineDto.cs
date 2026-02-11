using System;

namespace OrderService.Application.DTOs
{
    public class OrderTimelineDto
    {
        public Guid TimelineId { get; set; }
        public Guid OrderId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public bool IsVisibleToCustomer { get; set; }
        public string? Icon { get; set; }
        public string? Metadata { get; set; }
    }
}
