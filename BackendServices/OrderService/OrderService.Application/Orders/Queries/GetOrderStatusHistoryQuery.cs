using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetOrderStatusHistoryQuery : IQuery<IReadOnlyList<OrderStatusHistoryDto>>
    {
        public Guid OrderId { get; init; }
    }
}
