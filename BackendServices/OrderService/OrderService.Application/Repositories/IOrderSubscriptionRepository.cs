using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderSubscriptionRepository
    {
        Task<CreateSubscriptionResultDto?> CreateSubscriptionAsync(
            Guid orderId,
            Guid customerId,
            string frequency,
            DateTime? startDate,
            int? totalOccurrences,
            Guid? paymentMethodId,
            CancellationToken ct = default);

        Task<ProcessNextSubscriptionResultDto?> ProcessNextSubscriptionAsync(
            Guid subscriptionId,
            CancellationToken ct = default);

        Task<IReadOnlyList<VwSubscriptionAnalysis>> GetSubscriptionAnalysisAsync(CancellationToken ct = default);

        Task<IReadOnlyList<VwSubscriptionAnalysis>> GetSubscriptionAnalysisByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    }
}
