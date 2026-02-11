using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
        Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
        Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchAsync(
            string? term,
            Guid? customerId,
            string? orderStatus,
            string? paymentStatus,
            string? fulfillmentStatus,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize,
            CancellationToken ct = default);
        Task<IReadOnlyList<Order>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken ct = default);
        Task<int> GetCustomerOrderCountAsync(Guid customerId, CancellationToken ct = default);
        Task AddAsync(Order order, CancellationToken ct = default);
        Task UpdateAsync(Order order, CancellationToken ct = default);
    }
}
