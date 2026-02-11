using AutoMapper;
using FluentAssertions;
using Moq;
using OrderService.Application.IntegrationEvents;
using OrderService.Application.Mappers;
using OrderService.Application.Messaging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Orders.Handlers;
using OrderService.Application.Exceptions;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Application.Tests.Orders;

public class UpdateOrderStatusCommandHandlerTests
{
    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderMapper>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsNotFound()
    {
        var repo = new Mock<IOrderRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var handler = new UpdateOrderStatusCommandHandler(
            repo.Object,
            Mock.Of<IOrderStatusHistoryRepository>(),
            Mock.Of<IOrderTimelineRepository>(),
            Mock.Of<IIntegrationEventPublisher>(),
            CreateMapper());

        var act = () => handler.Handle(new UpdateOrderStatusCommand { OrderId = Guid.NewGuid(), NewStatus = "Confirmed" }, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.StatusCode == 404);
    }

    [Fact]
    public async Task Handle_WhenStatusUnchanged_ThrowsBusiness()
    {
        var order = new Order { OrderId = Guid.NewGuid(), OrderStatus = "Pending", OrderNumber = "ORD-1" };
        var repo = new Mock<IOrderRepository>();
        repo.Setup(r => r.GetByIdAsync(order.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new UpdateOrderStatusCommandHandler(
            repo.Object,
            Mock.Of<IOrderStatusHistoryRepository>(),
            Mock.Of<IOrderTimelineRepository>(),
            Mock.Of<IIntegrationEventPublisher>(),
            CreateMapper());

        var act = () => handler.Handle(new UpdateOrderStatusCommand { OrderId = order.OrderId, NewStatus = "Pending" }, CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .Where(ex => ex.StatusCode == 422 && ex.Type != null && ex.Type.EndsWith("order.status.unchanged"));
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesOrderAndRecordsHistory()
    {
        var repo = new Mock<IOrderRepository>();
        var historyRepo = new Mock<IOrderStatusHistoryRepository>();
        var timelineRepo = new Mock<IOrderTimelineRepository>();
        var publisher = new Mock<IIntegrationEventPublisher>();
        var mapper = CreateMapper();

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = "ORD-2",
            OrderStatus = "Pending",
            OrderDate = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        repo.Setup(r => r.GetByIdAsync(order.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new UpdateOrderStatusCommandHandler(
            repo.Object,
            historyRepo.Object,
            timelineRepo.Object,
            publisher.Object,
            mapper);

        var result = await handler.Handle(new UpdateOrderStatusCommand
        {
            OrderId = order.OrderId,
            NewStatus = "Confirmed",
            ChangedBy = Guid.NewGuid(),
            ChangeReason = "Payment captured",
            Notes = "Auto-update",
            NotifyCustomer = true
        }, CancellationToken.None);

        result.OrderStatus.Should().Be("Confirmed");
        result.ConfirmedAt.Should().NotBeNull();
        result.CompletedAt.Should().BeNull();
        result.CancelledAt.Should().BeNull();

        repo.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        historyRepo.Verify(h => h.AddAsync(It.Is<OrderStatusHistory>(h => h.OrderId == order.OrderId && h.ToStatus == "Confirmed"), It.IsAny<CancellationToken>()), Times.Once);
        timelineRepo.Verify(t => t.AddAsync(It.Is<OrderTimeline>(t => t.OrderId == order.OrderId && t.EventType == "StatusChanged"), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.EnqueueAsync(nameof(OrderStatusChangedIntegrationEvent), It.IsAny<OrderStatusChangedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
