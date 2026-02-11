using System;

namespace OrderService.Application.IntegrationEvents
{
    public record OrderCreatedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string OrderType,
        string OrderSource,
        decimal TotalAmount,
        string CurrencyCode,
        int ItemCount);

    public record OrderFromCartCreatedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CartId,
        Guid CustomerId,
        string CustomerEmail);
}
