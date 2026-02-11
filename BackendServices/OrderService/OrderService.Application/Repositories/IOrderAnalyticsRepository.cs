using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderAnalyticsRepository
    {
        Task<IReadOnlyList<VwOrderSummary>> GetOrderSummariesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VwCustomerOrderHistory>> GetCustomerOrderHistoryAsync(CancellationToken ct = default);
        Task<VwCustomerOrderHistory?> GetCustomerOrderHistoryByIdAsync(Guid customerId, CancellationToken ct = default);
        Task<IReadOnlyList<VwRevenueAnalysis>> GetRevenueAnalysisAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VwDailyOrderMetric>> GetDailyOrderMetricsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VwPendingOrdersAction>> GetPendingOrderActionsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VwPaymentAnalysis>> GetPaymentAnalysisAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VwProductOrderPerformance>> GetProductOrderPerformanceAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VwFulfillmentPerformance>> GetFulfillmentPerformanceAsync(CancellationToken ct = default);
    }
}
