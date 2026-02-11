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
    public sealed class GetOrderRefundsQueryHandler : IQueryHandler<GetOrderRefundsQuery, IReadOnlyList<OrderRefundDto>>
    {
        private readonly IOrderRefundRepository _refundRepository;
        private readonly IMapper _mapper;

        public GetOrderRefundsQueryHandler(IOrderRefundRepository refundRepository, IMapper mapper)
        {
            _refundRepository = refundRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderRefundDto>> Handle(GetOrderRefundsQuery query, CancellationToken ct)
        {
            var items = await _refundRepository.GetByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderRefundDto>>(items);
        }
    }
}
