using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetOrderDiscountsQuery : IQuery<IReadOnlyList<OrderDiscountDto>>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderTaxesQuery : IQuery<IReadOnlyList<OrderTaxDto>>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderAdjustmentsQuery : IQuery<IReadOnlyList<OrderAdjustmentDto>>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderGiftCardsQuery : IQuery<IReadOnlyList<OrderGiftCardDto>>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderLoyaltyPointsQuery : IQuery<IReadOnlyList<OrderLoyaltyPointDto>>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderDocumentsQuery : IQuery<IReadOnlyList<OrderDocumentDto>>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderMetricQuery : IQuery<OrderMetricDto?>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderItemOptionsQuery : IQuery<IReadOnlyList<OrderItemOptionDto>>
    {
        public Guid OrderItemId { get; init; }
    }

    public sealed class GetOrderFulfillmentAssignmentsQuery : IQuery<IReadOnlyList<OrderFulfillmentAssignmentDto>>
    {
        public Guid OrderId { get; init; }
    }

    public sealed class GetOrderCancellationsQuery : IQuery<IReadOnlyList<OrderCancellationDto>>
    {
        public Guid OrderId { get; init; }
    }
}
