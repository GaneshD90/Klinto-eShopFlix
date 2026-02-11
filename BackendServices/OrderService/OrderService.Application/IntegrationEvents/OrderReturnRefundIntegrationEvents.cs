using System;

namespace OrderService.Application.IntegrationEvents
{
    public record ReturnRequestCreatedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? ReturnId,
        string ReturnNumber,
        string ReturnType,
        string ReturnReason,
        DateTime RequestedAt);

    public record RefundProcessedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? RefundId,
        string RefundNumber,
        decimal RefundAmount,
        string RefundType,
        string RefundMethod,
        string RefundReason,
        DateTime ProcessedAt);
}
