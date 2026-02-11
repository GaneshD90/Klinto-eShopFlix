using AutoMapper;
using CatalogService.Application.DTO;
using CatalogService.Application.PriceHistory.Commands;
using CatalogService.Application.PriceHistory.Handlers;
using CatalogService.Application.Repositories;
using FluentAssertions;
using Moq;
using PriceHistoryEntity = CatalogService.Domain.Entities.PriceHistory;
using Xunit;

namespace CatalogService.Application.Tests.PriceHistory.Handlers;

public class RecordPriceChangeCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenMissingIdentifiers_Throws()
    {
        var repository = new Mock<IPriceHistoryRepository>();
        var mapper = new Mock<IMapper>();
        var handler = new RecordPriceChangeCommandHandler(repository.Object, mapper.Object);
        var command = new RecordPriceChangeCommand
        {
            ProductId = null,
            SkuId = null,
            NewPrice = 100
        };

        var action = async () => await handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenProductIdProvided_RecordsEntryAndMapsDto()
    {
        var repository = new Mock<IPriceHistoryRepository>();
        var mapper = new Mock<IMapper>();
        PriceHistoryEntity? captured = null;

        repository
            .Setup(r => r.RecordAsync(It.IsAny<PriceHistoryEntity>(), It.IsAny<CancellationToken>()))
            .Callback<PriceHistoryEntity, CancellationToken>((entry, _) => captured = entry)
            .Returns(Task.CompletedTask);

        var expectedDto = new PriceHistoryEntryDto
        {
            ProductId = 10,
            NewPrice = 99,
            Currency = "USD",
            ChangedBy = "tester"
        };

        mapper.Setup(m => m.Map<PriceHistoryEntryDto>(It.IsAny<PriceHistoryEntity>()))
            .Returns(expectedDto);

        var handler = new RecordPriceChangeCommandHandler(repository.Object, mapper.Object);
        var command = new RecordPriceChangeCommand
        {
            ProductId = 10,
            NewPrice = 99,
            Currency = "USD",
            ChangedBy = "tester"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ProductId.Should().Be(10);
        captured.NewPrice.Should().Be(99);
        captured.Currency.Should().Be("USD");
        captured.ChangedBy.Should().Be("tester");
        result.Should().Be(expectedDto);
    }
}
