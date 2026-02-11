using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderReturnRepository
    {
        Task<CreateReturnRequestResultDto?> CreateReturnRequestAsync(
            Guid orderId,
            Guid customerId,
            string returnType,
            string returnReason,
            string returnItemsJson,
            string customerComments,
            CancellationToken ct = default);

        Task<IReadOnlyList<OrderReturn>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

        Task<IReadOnlyList<VwReturnManagement>> GetReturnManagementByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
