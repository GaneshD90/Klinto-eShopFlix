using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Messaging.ServiceBus;

namespace Contracts.Messaging;

/// <summary>
/// Background service that monitors dead-letter queues and handles failed messages.
/// Can be registered in services that need DLQ monitoring.
/// </summary>
public class DeadLetterQueueMonitorService : BackgroundService
{
    private readonly ILogger<DeadLetterQueueMonitorService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _serviceName;
    private ServiceBusClient? _serviceBusClient;
    private ServiceBusProcessor? _dlqProcessor;

    public DeadLetterQueueMonitorService(
        ILogger<DeadLetterQueueMonitorService> logger,
        IConfiguration configuration,
        string serviceName)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceName = serviceName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogInformation(
                "[{ServiceName}] No Service Bus connection string configured. DLQ monitoring disabled.",
                _serviceName);
            return;
        }

        try
        {
            _serviceBusClient = new ServiceBusClient(connectionString);

            // Monitor the service's primary queue DLQ
            var queueName = $"{_serviceName.ToLower()}-queue/$deadletterqueue";
            
            _dlqProcessor = _serviceBusClient.CreateProcessor(
                queueName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1,
                    PrefetchCount = 10
                });

            _dlqProcessor.ProcessMessageAsync += ProcessDeadLetterMessageAsync;
            _dlqProcessor.ProcessErrorAsync += ProcessErrorAsync;

            await _dlqProcessor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation(
                "[{ServiceName}] Dead-letter queue monitoring started for {QueueName}",
                _serviceName, queueName);

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[{ServiceName}] Error in DLQ monitoring service",
                _serviceName);
        }
    }

    private async Task ProcessDeadLetterMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        
        _logger.LogWarning(
            "[{ServiceName}] Dead-letter message received. " +
            "MessageId={MessageId}, DeadLetterReason={DeadLetterReason}, " +
            "DeadLetterErrorDescription={DeadLetterErrorDescription}, " +
            "EnqueuedTime={EnqueuedTime}, DeliveryCount={DeliveryCount}",
            _serviceName,
            message.MessageId,
            message.DeadLetterReason,
            message.DeadLetterErrorDescription,
            message.EnqueuedTime,
            message.DeliveryCount);

        try
        {
            // Log message body for debugging
            var body = message.Body.ToString();
            _logger.LogWarning(
                "[{ServiceName}] DLQ Message Body (first 1000 chars): {Body}",
                _serviceName,
                body.Length > 1000 ? body[..1000] : body);

            // In a full implementation:
            // 1. Store the message in a database for manual review
            // 2. Send alerts (email, Slack, etc.)
            // 3. Attempt automatic retry for certain error types
            // 4. Create a support ticket

            // Complete the message to remove it from DLQ
            await args.CompleteMessageAsync(message);

            _logger.LogInformation(
                "[{ServiceName}] DLQ message processed and logged. MessageId={MessageId}",
                _serviceName, message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[{ServiceName}] Error processing DLQ message. MessageId={MessageId}",
                _serviceName, message.MessageId);
            
            // Abandon the message so it stays in DLQ for retry
            await args.AbandonMessageAsync(message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "[{ServiceName}] Error in DLQ processor. ErrorSource={ErrorSource}, EntityPath={EntityPath}",
            _serviceName,
            args.ErrorSource,
            args.EntityPath);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_dlqProcessor != null)
        {
            await _dlqProcessor.StopProcessingAsync(cancellationToken);
            await _dlqProcessor.DisposeAsync();
        }

        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Model for storing dead-letter messages for review.
/// </summary>
public record DeadLetterMessageRecord(
    string MessageId,
    string ServiceName,
    string QueueName,
    string? DeadLetterReason,
    string? DeadLetterErrorDescription,
    string MessageBody,
    string? CorrelationId,
    DateTime EnqueuedTimeUtc,
    int DeliveryCount,
    DateTime RecordedAtUtc,
    string Status // Pending, Reviewed, Retried, Archived
);
