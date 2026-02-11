using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class CreateReturnRequestRequest
    {
        public Guid CustomerId { get; set; }
        public string ReturnType { get; set; } = string.Empty;
        public string ReturnReason { get; set; } = string.Empty;
        public string ReturnItemsJson { get; set; } = string.Empty;
        public string CustomerComments { get; set; } = string.Empty;
    }
}
