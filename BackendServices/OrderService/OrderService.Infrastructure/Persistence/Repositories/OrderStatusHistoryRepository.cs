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
    public sealed class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderStatusHistoryRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderStatusHistory>()
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync(ct);
        }

        public async Task AddAsync(OrderStatusHistory entry, CancellationToken ct = default)
        {
            await _dbContext.Set<OrderStatusHistory>().AddAsync(entry, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
