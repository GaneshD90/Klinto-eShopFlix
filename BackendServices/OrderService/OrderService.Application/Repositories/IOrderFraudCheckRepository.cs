using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderFraudCheckRepository
    {
        Task<PerformFraudCheckResultDto?> PerformFraudCheckAsync(
            Guid orderId,
            string fraudProvider,
            CancellationToken ct = default);

        Task<IReadOnlyList<VwFraudRiskDashboard>> GetFraudRiskDashboardByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
