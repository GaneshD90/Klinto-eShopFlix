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
    public sealed class OrderTimelineRepository : IOrderTimelineRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderTimelineRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<OrderTimeline>> GetByOrderIdAsync(Guid orderId, bool customerVisibleOnly = false, CancellationToken ct = default)
        {
            var query = _dbContext.Set<OrderTimeline>()
                .Where(t => t.OrderId == orderId);

            if (customerVisibleOnly)
            {
                query = query.Where(t => t.IsVisibleToCustomer);
            }

            return await query
                .OrderByDescending(t => t.EventDate)
                .ToListAsync(ct);
        }

        public async Task AddAsync(OrderTimeline entry, CancellationToken ct = default)
        {
            await _dbContext.Set<OrderTimeline>().AddAsync(entry, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
