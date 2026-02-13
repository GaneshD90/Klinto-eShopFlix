namespace Contracts.Messaging;

/// <summary>
/// Centralized topic/queue naming constants for Azure Service Bus.
/// All services reference these to ensure consistent naming across producers and consumers.
/// MassTransit uses message type for routing by default, but these are useful
/// for explicit publish/send scenarios and documentation.
/// </summary>
public static class ServiceBusTopics
{
    public const string CartEvents = "eshopflix-cart-events";
    public const string OrderEvents = "eshopflix-order-events";
    public const string PaymentEvents = "eshopflix-payment-events";
    public const string StockEvents = "eshopflix-stock-events";
    public const string CatalogEvents = "eshopflix-catalog-events";
    public const string AuthEvents = "eshopflix-auth-events";
}
