using System;

namespace OrderService.Application.DTOs
{
    public class MarkOrderDeliveredResultDto
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
