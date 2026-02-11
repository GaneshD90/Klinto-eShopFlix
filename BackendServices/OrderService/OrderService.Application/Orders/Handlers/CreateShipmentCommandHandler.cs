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
    public sealed class CreateShipmentCommandHandler : ICommandHandler<CreateShipmentCommand, CreateShipmentResultDto>
    {
        private readonly IOrderShipmentRepository _shipmentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public CreateShipmentCommandHandler(
            IOrderShipmentRepository shipmentRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _shipmentRepository = shipmentRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<CreateShipmentResultDto> Handle(CreateShipmentCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _shipmentRepository.CreateShipmentAsync(
                command.OrderId,
                command.WarehouseId,
                command.ShippingMethod,
                command.CarrierName,
                command.ShipmentItemsJson,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.shipment.create.failed", "Failed to create shipment.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.shipment.create.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(ShipmentCreatedIntegrationEvent),
                new ShipmentCreatedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    result.ShipmentId,
                    result.ShipmentNumber,
                    command.ShippingMethod,
                    command.CarrierName,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
