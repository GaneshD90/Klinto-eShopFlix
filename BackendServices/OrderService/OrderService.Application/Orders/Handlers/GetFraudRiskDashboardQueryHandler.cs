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
    public sealed class GetFraudRiskDashboardQueryHandler : IQueryHandler<GetFraudRiskDashboardQuery, IReadOnlyList<FraudRiskDashboardDto>>
    {
        private readonly IOrderFraudCheckRepository _fraudCheckRepository;
        private readonly IMapper _mapper;

        public GetFraudRiskDashboardQueryHandler(IOrderFraudCheckRepository fraudCheckRepository, IMapper mapper)
        {
            _fraudCheckRepository = fraudCheckRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<FraudRiskDashboardDto>> Handle(GetFraudRiskDashboardQuery query, CancellationToken ct)
        {
            var items = await _fraudCheckRepository.GetFraudRiskDashboardByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<FraudRiskDashboardDto>>(items);
        }
    }
}
