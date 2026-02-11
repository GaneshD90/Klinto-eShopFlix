using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class CreateShipmentRequest
    {
        public Guid? WarehouseId { get; set; }
        public string ShippingMethod { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
        public string ShipmentItemsJson { get; set; } = string.Empty;
    }
}
