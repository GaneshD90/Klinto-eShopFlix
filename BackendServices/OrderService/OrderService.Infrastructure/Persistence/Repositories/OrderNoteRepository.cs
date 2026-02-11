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
    public sealed class OrderNoteRepository : IOrderNoteRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderNoteRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<OrderNote>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderNote>()
                .Where(n => n.OrderId == orderId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task AddAsync(OrderNote note, CancellationToken ct = default)
        {
            await _dbContext.Set<OrderNote>().AddAsync(note, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
