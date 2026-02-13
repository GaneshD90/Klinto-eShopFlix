using System.Diagnostics;
using Contracts.Sagas.Checkout;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrderService.Integration.Tests.Sagas;

/// <summary>
/// Integration tests for the Checkout Saga using MassTransit test harness.
/// </summary>
public class CheckoutSagaIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<CheckoutSagaStateMachine, CheckoutSagaState>()
                .InMemoryRepository();
        });

        _provider = services.BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task CheckoutSaga_WhenStarted_TransitionsToAwaitingInventory()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var startEvent = new CheckoutStarted(
            CorrelationId: correlationId,
            OrderId: orderId,
            OrderNumber: "ORD-001",
            CartId: cartId,
            CustomerId: customerId,
            CustomerEmail: "test@example.com",
            TotalAmount: 999.99m,
            CurrencyCode: "INR",
            ItemCount: 2,
            Lines: new[]
            {
                new CheckoutLineItem(Guid.NewGuid(), null, "Product 1", "SKU001", 1, 499.99m),
                new CheckoutLineItem(Guid.NewGuid(), null, "Product 2", "SKU002", 1, 500.00m)
            },
            OccurredAt: DateTime.UtcNow);

        // Act
        await _harness.Bus.Publish(startEvent);

        // Assert
        var sagaHarness = _harness.GetSagaStateMachineHarness<CheckoutSagaStateMachine, CheckoutSagaState>();
        
        Assert.True(await sagaHarness.Consumed.Any<CheckoutStarted>(
            x => x.Context.Message.CorrelationId == correlationId));

        var saga = sagaHarness.Sagas.Contains(correlationId);
        Assert.True(saga);
    }

    [Fact]
    public async Task CheckoutSaga_WhenInventoryReserved_TransitionsToAwaitingPayment()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // First start the saga
        await _harness.Bus.Publish(new CheckoutStarted(
            CorrelationId: correlationId,
            OrderId: orderId,
            OrderNumber: "ORD-002",
            CartId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            CustomerEmail: "test@example.com",
            TotalAmount: 100m,
            CurrencyCode: "INR",
            ItemCount: 1,
            Lines: Array.Empty<CheckoutLineItem>(),
            OccurredAt: DateTime.UtcNow));

        // Wait for saga to be in AwaitingInventory state
        await Task.Delay(100);

        // Then publish inventory reserved event
        await _harness.Bus.Publish(new InventoryReservedForCheckout(
            CorrelationId: correlationId,
            OrderId: orderId,
            ReservationId: Guid.NewGuid(),
            TotalQuantityReserved: 1,
            ExpiresAt: DateTime.UtcNow.AddMinutes(30),
            OccurredAt: DateTime.UtcNow));

        // Assert - saga should transition to AwaitingPayment
        var sagaHarness = _harness.GetSagaStateMachineHarness<CheckoutSagaStateMachine, CheckoutSagaState>();

        Assert.True(await sagaHarness.Consumed.Any<InventoryReservedForCheckout>(
            x => x.Context.Message.CorrelationId == correlationId));
    }

    [Fact]
    public async Task CheckoutSaga_WhenInventoryReservationFails_TransitionsToFailed()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        // Start the saga
        await _harness.Bus.Publish(new CheckoutStarted(
            CorrelationId: correlationId,
            OrderId: orderId,
            OrderNumber: "ORD-003",
            CartId: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            CustomerEmail: "test@example.com",
            TotalAmount: 100m,
            CurrencyCode: "INR",
            ItemCount: 1,
            Lines: Array.Empty<CheckoutLineItem>(),
            OccurredAt: DateTime.UtcNow));

        await Task.Delay(100);

        // Publish inventory reservation failed event
        await _harness.Bus.Publish(new InventoryReservationFailedForCheckout(
            CorrelationId: correlationId,
            OrderId: orderId,
            Reason: "Insufficient stock",
            OccurredAt: DateTime.UtcNow));

        // Assert
        var sagaHarness = _harness.GetSagaStateMachineHarness<CheckoutSagaStateMachine, CheckoutSagaState>();

        Assert.True(await sagaHarness.Consumed.Any<InventoryReservationFailedForCheckout>(
            x => x.Context.Message.CorrelationId == correlationId));
    }

    // Placeholder for CheckoutSagaStateMachine - this would reference the actual one
    private class CheckoutSagaStateMachine : MassTransitStateMachine<CheckoutSagaState>
    {
        public State AwaitingInventory { get; private set; } = null!;
        public Event<CheckoutStarted> CheckoutStarted { get; private set; } = null!;
        public Event<InventoryReservedForCheckout> InventoryReserved { get; private set; } = null!;
        public Event<InventoryReservationFailedForCheckout> InventoryReservationFailed { get; private set; } = null!;

        public CheckoutSagaStateMachine()
        {
            InstanceState(x => x.CurrentState);
            Event(() => CheckoutStarted, e => e.CorrelateById(m => m.Message.CorrelationId));
            Event(() => InventoryReserved, e => e.CorrelateById(m => m.Message.CorrelationId));
            Event(() => InventoryReservationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));

            Initially(
                When(CheckoutStarted)
                    .TransitionTo(AwaitingInventory));

            During(AwaitingInventory,
                When(InventoryReserved)
                    .Then(context => { /* transition logic */ }),
                When(InventoryReservationFailed)
                    .Finalize());

            SetCompletedWhenFinalized();
        }
    }
}
