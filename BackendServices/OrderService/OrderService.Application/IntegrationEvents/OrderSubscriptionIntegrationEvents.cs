using System;

namespace OrderService.Application.IntegrationEvents
{
    public record SubscriptionCreatedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? SubscriptionId,
        string Frequency,
        DateTime? StartDate,
        int? TotalOccurrences,
        DateTime CreatedAt);

    public record SubscriptionNextOrderProcessedIntegrationEvent(
        Guid SubscriptionId,
        string Status,
        string Message,
        DateTime ProcessedAt);
}
