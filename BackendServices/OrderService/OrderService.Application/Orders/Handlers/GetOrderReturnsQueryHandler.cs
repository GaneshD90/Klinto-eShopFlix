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
    public sealed class GetOrderReturnsQueryHandler : IQueryHandler<GetOrderReturnsQuery, IReadOnlyList<OrderReturnDto>>
    {
        private readonly IOrderReturnRepository _returnRepository;
        private readonly IMapper _mapper;

        public GetOrderReturnsQueryHandler(IOrderReturnRepository returnRepository, IMapper mapper)
        {
            _returnRepository = returnRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderReturnDto>> Handle(GetOrderReturnsQuery query, CancellationToken ct)
        {
            var items = await _returnRepository.GetByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderReturnDto>>(items);
        }
    }
}
