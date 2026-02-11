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
    public sealed class PlaceOrderOnHoldCommandHandler : ICommandHandler<PlaceOrderOnHoldCommand, PlaceOrderOnHoldResultDto>
    {
        private readonly IOrderHoldRepository _holdRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public PlaceOrderOnHoldCommandHandler(
            IOrderHoldRepository holdRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _holdRepository = holdRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<PlaceOrderOnHoldResultDto> Handle(PlaceOrderOnHoldCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _holdRepository.PlaceOnHoldAsync(
                command.OrderId,
                command.HoldType,
                command.HoldReason,
                command.PlacedBy,
                command.ExpiresAt,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.hold.failed", "Failed to place order on hold.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.hold.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(OrderPlacedOnHoldIntegrationEvent),
                new OrderPlacedOnHoldIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    command.HoldType,
                    command.HoldReason,
                    command.PlacedBy,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
