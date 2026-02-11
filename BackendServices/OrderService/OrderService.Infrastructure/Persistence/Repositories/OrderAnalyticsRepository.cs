using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public sealed class OrderAnalyticsRepository : IOrderAnalyticsRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderAnalyticsRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<VwOrderSummary>> GetOrderSummariesAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwOrderSummaries
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwCustomerOrderHistory>> GetCustomerOrderHistoryAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwCustomerOrderHistories
                .OrderByDescending(c => c.TotalSpent)
                .ToListAsync(ct);
        }

        public async Task<VwCustomerOrderHistory?> GetCustomerOrderHistoryByIdAsync(Guid customerId, CancellationToken ct = default)
        {
            return await _dbContext.VwCustomerOrderHistories
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
        }

        public async Task<IReadOnlyList<VwRevenueAnalysis>> GetRevenueAnalysisAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwRevenueAnalyses
                .OrderByDescending(r => r.OrderYear)
                .ThenByDescending(r => r.OrderMonth)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwDailyOrderMetric>> GetDailyOrderMetricsAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwDailyOrderMetrics
                .OrderByDescending(d => d.OrderDay)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwPendingOrdersAction>> GetPendingOrderActionsAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwPendingOrdersActions
                .OrderByDescending(p => p.HoursPending)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwPaymentAnalysis>> GetPaymentAnalysisAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwPaymentAnalyses
                .OrderByDescending(p => p.TotalAmount)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwProductOrderPerformance>> GetProductOrderPerformanceAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwProductOrderPerformances
                .OrderByDescending(p => p.TotalRevenue)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwFulfillmentPerformance>> GetFulfillmentPerformanceAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwFulfillmentPerformances
                .OrderByDescending(f => f.FulfillmentRate)
                .ToListAsync(ct);
        }
    }
}
