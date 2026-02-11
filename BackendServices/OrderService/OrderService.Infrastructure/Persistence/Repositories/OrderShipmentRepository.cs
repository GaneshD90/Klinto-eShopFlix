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
    public sealed class OrderShipmentRepository : IOrderShipmentRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderShipmentRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateShipmentResultDto?> CreateShipmentAsync(
            Guid orderId,
            Guid? warehouseId,
            string shippingMethod,
            string carrierName,
            string shipmentItemsJson,
            CancellationToken ct = default)
        {
            var shipmentIdParam = new OutputParameter<Guid?>();
            var results = await _dbContext.Procedures.SP_CreateShipmentAsync(
                orderId,
                warehouseId,
                shippingMethod,
                carrierName,
                shipmentItemsJson,
                shipmentIdParam,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new CreateShipmentResultDto
            {
                ShipmentId = result.ShipmentId ?? shipmentIdParam.Value,
                ShipmentNumber = result.ShipmentNumber ?? string.Empty,
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<ShipOrderResultDto?> ShipOrderAsync(
            Guid shipmentId,
            string trackingNumber,
            string trackingUrl,
            DateTime? estimatedDeliveryDate,
            CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_ShipOrderAsync(
                shipmentId,
                trackingNumber,
                trackingUrl,
                estimatedDeliveryDate,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new ShipOrderResultDto
            {
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<MarkOrderDeliveredResultDto?> MarkDeliveredAsync(
            Guid shipmentId,
            string deliverySignature,
            string deliveryProofImage,
            CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_MarkOrderDeliveredAsync(
                shipmentId,
                deliverySignature,
                deliveryProofImage,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new MarkOrderDeliveredResultDto
            {
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<OrderShipment?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderShipment>()
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, ct);
        }

        public async Task<IReadOnlyList<OrderShipment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderShipment>()
                .Include(s => s.OrderShipmentItems)
                .Where(s => s.OrderId == orderId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwShipmentTracking>> GetShipmentTrackingByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.VwShipmentTrackings
                .Where(t => t.OrderId == orderId)
                .OrderByDescending(t => t.ShippedAt)
                .ToListAsync(ct);
        }
    }
}
