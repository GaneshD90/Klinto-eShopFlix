using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.Messaging;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging
{
    public sealed class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly OrderServiceDbContext _dbContext;

        public OutboxIntegrationEventPublisher(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnqueueAsync(string eventType, object payload, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ArgumentException("Event type is required", nameof(eventType));
            }

            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var outboxEntry = new OutboxMessage
            {
                OutboxMessageId = Guid.NewGuid(),
                AggregateType = "Order",
                AggregateId = Guid.Empty,
                EventType = eventType,
                EventVersion = 1,
                Payload = JsonSerializer.Serialize(payload, payload.GetType(), SerializerOptions),
                OccurredAt = DateTime.UtcNow,
                ProcessedAt = null,
                RetryCount = 0
            };

            await _dbContext.Set<OutboxMessage>().AddAsync(outboxEntry, ct);
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
