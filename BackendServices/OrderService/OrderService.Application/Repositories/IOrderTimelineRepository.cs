using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderTimelineRepository
    {
        Task<IReadOnlyList<OrderTimeline>> GetByOrderIdAsync(Guid orderId, bool customerVisibleOnly = false, CancellationToken ct = default);
        Task AddAsync(OrderTimeline entry, CancellationToken ct = default);
    }
}
