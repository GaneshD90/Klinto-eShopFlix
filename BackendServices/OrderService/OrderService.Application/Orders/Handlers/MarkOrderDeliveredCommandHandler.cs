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
    public sealed class MarkOrderDeliveredCommandHandler : ICommandHandler<MarkOrderDeliveredCommand, MarkOrderDeliveredResultDto>
    {
        private readonly IOrderShipmentRepository _shipmentRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public MarkOrderDeliveredCommandHandler(
            IOrderShipmentRepository shipmentRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _shipmentRepository = shipmentRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<MarkOrderDeliveredResultDto> Handle(MarkOrderDeliveredCommand command, CancellationToken ct)
        {
            var shipment = await _shipmentRepository.GetByShipmentIdAsync(command.ShipmentId, ct);
            if (shipment is null)
            {
                throw AppException.NotFound("Shipment", $"Shipment {command.ShipmentId} not found.");
            }

            var result = await _shipmentRepository.MarkDeliveredAsync(
                command.ShipmentId,
                command.DeliverySignature,
                command.DeliveryProofImage,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.delivery.failed", "Failed to mark order as delivered.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.delivery.failed", result.Message);
            }

            var order = shipment.Order;
            await _eventPublisher.EnqueueAsync(
                nameof(OrderDeliveredIntegrationEvent),
                new OrderDeliveredIntegrationEvent(
                    shipment.OrderId,
                    order?.OrderNumber ?? string.Empty,
                    order?.CustomerId ?? Guid.Empty,
                    order?.CustomerEmail ?? string.Empty,
                    command.ShipmentId,
                    command.DeliverySignature,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
