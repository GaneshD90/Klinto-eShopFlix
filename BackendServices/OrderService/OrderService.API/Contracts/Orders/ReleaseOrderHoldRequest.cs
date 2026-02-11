using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class ReleaseOrderHoldRequest
    {
        public Guid? ReleasedBy { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
