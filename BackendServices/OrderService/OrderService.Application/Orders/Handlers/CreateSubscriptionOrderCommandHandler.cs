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
    public sealed class CreateSubscriptionOrderCommandHandler : ICommandHandler<CreateSubscriptionOrderCommand, CreateSubscriptionResultDto>
    {
        private readonly IOrderSubscriptionRepository _subscriptionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public CreateSubscriptionOrderCommandHandler(
            IOrderSubscriptionRepository subscriptionRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _subscriptionRepository = subscriptionRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<CreateSubscriptionResultDto> Handle(CreateSubscriptionOrderCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _subscriptionRepository.CreateSubscriptionAsync(
                command.OrderId,
                command.CustomerId,
                command.Frequency,
                command.StartDate,
                command.TotalOccurrences,
                command.PaymentMethodId,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.subscription.create.failed", "Failed to create subscription.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.subscription.create.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(SubscriptionCreatedIntegrationEvent),
                new SubscriptionCreatedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    result.SubscriptionId,
                    command.Frequency,
                    command.StartDate,
                    command.TotalOccurrences,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
