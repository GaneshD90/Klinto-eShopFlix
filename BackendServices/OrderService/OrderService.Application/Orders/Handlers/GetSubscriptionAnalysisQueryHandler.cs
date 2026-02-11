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
    public sealed class GetSubscriptionAnalysisQueryHandler : IQueryHandler<GetSubscriptionAnalysisQuery, IReadOnlyList<SubscriptionAnalysisDto>>
    {
        private readonly IOrderSubscriptionRepository _subscriptionRepository;
        private readonly IMapper _mapper;

        public GetSubscriptionAnalysisQueryHandler(IOrderSubscriptionRepository subscriptionRepository, IMapper mapper)
        {
            _subscriptionRepository = subscriptionRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<SubscriptionAnalysisDto>> Handle(GetSubscriptionAnalysisQuery query, CancellationToken ct)
        {
            var items = query.CustomerId.HasValue
                ? await _subscriptionRepository.GetSubscriptionAnalysisByCustomerIdAsync(query.CustomerId.Value, ct)
                : await _subscriptionRepository.GetSubscriptionAnalysisAsync(ct);

            return _mapper.Map<IReadOnlyList<SubscriptionAnalysisDto>>(items);
        }
    }
}
