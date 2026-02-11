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
    public sealed class GetOrderItemsQueryHandler : IQueryHandler<GetOrderItemsQuery, IReadOnlyList<OrderItemDto>>
    {
        private readonly IOrderItemRepository _itemRepository;
        private readonly IMapper _mapper;

        public GetOrderItemsQueryHandler(IOrderItemRepository itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderItemDto>> Handle(GetOrderItemsQuery query, CancellationToken ct)
        {
            var items = await _itemRepository.GetByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderItemDto>>(items);
        }
    }
}
