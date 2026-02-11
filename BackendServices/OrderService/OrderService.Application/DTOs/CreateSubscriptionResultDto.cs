using System;

namespace OrderService.Application.DTOs
{
    public class CreateSubscriptionResultDto
    {
        public Guid? SubscriptionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
