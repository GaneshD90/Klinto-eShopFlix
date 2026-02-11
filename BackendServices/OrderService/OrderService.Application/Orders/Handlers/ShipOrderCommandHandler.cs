using System;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.IntegrationEvents;
using OrderService.Application.Messaging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Repositories;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class ShipOrderCommandHandler : ICommandHandler<ShipOrderCommand, ShipOrderResultDto>
    {
        private readonly IOrderShipmentRepository _shipmentRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public ShipOrderCommandHandler(
            IOrderShipmentRepository shipmentRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _shipmentRepository = shipmentRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<ShipOrderResultDto> Handle(ShipOrderCommand command, CancellationToken ct)
        {
            var shipment = await _shipmentRepository.GetByShipmentIdAsync(command.ShipmentId, ct);
            if (shipment is null)
            {
                throw AppException.NotFound("Shipment", $"Shipment {command.ShipmentId} not found.");
            }

            var result = await _shipmentRepository.ShipOrderAsync(
                command.ShipmentId,
                command.TrackingNumber,
                command.TrackingUrl,
                command.EstimatedDeliveryDate,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.ship.failed", "Failed to ship order.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.ship.failed", result.Message);
            }

            var order = shipment.Order;
            await _eventPublisher.EnqueueAsync(
                nameof(OrderShippedIntegrationEvent),
                new OrderShippedIntegrationEvent(
                    shipment.OrderId,
                    order?.OrderNumber ?? string.Empty,
                    order?.CustomerId ?? Guid.Empty,
                    order?.CustomerEmail ?? string.Empty,
                    command.ShipmentId,
                    command.TrackingNumber,
                    command.TrackingUrl,
                    command.EstimatedDeliveryDate,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
