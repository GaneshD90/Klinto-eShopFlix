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
    public sealed class ConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand, ConfirmOrderResultDto>
    {
        private readonly IOrderConfirmRepository _confirmRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public ConfirmOrderCommandHandler(
            IOrderConfirmRepository confirmRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _confirmRepository = confirmRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<ConfirmOrderResultDto> Handle(ConfirmOrderCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _confirmRepository.ConfirmAsync(command.OrderId, ct);
            if (result is null)
            {
                throw AppException.Business("order.confirm.failed", "Failed to confirm order.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.confirm.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(OrderConfirmedIntegrationEvent),
                new OrderConfirmedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
