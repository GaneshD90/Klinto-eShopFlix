using CartService.Application.Carts.Commands;
using CartService.Application.Carts.Handlers;
using CartService.Application.EventSourcing;
using CartService.Application.Repositories;
using CartService.Application.Snapshots;
using CartService.Domain.Events;
using FluentAssertions;
using Moq;
using Xunit;

namespace CartService.Application.Tests.Carts;

public class RecalculateTotalsCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSnapshotPolicyTriggers_WritesSnapshot()
    {
        var repo = new Mock<ICartRepository>();
        var store = new Mock<IEventStore>();
        var policy = new Mock<ISnapshotPolicy>();
        var writer = new Mock<ISnapshotWriter>();

        store.Setup(s => s.AppendAsync(It.IsAny<long>(), It.IsAny<IEnumerable<IDomainEvent>>(), "CartService", It.IsAny<CancellationToken>()))
             .ReturnsAsync(5);
        policy.Setup(p => p.ShouldSnapshot(5)).Returns(true);

        var sut = new RecalculateTotalsCommandHandler(repo.Object, store.Object, policy.Object, writer.Object);

        var result = await sut.Handle(new RecalculateTotalsCommand(123), CancellationToken.None);

        result.Should().BeTrue();
        repo.Verify(r => r.RecalculateTotalsAsync(123, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(w => w.WriteAsync(123, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSnapshotPolicyDoesNotTrigger_SkipsSnapshotWrite()
    {
        var repo = new Mock<ICartRepository>();
        var store = new Mock<IEventStore>();
        var policy = new Mock<ISnapshotPolicy>();
        var writer = new Mock<ISnapshotWriter>();

        store.Setup(s => s.AppendAsync(It.IsAny<long>(), It.IsAny<IEnumerable<IDomainEvent>>(), "CartService", It.IsAny<CancellationToken>()))
             .ReturnsAsync(2);
        policy.Setup(p => p.ShouldSnapshot(2)).Returns(false);

        var sut = new RecalculateTotalsCommandHandler(repo.Object, store.Object, policy.Object, writer.Object);

        var result = await sut.Handle(new RecalculateTotalsCommand(456), CancellationToken.None);

        result.Should().BeTrue();
        repo.Verify(r => r.RecalculateTotalsAsync(456, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(w => w.WriteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
