using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Messaging;

/// <summary>
/// Shared MassTransit configuration used by all eShopFlix microservices.
/// Provides a consistent way to configure Azure Service Bus transport
/// with in-memory fallback for local development.
/// </summary>
public static class MassTransitConfiguration
{
    /// <summary>
    /// Adds MassTransit with Azure Service Bus (or in-memory fallback) to the service collection.
    /// Each service calls this from its Program.cs or ServiceRegistration, passing a consumer
    /// registration action for service-specific consumers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">App configuration (reads ServiceBus:ConnectionString).</param>
    /// <param name="configureConsumers">
    /// Optional action to register service-specific consumers/saga state machines.
    /// Called inside AddMassTransit so consumers are wired before transport is configured.
    /// </param>
    public static IServiceCollection AddEShopFlixMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(bus =>
        {
            // Let each service register its own consumers
            configureConsumers?.Invoke(bus);

            // Use kebab-case endpoint naming for consistent topic/queue names
            bus.SetKebabCaseEndpointNameFormatter();

            var connectionString = configuration["ServiceBus:ConnectionString"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                bus.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(connectionString);

                    // Auto-configure endpoints for all registered consumers
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                // In-memory transport for local development without Azure Service Bus
                bus.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }

    /// <summary>
    /// Adds MassTransit with full configuration control for services that need
    /// EntityFramework Outbox or other advanced features.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">App configuration.</param>
    /// <param name="configureBus">Full bus configuration action.</param>
    public static IServiceCollection AddEShopFlixMessagingAdvanced(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator, IConfiguration> configureBus)
    {
        services.AddMassTransit(bus =>
        {
            configureBus(bus, configuration);
        });

        return services;
    }
}
