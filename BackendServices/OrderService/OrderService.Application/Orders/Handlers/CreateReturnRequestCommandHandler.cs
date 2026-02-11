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
    public sealed class CreateReturnRequestCommandHandler : ICommandHandler<CreateReturnRequestCommand, CreateReturnRequestResultDto>
    {
        private readonly IOrderReturnRepository _returnRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public CreateReturnRequestCommandHandler(
            IOrderReturnRepository returnRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _returnRepository = returnRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<CreateReturnRequestResultDto> Handle(CreateReturnRequestCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _returnRepository.CreateReturnRequestAsync(
                command.OrderId,
                command.CustomerId,
                command.ReturnType,
                command.ReturnReason,
                command.ReturnItemsJson,
                command.CustomerComments,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.return.create.failed", "Failed to create return request.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.return.create.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(ReturnRequestCreatedIntegrationEvent),
                new ReturnRequestCreatedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    result.ReturnId,
                    result.ReturnNumber,
                    command.ReturnType,
                    command.ReturnReason,
                    DateTime.UtcNow),
                ct);

            return result;
        }
    }
}
