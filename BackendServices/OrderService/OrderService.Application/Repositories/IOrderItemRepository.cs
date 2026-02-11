using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderItemRepository
    {
        Task<AddOrderItemsResultDto?> AddOrderItemsAsync(
            Guid orderId,
            string orderItemsJson,
            CancellationToken ct = default);

        Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
