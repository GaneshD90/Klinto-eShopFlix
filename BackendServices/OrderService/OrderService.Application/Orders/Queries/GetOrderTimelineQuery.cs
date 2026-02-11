using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetOrderTimelineQuery : IQuery<IReadOnlyList<OrderTimelineDto>>
    {
        public Guid OrderId { get; init; }
        public bool CustomerVisibleOnly { get; init; }
    }
}
