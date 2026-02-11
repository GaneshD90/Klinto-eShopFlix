using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public sealed class OrderSubscriptionRepository : IOrderSubscriptionRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderSubscriptionRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateSubscriptionResultDto?> CreateSubscriptionAsync(
            Guid orderId,
            Guid customerId,
            string frequency,
            DateTime? startDate,
            int? totalOccurrences,
            Guid? paymentMethodId,
            CancellationToken ct = default)
        {
            var subscriptionIdParam = new OutputParameter<Guid?>();
            var results = await _dbContext.Procedures.SP_CreateSubscriptionOrderAsync(
                orderId,
                customerId,
                frequency,
                startDate,
                totalOccurrences,
                paymentMethodId,
                subscriptionIdParam,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new CreateSubscriptionResultDto
            {
                SubscriptionId = result.SubscriptionId ?? subscriptionIdParam.Value,
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<ProcessNextSubscriptionResultDto?> ProcessNextSubscriptionAsync(
            Guid subscriptionId,
            CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_ProcessNextSubscriptionOrderAsync(
                subscriptionId,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new ProcessNextSubscriptionResultDto
            {
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<VwSubscriptionAnalysis>> GetSubscriptionAnalysisAsync(CancellationToken ct = default)
        {
            return await _dbContext.VwSubscriptionAnalyses
                .OrderByDescending(s => s.StartDate)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwSubscriptionAnalysis>> GetSubscriptionAnalysisByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
        {
            return await _dbContext.VwSubscriptionAnalyses
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync(ct);
        }
    }
}
