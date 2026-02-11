using System;

namespace OrderService.Application.IntegrationEvents
{
    public record FraudCheckPerformedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid? FraudCheckId,
        decimal? RiskScore,
        string RiskLevel,
        string RecommendedAction,
        string Status,
        DateTime CheckedAt);

    public record OrderPlacedOnHoldIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        string HoldType,
        string HoldReason,
        Guid? PlacedBy,
        DateTime PlacedAt);

    public record OrderHoldReleasedIntegrationEvent(
        Guid OrderId,
        string OrderNumber,
        Guid CustomerId,
        string CustomerEmail,
        Guid HoldId,
        Guid? ReleasedBy,
        DateTime ReleasedAt);
}
