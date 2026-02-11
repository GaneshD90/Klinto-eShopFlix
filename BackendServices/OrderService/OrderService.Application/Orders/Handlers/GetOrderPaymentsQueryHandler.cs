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
    public sealed class GetOrderPaymentsQueryHandler : IQueryHandler<GetOrderPaymentsQuery, IReadOnlyList<OrderPaymentDto>>
    {
        private readonly IOrderPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public GetOrderPaymentsQueryHandler(IOrderPaymentRepository paymentRepository, IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderPaymentDto>> Handle(GetOrderPaymentsQuery query, CancellationToken ct)
        {
            var items = await _paymentRepository.GetByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderPaymentDto>>(items);
        }
    }
}
