using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class ShipOrderCommand : ICommand<ShipOrderResultDto>
    {
        public Guid ShipmentId { get; init; }
        public string TrackingNumber { get; init; } = string.Empty;
        public string TrackingUrl { get; init; } = string.Empty;
        public DateTime? EstimatedDeliveryDate { get; init; }
    }
}
