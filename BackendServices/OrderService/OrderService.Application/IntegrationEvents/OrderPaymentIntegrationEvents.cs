using System;

namespace OrderService.Application.IntegrationEvents
{
    public record OrderPaymentProcessedIntegrationEvent(
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
        DateTime ProcessedAt);
}
