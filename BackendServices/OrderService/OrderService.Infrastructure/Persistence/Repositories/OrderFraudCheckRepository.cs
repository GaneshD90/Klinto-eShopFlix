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
    public sealed class OrderFraudCheckRepository : IOrderFraudCheckRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderFraudCheckRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PerformFraudCheckResultDto?> PerformFraudCheckAsync(
            Guid orderId,
            string fraudProvider,
            CancellationToken ct = default)
        {
            var fraudCheckIdParam = new OutputParameter<Guid?>();
            var results = await _dbContext.Procedures.SP_PerformFraudCheckAsync(
                orderId,
                fraudProvider,
                fraudCheckIdParam,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new PerformFraudCheckResultDto
            {
                FraudCheckId = result.FraudCheckId ?? fraudCheckIdParam.Value,
                RiskScore = result.RiskScore,
                RiskLevel = result.RiskLevel ?? string.Empty,
                RecommendedAction = result.RecommendedAction ?? string.Empty,
                Status = result.Status ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<VwFraudRiskDashboard>> GetFraudRiskDashboardByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.VwFraudRiskDashboards
                .Where(f => f.OrderId == orderId)
                .OrderByDescending(f => f.CheckedAt)
                .ToListAsync(ct);
        }
    }
}
