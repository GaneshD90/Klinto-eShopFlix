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
    public sealed class GetOrderStatusHistoryQueryHandler : IQueryHandler<GetOrderStatusHistoryQuery, IReadOnlyList<OrderStatusHistoryDto>>
    {
        private readonly IOrderStatusHistoryRepository _historyRepository;
        private readonly IMapper _mapper;

        public GetOrderStatusHistoryQueryHandler(IOrderStatusHistoryRepository historyRepository, IMapper mapper)
        {
            _historyRepository = historyRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderStatusHistoryDto>> Handle(GetOrderStatusHistoryQuery query, CancellationToken ct)
        {
            var items = await _historyRepository.GetByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderStatusHistoryDto>>(items);
        }
    }
}
