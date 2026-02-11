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
    public sealed class ProcessRefundCommandHandler : ICommandHandler<ProcessRefundCommand, ProcessRefundResultDto>
    {
        private readonly IOrderRefundRepository _refundRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public ProcessRefundCommandHandler(
            IOrderRefundRepository refundRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _refundRepository = refundRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<ProcessRefundResultDto> Handle(ProcessRefundCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _refundRepository.ProcessRefundAsync(
                command.OrderId,
                command.ReturnId,
                command.RefundAmount,
                command.RefundType,
                command.RefundMethod,
                command.RefundReason,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.refund.failed", "Failed to process refund.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.refund.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(RefundProcessedIntegrationEvent),
                new RefundProcessedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    result.RefundId,
                    result.RefundNumber,
                    command.RefundAmount,
                    command.RefundType,
                    command.RefundMethod,
                    command.RefundReason,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
