using System.Text.Json;
using Contracts.DTOs;
using Contracts.Events.V2;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging;

/// <summary>
/// Background service that polls the outbox table and publishes messages via MassTransit.
/// Transforms internal integration events to shared contract events for saga orchestration.
/// </summary>
public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxDispatcherHostedService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);
    private readonly int _batchSize = 25;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OutboxDispatcherHostedService(
        IServiceProvider serviceProvider,
        ILogger<OutboxDispatcherHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderService OutboxDispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OutboxDispatcher loop");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("OrderService OutboxDispatcher stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderServiceDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // Get unprocessed messages
        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null && m.RetryCount < 5)
            .OrderBy(m => m.OccurredAt)
            .Take(_batchSize)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            try
            {
                await PublishMessageAsync(publishEndpoint, message, ct);

                // Mark as processed
                message.ProcessedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Published outbox message {MessageId} of type {EventType}",
                    message.OutboxMessageId, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish outbox message {MessageId}, retry {RetryCount}",
                    message.OutboxMessageId, message.RetryCount);

                message.RetryCount++;
                message.LastError = ex.Message;
                await dbContext.SaveChangesAsync(ct);
            }
        }
    }

    private async Task PublishMessageAsync(
        IPublishEndpoint publishEndpoint,
        OutboxMessage message,
        CancellationToken ct)
    {
        // Transform internal events to shared contract events
        switch (message.EventType)
        {
            case "OrderCreatedIntegrationEvent":
                await PublishOrderCreatedAsync(publishEndpoint, message.Payload, ct);
                break;

            case "OrderFromCartCreatedIntegrationEvent":
                await PublishOrderFromCartCreatedAsync(publishEndpoint, message.Payload, ct);
                break;

            case "OrderConfirmedIntegrationEvent":
                await PublishOrderConfirmedAsync(publishEndpoint, message.Payload, ct);
                break;

            case "OrderCancelledIntegrationEvent":
                await PublishOrderCancelledAsync(publishEndpoint, message.Payload, ct);
                break;

            case "OrderStatusChangedIntegrationEvent":
                await PublishOrderStatusChangedAsync(publishEndpoint, message.Payload, ct);
                break;

            case "OrderPaymentProcessedIntegrationEvent":
                await PublishOrderPaymentProcessedAsync(publishEndpoint, message.Payload, ct);
                break;

            default:
                _logger.LogWarning("Unknown event type: {EventType}", message.EventType);
                break;
        }
    }

    private async Task PublishOrderCreatedAsync(IPublishEndpoint publisher, string payload, CancellationToken ct)
    {
        var internalEvent = JsonSerializer.Deserialize<InternalOrderCreatedEvent>(payload, JsonOptions);
        if (internalEvent is null) return;

        // For orders created directly (not from cart), we need to fetch order items
        // For now, publish with empty lines - the consumer will handle this
        var sharedEvent = new OrderCreatedV2(
            OrderId: internalEvent.OrderId,
            OrderNumber: internalEvent.OrderNumber,
            CustomerId: internalEvent.CustomerId,
            CustomerEmail: internalEvent.CustomerEmail,
            OrderType: internalEvent.OrderType,
            OrderSource: internalEvent.OrderSource,
            TotalAmount: internalEvent.TotalAmount,
            CurrencyCode: internalEvent.CurrencyCode,
            ItemCount: internalEvent.ItemCount,
            CartId: null,
            Lines: internalEvent.Lines ?? Enumerable.Empty<OrderLineV2Dto>(),
            OccurredAt: DateTime.UtcNow
        );

        await publisher.Publish(sharedEvent, ct);
    }

    private async Task PublishOrderFromCartCreatedAsync(IPublishEndpoint publisher, string payload, CancellationToken ct)
    {
        var internalEvent = JsonSerializer.Deserialize<InternalOrderFromCartCreatedEvent>(payload, JsonOptions);
        if (internalEvent is null) return;

        // Fetch order details to get complete information for saga
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderServiceDbContext>();
        
        var order = await dbContext.Set<Order>()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == internalEvent.OrderId, ct);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for OrderFromCartCreated event", internalEvent.OrderId);
            return;
        }

        var lines = order.OrderItems.Select(i => new OrderLineV2Dto(
            ProductId: i.ProductId,
            VariationId: i.VariationId,
            ProductName: i.ProductName,
            Sku: i.Sku ?? string.Empty,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            TotalPrice: i.TotalPrice
        )).ToList();

        var sharedEvent = new OrderCreatedV2(
            OrderId: order.OrderId,
            OrderNumber: order.OrderNumber,
            CustomerId: order.CustomerId,
            CustomerEmail: order.CustomerEmail ?? string.Empty,
            OrderType: order.OrderType ?? "Standard",
            OrderSource: order.OrderSource ?? "Cart",
            TotalAmount: order.TotalAmount,
            CurrencyCode: order.CurrencyCode ?? "INR",
            ItemCount: order.OrderItems.Count,
            CartId: internalEvent.CartId,
            Lines: lines,
            OccurredAt: DateTime.UtcNow
        );

        await publisher.Publish(sharedEvent, ct);

        _logger.LogInformation(
            "Published OrderCreatedV2 for OrderId={OrderId} with {LineCount} lines",
            order.OrderId, lines.Count);
    }

    private async Task PublishOrderConfirmedAsync(IPublishEndpoint publisher, string payload, CancellationToken ct)
    {
        var internalEvent = JsonSerializer.Deserialize<InternalOrderConfirmedEvent>(payload, JsonOptions);
        if (internalEvent is null) return;

        var sharedEvent = new OrderConfirmedV2(
            OrderId: internalEvent.OrderId,
            OrderNumber: internalEvent.OrderNumber,
            CustomerId: internalEvent.CustomerId,
            CustomerEmail: internalEvent.CustomerEmail,
            ConfirmedAt: internalEvent.ConfirmedAt,
            OccurredAt: DateTime.UtcNow
        );

        await publisher.Publish(sharedEvent, ct);
    }

    private async Task PublishOrderCancelledAsync(IPublishEndpoint publisher, string payload, CancellationToken ct)
    {
        var internalEvent = JsonSerializer.Deserialize<InternalOrderCancelledEvent>(payload, JsonOptions);
        if (internalEvent is null) return;

        var sharedEvent = new OrderCancelledV2(
            OrderId: internalEvent.OrderId,
            OrderNumber: internalEvent.OrderNumber,
            CustomerId: internalEvent.CustomerId,
            CustomerEmail: internalEvent.CustomerEmail,
            CancellationType: internalEvent.CancellationType,
            CancellationReason: internalEvent.CancellationReason,
            CancelledBy: null,
            CancelledAt: internalEvent.CancelledAt,
            OccurredAt: DateTime.UtcNow
        );

        await publisher.Publish(sharedEvent, ct);
    }

    private async Task PublishOrderStatusChangedAsync(IPublishEndpoint publisher, string payload, CancellationToken ct)
    {
        var internalEvent = JsonSerializer.Deserialize<InternalOrderStatusChangedEvent>(payload, JsonOptions);
        if (internalEvent is null) return;

        var sharedEvent = new OrderStatusChangedV2(
            OrderId: internalEvent.OrderId,
            OrderNumber: internalEvent.OrderNumber,
            FromStatus: internalEvent.FromStatus,
            ToStatus: internalEvent.ToStatus,
            ChangedBy: internalEvent.ChangedBy,
            Reason: internalEvent.Reason,
            NotifyCustomer: true,
            ChangedAt: internalEvent.ChangedAt,
            OccurredAt: DateTime.UtcNow
        );

        await publisher.Publish(sharedEvent, ct);
    }

    private async Task PublishOrderPaymentProcessedAsync(IPublishEndpoint publisher, string payload, CancellationToken ct)
    {
        var internalEvent = JsonSerializer.Deserialize<InternalOrderPaymentProcessedEvent>(payload, JsonOptions);
        if (internalEvent is null) return;

        var sharedEvent = new OrderPaymentProcessedV2(
            OrderId: internalEvent.OrderId,
            OrderNumber: internalEvent.OrderNumber,
            CustomerId: internalEvent.CustomerId,
            CustomerEmail: internalEvent.CustomerEmail,
            PaymentId: internalEvent.PaymentId,
            PaymentMethod: internalEvent.PaymentMethod,
            PaymentProvider: internalEvent.PaymentProvider,
            Amount: internalEvent.Amount,
            CurrencyCode: "INR",
            TransactionId: internalEvent.TransactionId,
            Status: internalEvent.Status,
            ProcessedAt: internalEvent.ProcessedAt,
            OccurredAt: DateTime.UtcNow
        );

        await publisher.Publish(sharedEvent, ct);
    }

    // Internal event DTOs for deserialization
    private record InternalOrderCreatedEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string OrderType,
        string OrderSource,
        decimal TotalAmount,
        string CurrencyCode,
        int ItemCount,
        IEnumerable<OrderLineV2Dto>? Lines = null
    );

    private record InternalOrderFromCartCreatedEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CartId,
        Guid CustomerId,
        string CustomerEmail
    );

    private record InternalOrderConfirmedEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        DateTime ConfirmedAt
    );

    private record InternalOrderCancelledEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string CancellationType,
        string CancellationReason,
        DateTime CancelledAt
    );

    private record InternalOrderStatusChangedEvent(
        Guid OrderId,
        string OrderNumber,
        string FromStatus,
        string ToStatus,
        Guid? ChangedBy,
        string? Reason,
        DateTime ChangedAt
    );

    private record InternalOrderPaymentProcessedEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? PaymentId,
        string PaymentMethod,
        string PaymentProvider,
        decimal Amount,
        string TransactionId,
        string Status,
        DateTime ProcessedAt
    );
}
