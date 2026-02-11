using System;

namespace OrderService.Application.IntegrationEvents
{
    public record ShipmentCreatedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? ShipmentId,
        string ShipmentNumber,
        string ShippingMethod,
        string CarrierName,
        DateTime CreatedAt);

    public record OrderShippedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid ShipmentId,
        string TrackingNumber,
        string TrackingUrl,
        DateTime? EstimatedDeliveryDate,
        DateTime ShippedAt);

    public record OrderDeliveredIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid ShipmentId,
        string DeliverySignature,
        DateTime DeliveredAt);
}
