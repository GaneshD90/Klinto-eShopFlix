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
    public sealed class GetOrderTimelineQueryHandler : IQueryHandler<GetOrderTimelineQuery, IReadOnlyList<OrderTimelineDto>>
    {
        private readonly IOrderTimelineRepository _timelineRepository;
        private readonly IMapper _mapper;

        public GetOrderTimelineQueryHandler(IOrderTimelineRepository timelineRepository, IMapper mapper)
        {
            _timelineRepository = timelineRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderTimelineDto>> Handle(GetOrderTimelineQuery query, CancellationToken ct)
        {
            var items = await _timelineRepository.GetByOrderIdAsync(query.OrderId, query.CustomerVisibleOnly, ct);
            return _mapper.Map<IReadOnlyList<OrderTimelineDto>>(items);
        }
    }
}
