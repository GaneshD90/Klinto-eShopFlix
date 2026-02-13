using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Contracts.Messaging;

/// <summary>
/// Extension methods for adding OpenTelemetry tracing with MassTransit instrumentation.
/// </summary>
public static class OpenTelemetryMassTransitExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing with MassTransit instrumentation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service for resource attribution.</param>
    /// <param name="serviceVersion">The version of the service.</param>
    /// <param name="otlpEndpoint">Optional OTLP endpoint for exporting traces.</param>
    public static IServiceCollection AddEShopFlixTelemetry(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion = "1.0.0",
        string? otlpEndpoint = null)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = ctx =>
                        {
                            // Filter out health check endpoints
                            var path = ctx.Request.Path.Value ?? string.Empty;
                            return !path.Contains("/health", StringComparison.OrdinalIgnoreCase);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                    })
                    // Add MassTransit instrumentation
                    .AddSource("MassTransit");

                // Add OTLP exporter if endpoint is configured
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
                else
                {
                    // Console exporter for development
                    tracing.AddConsoleExporter();
                }
            });

        return services;
    }

    /// <summary>
    /// Adds saga-specific activity tags for tracing.
    /// </summary>
    public static class SagaActivityTags
    {
        public const string SagaType = "saga.type";
        public const string SagaState = "saga.state";
        public const string SagaCorrelationId = "saga.correlation_id";
        public const string SagaOrderId = "saga.order_id";
        public const string SagaOrderNumber = "saga.order_number";
        public const string SagaCustomerId = "saga.customer_id";
        public const string SagaStep = "saga.step";
        public const string SagaIsCompensation = "saga.is_compensation";
    }
}
