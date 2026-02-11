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
    public sealed class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, OrderDetailDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStatusHistoryRepository _statusHistoryRepository;
        private readonly IOrderTimelineRepository _timelineRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IMapper _mapper;

        public UpdateOrderStatusCommandHandler(
            IOrderRepository orderRepository,
            IOrderStatusHistoryRepository statusHistoryRepository,
            IOrderTimelineRepository timelineRepository,
            IIntegrationEventPublisher eventPublisher,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _timelineRepository = timelineRepository;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
        }

        public async Task<OrderDetailDto> Handle(UpdateOrderStatusCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            if (string.Equals(order.OrderStatus, command.NewStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.status.unchanged",
                    $"Order is already in status '{order.OrderStatus}'.");
            }

            var previousStatus = order.OrderStatus;
            var now = DateTime.UtcNow;

            order.OrderStatus = command.NewStatus;
            order.UpdatedAt = now;

            if (string.Equals(command.NewStatus, "Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                order.ConfirmedAt = now;
            }
            else if (string.Equals(command.NewStatus, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                order.CompletedAt = now;
            }
            else if (string.Equals(command.NewStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                order.CancelledAt = now;
            }

            await _orderRepository.UpdateAsync(order, ct);

            var history = new OrderStatusHistory
            {
                StatusHistoryId = Guid.NewGuid(),
                OrderId = order.OrderId,
                FromStatus = previousStatus,
                ToStatus = command.NewStatus,
                ChangedBy = command.ChangedBy,
                ChangeReason = command.ChangeReason,
                Notes = command.Notes,
                IsCustomerNotified = command.NotifyCustomer,
                Timestamp = now
            };

            await _statusHistoryRepository.AddAsync(history, ct);

            var timeline = new OrderTimeline
            {
                TimelineId = Guid.NewGuid(),
                OrderId = order.OrderId,
                EventType = "StatusChanged",
                EventDescription = $"Order status changed from '{previousStatus}' to '{command.NewStatus}'.",
                EventDate = now,
                IsVisibleToCustomer = command.NotifyCustomer,
                Icon = "status-change"
            };

            await _timelineRepository.AddAsync(timeline, ct);

            await _eventPublisher.EnqueueAsync(
                nameof(OrderStatusChangedIntegrationEvent),
                new OrderStatusChangedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    previousStatus,
                    command.NewStatus,
                    command.ChangedBy,
                    command.ChangeReason,
                    now),
                ct);

            return _mapper.Map<OrderDetailDto>(order);
        }
    }
}
