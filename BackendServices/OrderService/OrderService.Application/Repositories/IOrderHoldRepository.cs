using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderHoldRepository
    {
        Task<PlaceOrderOnHoldResultDto?> PlaceOnHoldAsync(
            Guid orderId,
            string holdType,
            string holdReason,
            Guid? placedBy,
            DateTime? expiresAt,
            CancellationToken ct = default);

        Task<ReleaseOrderHoldResultDto?> ReleaseHoldAsync(
            Guid holdId,
            Guid? releasedBy,
            string notes,
            CancellationToken ct = default);

        Task<IReadOnlyList<VwOrderHoldManagement>> GetHoldManagementByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
