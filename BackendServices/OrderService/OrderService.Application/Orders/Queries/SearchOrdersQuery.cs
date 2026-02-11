using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class SearchOrdersQuery : IQuery<PagedResult<OrderListItemDto>>
    {
        public string? Term { get; init; }
        public Guid? CustomerId { get; init; }
        public string? OrderStatus { get; init; }
        public string? PaymentStatus { get; init; }
        public string? FulfillmentStatus { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
