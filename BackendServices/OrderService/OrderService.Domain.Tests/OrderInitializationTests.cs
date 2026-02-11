using FluentAssertions;
using OrderService.Domain.Entities;
using Xunit;

namespace OrderService.Domain.Tests;

public class OrderInitializationTests
{
    [Fact]
    public void NewOrder_ShouldInitializeCollections()
    {
        var order = new Order();

        order.OrderAddresses.Should().NotBeNull().And.BeEmpty();
        order.OrderAdjustments.Should().NotBeNull().And.BeEmpty();
        order.OrderCancellations.Should().NotBeNull().And.BeEmpty();
        order.OrderDiscounts.Should().NotBeNull().And.BeEmpty();
        order.OrderDocuments.Should().NotBeNull().And.BeEmpty();
        order.OrderFraudChecks.Should().NotBeNull().And.BeEmpty();
        order.OrderFulfillmentAssignments.Should().NotBeNull().And.BeEmpty();
        order.OrderGiftCards.Should().NotBeNull().And.BeEmpty();
        order.OrderHolds.Should().NotBeNull().And.BeEmpty();
        order.OrderItems.Should().NotBeNull().And.BeEmpty();
        order.OrderLoyaltyPoints.Should().NotBeNull().And.BeEmpty();
        order.OrderMetrics.Should().NotBeNull().And.BeEmpty();
        order.OrderNotes.Should().NotBeNull().And.BeEmpty();
        order.OrderPayments.Should().NotBeNull().And.BeEmpty();
        order.OrderRefunds.Should().NotBeNull().And.BeEmpty();
        order.OrderReturns.Should().NotBeNull().And.BeEmpty();
        order.OrderShipments.Should().NotBeNull().And.BeEmpty();
        order.OrderStatusHistories.Should().NotBeNull().And.BeEmpty();
        order.OrderSubscriptions.Should().NotBeNull().And.BeEmpty();
        order.OrderTaxes.Should().NotBeNull().And.BeEmpty();
        order.OrderTimelines.Should().NotBeNull().And.BeEmpty();
    }
}
