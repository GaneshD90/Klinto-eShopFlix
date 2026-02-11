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
    public sealed class AddOrderItemsCommandHandler : ICommandHandler<AddOrderItemsCommand, AddOrderItemsResultDto>
    {
        private readonly IOrderItemRepository _itemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public AddOrderItemsCommandHandler(
            IOrderItemRepository itemRepository,
            IOrderRepository orderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _itemRepository = itemRepository;
            _orderRepository = orderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<AddOrderItemsResultDto> Handle(AddOrderItemsCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var result = await _itemRepository.AddOrderItemsAsync(
                command.OrderId,
                command.OrderItemsJson,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.items.add.failed", "Failed to add order items.");
            }

            if (string.Equals(result.Status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.items.add.failed", result.Message);
            }

            await _eventPublisher.EnqueueAsync(
                nameof(OrderItemsAddedIntegrationEvent),
                new OrderItemsAddedIntegrationEvent(
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
