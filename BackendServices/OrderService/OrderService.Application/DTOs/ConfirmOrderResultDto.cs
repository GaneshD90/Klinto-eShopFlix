using System;

namespace OrderService.Application.DTOs
{
    public class ConfirmOrderResultDto
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
