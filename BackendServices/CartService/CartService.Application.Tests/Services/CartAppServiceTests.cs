using CartService.Application.CQRS;
using CartService.Application.Carts.Commands;
using CartService.Application.DTOs;
using CartService.Application.Exceptions;
using CartService.Application.HttpClients;
using CartService.Application.Repositories;
using CartService.Application.Services.Implementations;
using CartService.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CartService.Application.Tests.Services;

public class CartAppServiceTests
{
    private static CartAppService CreateSut(Mock<IDispatcher> dispatcher, Mock<ICartRepository>? repo = null)
    {
        IConfiguration configuration = Mock.Of<IConfiguration>();
        var catalog = new CatalogServiceClient(new HttpClient(new FakeHandler()), Mock.Of<ILogger<CatalogServiceClient>>());

        return new CartAppService(
            repo?.Object ?? Mock.Of<ICartRepository>(),
            Mock.Of<AutoMapper.IMapper>(),
            configuration,
            catalog,
            dispatcher.Object);
    }

    [Fact]
    public async Task AddItem_WhenQuantityIsInvalid_ThrowsValidation()
    {
        var dispatcher = new Mock<IDispatcher>();
        var sut = CreateSut(dispatcher);
        var item = new CartItem { ItemId = 10, Quantity = 0, UnitPrice = 9.99m };

        var act = () => sut.AddItem(5, item);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        ex.Which.Errors.Should().ContainKey(nameof(item.Quantity));
        dispatcher.Verify(d => d.Send(It.IsAny<AddItemCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddItem_WhenValid_DispatchesCommand()
    {
        var dispatcher = new Mock<IDispatcher>();
        var expected = new CartDTO { Id = 2, UserId = 5, CartItems = new List<CartItemDTO>() };
        dispatcher.Setup(d => d.Send(It.IsAny<AddItemCommand>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(expected);

        var sut = CreateSut(dispatcher);
        var item = new CartItem { ItemId = 20, Quantity = 3, UnitPrice = 4.5m };

        var result = await sut.AddItem(5, item);

        result.Should().BeSameAs(expected);
        dispatcher.Verify(d => d.Send(It.Is<AddItemCommand>(cmd => cmd.UserId == 5 && cmd.Item == item), It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });
    }
}
