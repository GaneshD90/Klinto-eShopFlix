using System;

namespace OrderService.Application.IntegrationEvents
{
    public record OrderItemsAddedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        DateTime AddedAt);
}
