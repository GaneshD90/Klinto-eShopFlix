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
    public sealed class GetOrderHoldsQueryHandler : IQueryHandler<GetOrderHoldsQuery, IReadOnlyList<OrderHoldManagementDto>>
    {
        private readonly IOrderHoldRepository _holdRepository;
        private readonly IMapper _mapper;

        public GetOrderHoldsQueryHandler(IOrderHoldRepository holdRepository, IMapper mapper)
        {
            _holdRepository = holdRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderHoldManagementDto>> Handle(GetOrderHoldsQuery query, CancellationToken ct)
        {
            var items = await _holdRepository.GetHoldManagementByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderHoldManagementDto>>(items);
        }
    }
}
