using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class CancelOrderRequest
    {
        public string CancellationType { get; set; } = "Customer";
        public string CancellationReason { get; set; } = string.Empty;
        public Guid? CancelledBy { get; set; }
        public string CancelledByType { get; set; } = "Customer";
    }
}
