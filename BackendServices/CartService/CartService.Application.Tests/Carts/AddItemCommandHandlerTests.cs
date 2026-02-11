using AutoMapper;
using CartService.Application.Carts.Commands;
using CartService.Application.Carts.Handlers;
using CartService.Application.DTOs;
using CartService.Application.EventSourcing;
using CartService.Application.HttpClients;
using CartService.Application.Repositories;
using CartService.Domain.Entities;
using CartService.Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace CartService.Application.Tests.Carts;

public class AddItemCommandHandlerTests
{
    private static IMapper BuildMapper()
    {
        var mapper = new Mock<IMapper>();

        mapper.Setup(m => m.Map<CartDTO>(It.IsAny<Cart>()))
            .Returns((Cart c) => new CartDTO
            {
                Id = c.Id,
                UserId = c.UserId,
                CartItems = new List<CartItemDTO>()
            });

        mapper.Setup(m => m.Map<List<CartItemDTO>>(It.IsAny<IEnumerable<CartItem>>()))
            .Returns((IEnumerable<CartItem> items) => items
                .Select(i => new CartItemDTO
                {
                    Id = i.Id,
                    CartId = i.CartId,
                    ItemId = i.ItemId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList());

        return mapper.Object;
    }

    [Fact]
    public async Task Handle_AppendsEvent_EnrichesSnapshotAndReturnsDto()
    {
        var repo = new Mock<ICartRepository>();
        var store = new Mock<IEventStore>();
        var catalog = CreateCatalogClient();
        var mapper = BuildMapper();

        var cart = new Cart { Id = 7, UserId = 42, CurrencyCode = "USD", CartItems = new List<CartItem>() };
        var item = new CartItem
        {
            CartId = cart.Id,
            ItemId = 101,
            Quantity = 2,
            UnitPrice = 15m,
            Sku = "SKU-101",
            ProductName = "Fallback name"
        };

        repo.Setup(r => r.AddItem(cart.UserId, item)).ReturnsAsync(cart);
        repo.Setup(r => r.GetSnapshotAsync(cart.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new CartSnapshotDto
        {
            Cart = new List<Cart> { cart },
            Items = new List<CartItem> { item },
            Totals = new List<CartTotal>
            {
                new()
                {
                    CartId = cart.Id,
                    Subtotal = item.UnitPrice * item.Quantity,
                    TaxTotal = 5m,
                    GrandTotal = item.UnitPrice * item.Quantity + 5m
                }
            }
        });

        IDomainEvent? capturedEvent = null;
        store.Setup(s => s.AppendAsync(cart.Id, It.IsAny<IEnumerable<IDomainEvent>>(), "CartService", It.IsAny<CancellationToken>()))
             .Callback<long, IEnumerable<IDomainEvent>, string?, CancellationToken>((_, events, _, _) => capturedEvent = events.Single())
             .ReturnsAsync(3);

        var sut = new AddItemCommandHandler(repo.Object, store.Object, catalog, mapper);

        var result = await sut.Handle(new AddItemCommand(cart.UserId, item), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(cart.Id);
        result.CartItems.Should().HaveCount(1);
        result.CartItems[0].Name.Should().Be("Catalog Product");
        result.CartItems[0].ImageUrl.Should().Be("https://cdn/img.png");
        result.Total.Should().Be(item.UnitPrice * item.Quantity);
        result.GrandTotal.Should().Be(item.UnitPrice * item.Quantity + 5m);

        capturedEvent.Should().BeOfType<ItemAddedV1>();
        var evt = (ItemAddedV1)capturedEvent!;
        evt.CartId.Should().Be(cart.Id);
        evt.UserId.Should().Be(cart.UserId);
        evt.ItemId.Should().Be(item.ItemId);
        evt.Quantity.Should().Be(item.Quantity);
        evt.UnitPrice.Should().Be(item.UnitPrice);
        evt.Sku.Should().Be(item.Sku);
    }

    private static CatalogServiceClient CreateCatalogClient()
    {
        var products = new[]
        {
            new ProductDTO
            {
                ProductId = 101,
                Name = "Catalog Product",
                ImageUrl = "https://cdn/img.png"
            }
        };

        var handler = new FakeHandler(JsonSerializer.Serialize(products));
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://catalog.local/")
        };

        return new CatalogServiceClient(client, Mock.Of<ILogger<CatalogServiceClient>>());
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _response;

        public FakeHandler(string response = "[]") => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_response)
            });
    }
}
