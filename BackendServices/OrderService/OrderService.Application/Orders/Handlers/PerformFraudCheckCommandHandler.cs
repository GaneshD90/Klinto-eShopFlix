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
    public sealed class PerformFraudCheckCommandHandler : ICommandHandler<PerformFraudCheckCommand, PerformFraudCheckResultDto>
    {
        private readonly IOrderFraudCheckRepository _fraudCheckRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public PerformFraudCheckCommandHandler(
            IOrderFraudCheckRepository fraudCheckRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _fraudCheckRepository = fraudCheckRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<PerformFraudCheckResultDto> Handle(PerformFraudCheckCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _fraudCheckRepository.PerformFraudCheckAsync(
                command.OrderId,
                command.FraudProvider,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.fraudcheck.failed", "Failed to perform fraud check.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.fraudcheck.failed", $"Fraud check failed with status: {result.Status}");
            }

            await _eventPublisher.EnqueueAsync(
                nameof(FraudCheckPerformedIntegrationEvent),
                new FraudCheckPerformedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    result.FraudCheckId,
                    result.RiskScore,
                    result.RiskLevel,
                    result.RecommendedAction,
                    result.Status,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
