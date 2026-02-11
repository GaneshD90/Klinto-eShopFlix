using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetShipmentTrackingQuery : IQuery<IReadOnlyList<ShipmentTrackingDto>>
    {
        public Guid OrderId { get; init; }
    }
}
