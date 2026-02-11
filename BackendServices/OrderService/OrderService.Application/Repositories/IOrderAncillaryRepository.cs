using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderAncillaryRepository
    {
        Task<IReadOnlyList<OrderDiscount>> GetDiscountsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderTaxis>> GetTaxesByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderAdjustment>> GetAdjustmentsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderGiftCard>> GetGiftCardsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderLoyaltyPoint>> GetLoyaltyPointsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderDocument>> GetDocumentsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<OrderMetric?> GetMetricByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderItemOption>> GetItemOptionsByOrderItemIdAsync(Guid orderItemId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderFulfillmentAssignment>> GetFulfillmentAssignmentsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderCancellation>> GetCancellationsByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
