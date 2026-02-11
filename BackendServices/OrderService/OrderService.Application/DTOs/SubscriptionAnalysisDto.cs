using System;

namespace OrderService.Application.DTOs
{
    public class SubscriptionAnalysisDto
    {
        public Guid SubscriptionId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string SubscriptionStatus { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? NextOrderDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int? TotalOccurrences { get; set; }
        public int CompletedOccurrences { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal? TotalSubscriptionValue { get; set; }
        public int? SubscriptionAgeDays { get; set; }
        public string SubscriptionHealth { get; set; } = string.Empty;
    }
}
