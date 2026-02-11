using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class CreateShipmentCommand : ICommand<CreateShipmentResultDto>
    {
        public Guid OrderId { get; init; }
        public Guid? WarehouseId { get; init; }
        public string ShippingMethod { get; init; } = string.Empty;
        public string CarrierName { get; init; } = string.Empty;
        public string ShipmentItemsJson { get; init; } = string.Empty;
    }
}
