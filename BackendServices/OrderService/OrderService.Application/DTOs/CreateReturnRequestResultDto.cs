using System;

namespace OrderService.Application.DTOs
{
    public class CreateReturnRequestResultDto
    {
        public Guid? ReturnId { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
