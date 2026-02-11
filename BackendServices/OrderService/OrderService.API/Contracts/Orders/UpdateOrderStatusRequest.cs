using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class UpdateOrderStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
        public Guid? ChangedBy { get; set; }
        public string? ChangeReason { get; set; }
        public string? Notes { get; set; }
        public bool NotifyCustomer { get; set; }
    }
}
