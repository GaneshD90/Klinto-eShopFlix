using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderShipmentRepository
    {
        Task<CreateShipmentResultDto?> CreateShipmentAsync(
            Guid orderId,
            Guid? warehouseId,
            string shippingMethod,
            string carrierName,
            string shipmentItemsJson,
            CancellationToken ct = default);

        Task<ShipOrderResultDto?> ShipOrderAsync(
            Guid shipmentId,
            string trackingNumber,
            string trackingUrl,
            DateTime? estimatedDeliveryDate,
            CancellationToken ct = default);

        Task<MarkOrderDeliveredResultDto?> MarkDeliveredAsync(
            Guid shipmentId,
            string deliverySignature,
            string deliveryProofImage,
            CancellationToken ct = default);

        Task<OrderShipment?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default);

        Task<IReadOnlyList<OrderShipment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

        Task<IReadOnlyList<VwShipmentTracking>> GetShipmentTrackingByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
