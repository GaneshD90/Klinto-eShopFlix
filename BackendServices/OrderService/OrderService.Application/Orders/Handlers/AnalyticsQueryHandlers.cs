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
    public sealed class GetOrderSummariesQueryHandler : IQueryHandler<GetOrderSummariesQuery, IReadOnlyList<OrderSummaryDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderSummariesQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderSummaryDto>> Handle(GetOrderSummariesQuery query, CancellationToken ct)
        {
            var items = await _repo.GetOrderSummariesAsync(ct);
            return _mapper.Map<IReadOnlyList<OrderSummaryDto>>(items);
        }
    }

    public sealed class GetCustomerOrderHistoryQueryHandler : IQueryHandler<GetCustomerOrderHistoryQuery, IReadOnlyList<CustomerOrderHistoryDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetCustomerOrderHistoryQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CustomerOrderHistoryDto>> Handle(GetCustomerOrderHistoryQuery query, CancellationToken ct)
        {
            var items = await _repo.GetCustomerOrderHistoryAsync(ct);
            return _mapper.Map<IReadOnlyList<CustomerOrderHistoryDto>>(items);
        }
    }

    public sealed class GetRevenueAnalysisQueryHandler : IQueryHandler<GetRevenueAnalysisQuery, IReadOnlyList<RevenueAnalysisDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetRevenueAnalysisQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<RevenueAnalysisDto>> Handle(GetRevenueAnalysisQuery query, CancellationToken ct)
        {
            var items = await _repo.GetRevenueAnalysisAsync(ct);
            return _mapper.Map<IReadOnlyList<RevenueAnalysisDto>>(items);
        }
    }

    public sealed class GetDailyOrderMetricsQueryHandler : IQueryHandler<GetDailyOrderMetricsQuery, IReadOnlyList<DailyOrderMetricDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetDailyOrderMetricsQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<DailyOrderMetricDto>> Handle(GetDailyOrderMetricsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetDailyOrderMetricsAsync(ct);
            return _mapper.Map<IReadOnlyList<DailyOrderMetricDto>>(items);
        }
    }

    public sealed class GetPendingOrderActionsQueryHandler : IQueryHandler<GetPendingOrderActionsQuery, IReadOnlyList<PendingOrderActionDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetPendingOrderActionsQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<PendingOrderActionDto>> Handle(GetPendingOrderActionsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetPendingOrderActionsAsync(ct);
            return _mapper.Map<IReadOnlyList<PendingOrderActionDto>>(items);
        }
    }

    public sealed class GetPaymentAnalysisQueryHandler : IQueryHandler<GetPaymentAnalysisQuery, IReadOnlyList<PaymentAnalysisDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetPaymentAnalysisQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<PaymentAnalysisDto>> Handle(GetPaymentAnalysisQuery query, CancellationToken ct)
        {
            var items = await _repo.GetPaymentAnalysisAsync(ct);
            return _mapper.Map<IReadOnlyList<PaymentAnalysisDto>>(items);
        }
    }

    public sealed class GetProductOrderPerformanceQueryHandler : IQueryHandler<GetProductOrderPerformanceQuery, IReadOnlyList<ProductOrderPerformanceDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetProductOrderPerformanceQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ProductOrderPerformanceDto>> Handle(GetProductOrderPerformanceQuery query, CancellationToken ct)
        {
            var items = await _repo.GetProductOrderPerformanceAsync(ct);
            return _mapper.Map<IReadOnlyList<ProductOrderPerformanceDto>>(items);
        }
    }

    public sealed class GetFulfillmentPerformanceQueryHandler : IQueryHandler<GetFulfillmentPerformanceQuery, IReadOnlyList<FulfillmentPerformanceDto>>
    {
        private readonly IOrderAnalyticsRepository _repo;
        private readonly IMapper _mapper;

        public GetFulfillmentPerformanceQueryHandler(IOrderAnalyticsRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<FulfillmentPerformanceDto>> Handle(GetFulfillmentPerformanceQuery query, CancellationToken ct)
        {
            var items = await _repo.GetFulfillmentPerformanceAsync(ct);
            return _mapper.Map<IReadOnlyList<FulfillmentPerformanceDto>>(items);
        }
    }
}
