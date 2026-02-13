using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Contracts.Messaging;

/// <summary>
/// Extension methods to add Azure Service Bus health checks to services.
/// </summary>
public static class ServiceBusHealthCheckExtensions
{
    /// <summary>
    /// Adds Azure Service Bus health check to the health checks builder.
    /// Falls back to a simple "healthy" check if no connection string is configured.
    /// </summary>
    public static IHealthChecksBuilder AddAzureServiceBusHealthCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string name = "azureservicebus",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        var connectionString = configuration["ServiceBus:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // No Service Bus configured - add a dummy healthy check
            builder.AddCheck(name, () => HealthCheckResult.Healthy("In-memory transport - no Service Bus configured"), tags);
        }
        else
        {
            // Add Azure Service Bus health check
            builder.AddAzureServiceBusQueue(
                connectionString,
                queueName: "health-check-queue",
                name: name,
                failureStatus: failureStatus ?? HealthStatus.Degraded,
                tags: tags);
        }

        return builder;
    }
}
