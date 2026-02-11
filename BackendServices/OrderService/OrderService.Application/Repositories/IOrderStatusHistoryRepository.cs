using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderStatusHistoryRepository
    {
        Task<IReadOnlyList<OrderStatusHistory>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task AddAsync(OrderStatusHistory entry, CancellationToken ct = default);
    }
}
