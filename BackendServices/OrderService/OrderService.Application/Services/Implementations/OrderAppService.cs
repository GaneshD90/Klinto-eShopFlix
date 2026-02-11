using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Orders.Queries;
using OrderService.Application.Services.Abstractions;

namespace OrderService.Application.Services.Implementations
{
    public sealed class OrderAppService : IOrderAppService
    {
        private readonly IDispatcher _dispatcher;

        public OrderAppService(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task<OrderDetailDto> CreateAsync(CreateOrderCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<CreateOrderFromCartResultDto> CreateFromCartAsync(CreateOrderFromCartCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<OrderDetailDto?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderByIdQuery { OrderId = orderId }, ct);

        public async Task<PagedResult<OrderListItemDto>> GetByCustomerAsync(GetOrdersByCustomerQuery query, CancellationToken ct = default)
            => await _dispatcher.Query(query, ct);

        public async Task<PagedResult<OrderListItemDto>> SearchAsync(SearchOrdersQuery query, CancellationToken ct = default)
            => await _dispatcher.Query(query, ct);

        public async Task<ConfirmOrderResultDto> ConfirmAsync(ConfirmOrderCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<OrderDetailDto> CancelAsync(CancelOrderCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<OrderDetailDto> UpdateStatusAsync(UpdateOrderStatusCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<OrderNoteDto> AddNoteAsync(AddOrderNoteCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<OrderStatusHistoryDto>> GetStatusHistoryAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderStatusHistoryQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderTimelineDto>> GetTimelineAsync(Guid orderId, bool customerVisibleOnly = false, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderTimelineQuery { OrderId = orderId, CustomerVisibleOnly = customerVisibleOnly }, ct);

        public async Task<IReadOnlyList<OrderNoteDto>> GetNotesAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderNotesQuery { OrderId = orderId }, ct);

        public async Task<ProcessPaymentResultDto> ProcessPaymentAsync(ProcessPaymentCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<OrderPaymentDto>> GetPaymentsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderPaymentsQuery { OrderId = orderId }, ct);

        public async Task<CreateShipmentResultDto> CreateShipmentAsync(CreateShipmentCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<ShipOrderResultDto> ShipOrderAsync(ShipOrderCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<MarkOrderDeliveredResultDto> MarkOrderDeliveredAsync(MarkOrderDeliveredCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<OrderShipmentDto>> GetShipmentsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderShipmentsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<ShipmentTrackingDto>> GetShipmentTrackingAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetShipmentTrackingQuery { OrderId = orderId }, ct);

        public async Task<CreateReturnRequestResultDto> CreateReturnRequestAsync(CreateReturnRequestCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<OrderReturnDto>> GetReturnsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderReturnsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<ReturnManagementDto>> GetReturnManagementAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetReturnManagementQuery { OrderId = orderId }, ct);

        public async Task<ProcessRefundResultDto> ProcessRefundAsync(ProcessRefundCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<OrderRefundDto>> GetRefundsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderRefundsQuery { OrderId = orderId }, ct);

        public async Task<PerformFraudCheckResultDto> PerformFraudCheckAsync(PerformFraudCheckCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<FraudRiskDashboardDto>> GetFraudRiskDashboardAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetFraudRiskDashboardQuery { OrderId = orderId }, ct);

        public async Task<PlaceOrderOnHoldResultDto> PlaceOnHoldAsync(PlaceOrderOnHoldCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<ReleaseOrderHoldResultDto> ReleaseHoldAsync(ReleaseOrderHoldCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<OrderHoldManagementDto>> GetOrderHoldsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderHoldsQuery { OrderId = orderId }, ct);

        public async Task<CreateSubscriptionResultDto> CreateSubscriptionAsync(CreateSubscriptionOrderCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<ProcessNextSubscriptionResultDto> ProcessNextSubscriptionAsync(ProcessNextSubscriptionCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<SubscriptionAnalysisDto>> GetSubscriptionAnalysisAsync(Guid? customerId = null, CancellationToken ct = default)
            => await _dispatcher.Query(new GetSubscriptionAnalysisQuery { CustomerId = customerId }, ct);

        public async Task<AddOrderItemsResultDto> AddOrderItemsAsync(AddOrderItemsCommand command, CancellationToken ct = default)
            => await _dispatcher.Send(command, ct);

        public async Task<IReadOnlyList<OrderItemDto>> GetOrderItemsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderItemsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderSummaryDto>> GetOrderSummariesAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderSummariesQuery(), ct);

        public async Task<IReadOnlyList<CustomerOrderHistoryDto>> GetCustomerOrderHistoryAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetCustomerOrderHistoryQuery(), ct);

        public async Task<IReadOnlyList<RevenueAnalysisDto>> GetRevenueAnalysisAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetRevenueAnalysisQuery(), ct);

        public async Task<IReadOnlyList<DailyOrderMetricDto>> GetDailyOrderMetricsAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetDailyOrderMetricsQuery(), ct);

        public async Task<IReadOnlyList<PendingOrderActionDto>> GetPendingOrderActionsAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetPendingOrderActionsQuery(), ct);

        public async Task<IReadOnlyList<PaymentAnalysisDto>> GetPaymentAnalysisAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetPaymentAnalysisQuery(), ct);

        public async Task<IReadOnlyList<ProductOrderPerformanceDto>> GetProductOrderPerformanceAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetProductOrderPerformanceQuery(), ct);

        public async Task<IReadOnlyList<FulfillmentPerformanceDto>> GetFulfillmentPerformanceAsync(CancellationToken ct = default)
            => await _dispatcher.Query(new GetFulfillmentPerformanceQuery(), ct);

        public async Task<IReadOnlyList<OrderDiscountDto>> GetDiscountsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderDiscountsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderTaxDto>> GetTaxesAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderTaxesQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderAdjustmentDto>> GetAdjustmentsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderAdjustmentsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderGiftCardDto>> GetGiftCardsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderGiftCardsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderLoyaltyPointDto>> GetLoyaltyPointsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderLoyaltyPointsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderDocumentDto>> GetDocumentsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderDocumentsQuery { OrderId = orderId }, ct);

        public async Task<OrderMetricDto?> GetMetricAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderMetricQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderItemOptionDto>> GetItemOptionsAsync(Guid orderItemId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderItemOptionsQuery { OrderItemId = orderItemId }, ct);

        public async Task<IReadOnlyList<OrderFulfillmentAssignmentDto>> GetFulfillmentAssignmentsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderFulfillmentAssignmentsQuery { OrderId = orderId }, ct);

        public async Task<IReadOnlyList<OrderCancellationDto>> GetCancellationsAsync(Guid orderId, CancellationToken ct = default)
            => await _dispatcher.Query(new GetOrderCancellationsQuery { OrderId = orderId }, ct);
    }
}
