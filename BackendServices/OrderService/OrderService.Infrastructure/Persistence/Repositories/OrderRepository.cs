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
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderAddresses)
                .FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderAddresses)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        }

        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchAsync(
            string? term,
            Guid? customerId,
            string? orderStatus,
            string? paymentStatus,
            string? fulfillmentStatus,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize;

            var query = _dbContext.Orders
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var lowered = term.ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(lowered) ||
                    o.CustomerEmail.ToLower().Contains(lowered));
            }

            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(orderStatus))
            {
                query = query.Where(o => o.OrderStatus == orderStatus);
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                query = query.Where(o => o.PaymentStatus == paymentStatus);
            }

            if (!string.IsNullOrWhiteSpace(fulfillmentStatus))
            {
                query = query.Where(o => o.FulfillmentStatus == fulfillmentStatus);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken ct = default)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize;

            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> GetCustomerOrderCountAsync(Guid customerId, CancellationToken ct = default)
        {
            return await _dbContext.Orders
                .CountAsync(o => o.CustomerId == customerId, ct);
        }

        public async Task AddAsync(Order order, CancellationToken ct = default)
        {
            await _dbContext.Orders.AddAsync(order, ct);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct = default)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
