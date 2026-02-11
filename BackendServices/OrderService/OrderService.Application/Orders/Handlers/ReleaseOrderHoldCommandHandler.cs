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
    public sealed class ReleaseOrderHoldCommandHandler : ICommandHandler<ReleaseOrderHoldCommand, ReleaseOrderHoldResultDto>
    {
        private readonly IOrderHoldRepository _holdRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public ReleaseOrderHoldCommandHandler(
            IOrderHoldRepository holdRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _holdRepository = holdRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<ReleaseOrderHoldResultDto> Handle(ReleaseOrderHoldCommand command, CancellationToken ct)
        {
            var result = await _holdRepository.ReleaseHoldAsync(
                command.HoldId,
                command.ReleasedBy,
                command.Notes,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.hold.release.failed", "Failed to release order hold.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.hold.release.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(OrderHoldReleasedIntegrationEvent),
                new OrderHoldReleasedIntegrationEvent(
                    Guid.Empty,
                    string.Empty,
                    Guid.Empty,
                    string.Empty,
                    command.HoldId,
                    command.ReleasedBy,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
