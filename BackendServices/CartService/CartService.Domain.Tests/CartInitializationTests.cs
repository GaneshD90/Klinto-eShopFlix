using CartService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CartService.Domain.Tests;

public class CartInitializationTests
{
    [Fact]
    public void NewCart_ShouldInitializeCollections()
    {
        var cart = new Cart();

        cart.CartAdjustments.Should().NotBeNull().And.BeEmpty();
        cart.CartCoupons.Should().NotBeNull().And.BeEmpty();
        cart.CartEvents.Should().NotBeNull().And.BeEmpty();
        cart.CartItems.Should().NotBeNull().And.BeEmpty();
        cart.CartPayments.Should().NotBeNull().And.BeEmpty();
        cart.CartShipments.Should().NotBeNull().And.BeEmpty();
        cart.CartTaxes.Should().NotBeNull().And.BeEmpty();
        cart.SavedForLaterItems.Should().NotBeNull().And.BeEmpty();
    }
}
