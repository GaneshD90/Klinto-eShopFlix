using System;

namespace OrderService.Application.IntegrationEvents
{
    public record OrderConfirmedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        DateTime ConfirmedAt);

    public record OrderCancelledIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string CancellationType,
        string CancellationReason,
        DateTime CancelledAt);

    public record OrderStatusChangedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        string FromStatus,
        string ToStatus,
        Guid? ChangedBy,
        string? Reason,
        DateTime ChangedAt);
}
