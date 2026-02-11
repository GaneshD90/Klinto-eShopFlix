using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetOrderByIdQuery : IQuery<OrderDetailDto?>
    {
        public Guid OrderId { get; init; }
    }
}
