using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class PlaceOrderOnHoldRequest
    {
        public string HoldType { get; set; } = string.Empty;
        public string HoldReason { get; set; } = string.Empty;
        public Guid? PlacedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
