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
    public sealed class GetOrdersByCustomerQueryHandler : IQueryHandler<GetOrdersByCustomerQuery, PagedResult<OrderListItemDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<OrderListItemDto>> Handle(GetOrdersByCustomerQuery query, CancellationToken ct)
        {
            var items = await _orderRepository.GetByCustomerIdAsync(query.CustomerId, query.Page, query.PageSize, ct);
            var total = await _orderRepository.GetCustomerOrderCountAsync(query.CustomerId, ct);
            var mapped = _mapper.Map<IReadOnlyList<OrderListItemDto>>(items);
            return new PagedResult<OrderListItemDto>(mapped, total, query.Page, query.PageSize);
        }
    }
}
