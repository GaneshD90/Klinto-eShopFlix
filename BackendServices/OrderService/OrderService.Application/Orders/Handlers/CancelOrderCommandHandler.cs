using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.IntegrationEvents;
using OrderService.Application.Messaging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, OrderDetailDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IMapper _mapper;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            IOrderStatusHistoryRepository statusHistoryRepository,
            IIntegrationEventPublisher eventPublisher,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
        }

        public async Task<OrderDetailDto> Handle(CancelOrderCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var nonCancellable = new[] { "Cancelled", "Completed", "Delivered", "Shipped" };
            if (Array.Exists(nonCancellable, s => string.Equals(s, order.OrderStatus, StringComparison.OrdinalIgnoreCase)))
            {
                throw AppException.Business("order.cancel.invalid_status",
                    $"Order in status '{order.OrderStatus}' cannot be cancelled.");
            }

            var previousStatus = order.OrderStatus;
            var now = DateTime.UtcNow;

            order.OrderStatus = "Cancelled";
            order.CancelledAt = now;
            order.UpdatedAt = now;

            await _orderRepository.UpdateAsync(order, ct);

            var history = new OrderStatusHistory
            {
                StatusHistoryId = Guid.NewGuid(),
                OrderId = order.OrderId,
                FromStatus = previousStatus,
                ToStatus = "Cancelled",
                ChangedBy = command.CancelledBy,
                ChangeReason = command.CancellationReason,
                Notes = $"Cancellation type: {command.CancellationType}. By: {command.CancelledByType}.",
                IsCustomerNotified = true,
                Timestamp = now
            };

            await _statusHistoryRepository.AddAsync(history, ct);

            await _eventPublisher.EnqueueAsync(
                nameof(OrderCancelledIntegrationEvent),
                new OrderCancelledIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    command.CancellationType,
                    command.CancellationReason,
                    now),
                ct);

            return _mapper.Map<OrderDetailDto>(order);
        }
    }
}
