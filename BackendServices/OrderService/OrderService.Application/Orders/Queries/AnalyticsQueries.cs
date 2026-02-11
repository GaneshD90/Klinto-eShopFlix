using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetOrderSummariesQuery : IQuery<IReadOnlyList<OrderSummaryDto>> { }

    public sealed class GetCustomerOrderHistoryQuery : IQuery<IReadOnlyList<CustomerOrderHistoryDto>> { }

    public sealed class GetRevenueAnalysisQuery : IQuery<IReadOnlyList<RevenueAnalysisDto>> { }

    public sealed class GetDailyOrderMetricsQuery : IQuery<IReadOnlyList<DailyOrderMetricDto>> { }

    public sealed class GetPendingOrderActionsQuery : IQuery<IReadOnlyList<PendingOrderActionDto>> { }

    public sealed class GetPaymentAnalysisQuery : IQuery<IReadOnlyList<PaymentAnalysisDto>> { }

    public sealed class GetProductOrderPerformanceQuery : IQuery<IReadOnlyList<ProductOrderPerformanceDto>> { }

    public sealed class GetFulfillmentPerformanceQuery : IQuery<IReadOnlyList<FulfillmentPerformanceDto>> { }
}
