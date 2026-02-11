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
    public sealed class ProcessNextSubscriptionCommandHandler : ICommandHandler<ProcessNextSubscriptionCommand, ProcessNextSubscriptionResultDto>
    {
        private readonly IOrderSubscriptionRepository _subscriptionRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public ProcessNextSubscriptionCommandHandler(
            IOrderSubscriptionRepository subscriptionRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _subscriptionRepository = subscriptionRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<ProcessNextSubscriptionResultDto> Handle(ProcessNextSubscriptionCommand command, CancellationToken ct)
        {
            var result = await _subscriptionRepository.ProcessNextSubscriptionAsync(
                command.SubscriptionId,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.subscription.process.failed", "Failed to process next subscription order.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.subscription.process.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(SubscriptionNextOrderProcessedIntegrationEvent),
                new SubscriptionNextOrderProcessedIntegrationEvent(
                    command.SubscriptionId,
                    result.Status,
                    result.Message,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
