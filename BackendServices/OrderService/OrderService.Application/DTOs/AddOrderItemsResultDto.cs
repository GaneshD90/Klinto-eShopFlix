using System;

namespace OrderService.Application.DTOs
{
    public class AddOrderItemsResultDto
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
