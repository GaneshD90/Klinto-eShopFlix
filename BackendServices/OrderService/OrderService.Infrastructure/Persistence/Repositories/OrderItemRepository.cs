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
    public sealed class OrderItemRepository : IOrderItemRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderItemRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AddOrderItemsResultDto?> AddOrderItemsAsync(
            Guid orderId,
            string orderItemsJson,
            CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_AddOrderItemsAsync(
                orderId,
                orderItemsJson,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new AddOrderItemsResultDto
            {
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderItem>()
                .Where(i => i.OrderId == orderId)
                .OrderBy(i => i.CreatedAt)
                .ToListAsync(ct);
        }
    }
}
