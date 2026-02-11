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
    public sealed class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, ProcessPaymentResultDto>
    {
        private readonly IOrderPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public ProcessPaymentCommandHandler(
            IOrderPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<ProcessPaymentResultDto> Handle(ProcessPaymentCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _paymentRepository.ProcessPaymentAsync(
                command.OrderId,
                command.PaymentMethod,
                command.PaymentProvider,
                command.Amount,
                command.TransactionId,
                command.AuthorizationCode,
                command.PaymentGatewayResponse,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.payment.failed", "Failed to process payment.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.payment.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(OrderPaymentProcessedIntegrationEvent),
                new OrderPaymentProcessedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    result.PaymentId,
                    command.PaymentMethod,
                    command.PaymentProvider,
                    command.Amount,
                    command.TransactionId,
                    result.Status,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
