using Contracts.Messaging;
using Contracts.Sagas.Cancellation;
using Contracts.Sagas.Checkout;
using Contracts.Sagas.ReturnRefund;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Infrastructure.Messaging.Consumers;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Sagas;

namespace OrderService.Infrastructure.Messaging;

/// <summary>
/// MassTransit + Azure Service Bus registration for OrderService.
/// Includes:
/// - Checkout Saga State Machine (orchestration)
/// - Cancellation Saga State Machine (orchestration)
/// - Return/Refund Saga State Machine (orchestration)
/// - Saga command consumers (participants)
/// - Event consumers (choreography)
/// </summary>
public static class MassTransitRegistration
{
    public static IServiceCollection AddOrderServiceMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEShopFlixMessaging(configuration, bus =>
        {
            // ============ SAGA 1: Checkout Saga ============
            bus.AddSagaStateMachine<CheckoutSagaStateMachine, CheckoutSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<DbContext, OrderServiceDbContext>((provider, builder) =>
                    {
                        var connectionString = configuration.GetConnectionString("DbConnection");
                        builder.UseSqlServer(connectionString, m =>
                        {
                            m.MigrationsAssembly(typeof(OrderServiceDbContext).Assembly.GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(OrderServiceDbContext)}");
                        });
                    });
                });

            // ============ SAGA 2: Cancellation Saga ============
            bus.AddSagaStateMachine<CancellationSagaStateMachine, CancellationSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<DbContext, OrderServiceDbContext>((provider, builder) =>
                    {
                        var connectionString = configuration.GetConnectionString("DbConnection");
                        builder.UseSqlServer(connectionString, m =>
                        {
                            m.MigrationsAssembly(typeof(OrderServiceDbContext).Assembly.GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(OrderServiceDbContext)}");
                        });
                    });
                });

            // ============ SAGA 3: Return/Refund Saga ============
            bus.AddSagaStateMachine<ReturnRefundSagaStateMachine, ReturnRefundSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<DbContext, OrderServiceDbContext>((provider, builder) =>
                    {
                        var connectionString = configuration.GetConnectionString("DbConnection");
                        builder.UseSqlServer(connectionString, m =>
                        {
                            m.MigrationsAssembly(typeof(OrderServiceDbContext).Assembly.GetName().Name);
                            m.MigrationsHistoryTable($"__{nameof(OrderServiceDbContext)}");
                        });
                    });
                });

            // ============ Saga Command Consumers (Participants) ============
            bus.AddConsumer<ConfirmOrderForCheckoutConsumer>();
            bus.AddConsumer<CancelOrderForCheckoutConsumer>();
            bus.AddConsumer<FinalizeOrderCancellationConsumer>();
            bus.AddConsumer<ValidateReturnRequestConsumer>();
            bus.AddConsumer<FinalizeReturnConsumer>();

            // ============ Event Consumers (Choreography) ============
            bus.AddConsumer<PaymentAuthorizedConsumer>();
            bus.AddConsumer<PaymentFailedConsumer>();
            bus.AddConsumer<InventoryReservationFailedConsumer>();
            bus.AddConsumer<InventoryReservedConsumer>();

            // ============ User Event Consumers ============
            bus.AddConsumer<UserUpdatedConsumer>();
            bus.AddConsumer<UserDeletedConsumer>();
        });

        return services;
    }
}
