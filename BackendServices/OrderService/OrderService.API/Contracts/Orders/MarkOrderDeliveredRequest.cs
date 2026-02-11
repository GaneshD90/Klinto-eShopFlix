using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class MarkOrderDeliveredRequest
    {
        public string DeliverySignature { get; set; } = string.Empty;
        public string DeliveryProofImage { get; set; } = string.Empty;
    }
}
