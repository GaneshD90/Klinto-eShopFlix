using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Orders.Queries;
using OrderService.Application.Repositories;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class SearchOrdersQueryHandler : IQueryHandler<SearchOrdersQuery, PagedResult<OrderListItemDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public SearchOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<OrderListItemDto>> Handle(SearchOrdersQuery query, CancellationToken ct)
        {
            var (items, total) = await _orderRepository.SearchAsync(
                query.Term,
                query.CustomerId,
                query.OrderStatus,
                query.PaymentStatus,
                query.FulfillmentStatus,
                query.FromDate,
                query.ToDate,
                query.Page,
                query.PageSize,
                ct);

            var mapped = _mapper.Map<IReadOnlyList<OrderListItemDto>>(items);
            return new PagedResult<OrderListItemDto>(mapped, total, query.Page, query.PageSize);
        }
    }
}
