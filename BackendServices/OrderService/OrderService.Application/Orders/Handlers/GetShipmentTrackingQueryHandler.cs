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
    public sealed class GetShipmentTrackingQueryHandler : IQueryHandler<GetShipmentTrackingQuery, IReadOnlyList<ShipmentTrackingDto>>
    {
        private readonly IOrderShipmentRepository _shipmentRepository;
        private readonly IMapper _mapper;

        public GetShipmentTrackingQueryHandler(IOrderShipmentRepository shipmentRepository, IMapper mapper)
        {
            _shipmentRepository = shipmentRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ShipmentTrackingDto>> Handle(GetShipmentTrackingQuery query, CancellationToken ct)
        {
            var items = await _shipmentRepository.GetShipmentTrackingByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<ShipmentTrackingDto>>(items);
        }
    }
}
