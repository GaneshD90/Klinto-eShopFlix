using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Orders.Queries;
using OrderService.Application.Repositories;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDetailDto?>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<OrderDetailDto?> Handle(GetOrderByIdQuery query, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(query.OrderId, ct);
            return order is null ? null : _mapper.Map<OrderDetailDto>(order);
        }
    }
}
