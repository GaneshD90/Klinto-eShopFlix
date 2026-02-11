using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetOrdersByCustomerQuery : IQuery<PagedResult<OrderListItemDto>>
    {
        public Guid CustomerId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
