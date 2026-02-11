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
    public sealed class OrderAncillaryRepository : IOrderAncillaryRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderAncillaryRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<OrderDiscount>> GetDiscountsByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderDiscount>()
                .Where(d => d.OrderId == orderId)
                .OrderBy(d => d.AppliedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderTaxis>> GetTaxesByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderTaxis>()
                .Where(t => t.OrderId == orderId)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderAdjustment>> GetAdjustmentsByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderAdjustment>()
                .Where(a => a.OrderId == orderId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderGiftCard>> GetGiftCardsByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderGiftCard>()
                .Where(g => g.OrderId == orderId)
                .OrderBy(g => g.AppliedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderLoyaltyPoint>> GetLoyaltyPointsByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderLoyaltyPoint>()
                .Where(l => l.OrderId == orderId)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderDocument>> GetDocumentsByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderDocument>()
                .Where(d => d.OrderId == orderId)
                .OrderByDescending(d => d.GeneratedAt)
                .ToListAsync(ct);
        }

        public async Task<OrderMetric?> GetMetricByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderMetric>()
                .FirstOrDefaultAsync(m => m.OrderId == orderId, ct);
        }

        public async Task<IReadOnlyList<OrderItemOption>> GetItemOptionsByOrderItemIdAsync(Guid orderItemId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderItemOption>()
                .Where(o => o.OrderItemId == orderItemId)
                .OrderBy(o => o.OptionName)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderFulfillmentAssignment>> GetFulfillmentAssignmentsByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderFulfillmentAssignment>()
                .Where(f => f.OrderId == orderId)
                .OrderBy(f => f.AssignedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OrderCancellation>> GetCancellationsByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderCancellation>()
                .Where(c => c.OrderId == orderId)
                .OrderByDescending(c => c.CancelledAt)
                .ToListAsync(ct);
        }
    }
}
