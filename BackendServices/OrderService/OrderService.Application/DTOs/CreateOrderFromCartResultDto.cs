using System;

namespace OrderService.Application.DTOs
{
    public class CreateOrderFromCartResultDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
