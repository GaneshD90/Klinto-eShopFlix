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
    public sealed class OrderHoldRepository : IOrderHoldRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderHoldRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlaceOrderOnHoldResultDto?> PlaceOnHoldAsync(
            Guid orderId,
            string holdType,
            string holdReason,
            Guid? placedBy,
            DateTime? expiresAt,
            CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_PlaceOrderOnHoldAsync(
                orderId,
                holdType,
                holdReason,
                placedBy,
                expiresAt,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new PlaceOrderOnHoldResultDto
            {
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<ReleaseOrderHoldResultDto?> ReleaseHoldAsync(
            Guid holdId,
            Guid? releasedBy,
            string notes,
            CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_ReleaseOrderHoldAsync(
                holdId,
                releasedBy,
                notes,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new ReleaseOrderHoldResultDto
            {
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<VwOrderHoldManagement>> GetHoldManagementByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.VwOrderHoldManagements
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.PlacedAt)
                .ToListAsync(ct);
        }
    }
}
