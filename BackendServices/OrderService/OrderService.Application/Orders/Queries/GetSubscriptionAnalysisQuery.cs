using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetSubscriptionAnalysisQuery : IQuery<IReadOnlyList<SubscriptionAnalysisDto>>
    {
        public Guid? CustomerId { get; init; }
    }
}
