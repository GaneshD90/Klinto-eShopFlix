using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderNoteRepository
    {
        Task<IReadOnlyList<OrderNote>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task AddAsync(OrderNote note, CancellationToken ct = default);
    }
}
