using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Orders.Queries;

namespace OrderService.Application.Services.Abstractions
{
    public interface IOrderAppService
    {
        Task<PagedResult<OrderListItemDto>> SearchAsync(SearchOrdersQuery query, CancellationToken ct = default);
        Task<OrderDetailDto?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
        Task<PagedResult<OrderListItemDto>> GetByCustomerAsync(GetOrdersByCustomerQuery query, CancellationToken ct = default);
        Task<OrderDetailDto> CreateAsync(CreateOrderCommand command, CancellationToken ct = default);
        Task<CreateOrderFromCartResultDto> CreateFromCartAsync(CreateOrderFromCartCommand command, CancellationToken ct = default);
        Task<ConfirmOrderResultDto> ConfirmAsync(ConfirmOrderCommand command, CancellationToken ct = default);
        Task<OrderDetailDto> CancelAsync(CancelOrderCommand command, CancellationToken ct = default);
        Task<OrderDetailDto> UpdateStatusAsync(UpdateOrderStatusCommand command, CancellationToken ct = default);
        Task<OrderNoteDto> AddNoteAsync(AddOrderNoteCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<OrderStatusHistoryDto>> GetStatusHistoryAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderTimelineDto>> GetTimelineAsync(Guid orderId, bool customerVisibleOnly = false, CancellationToken ct = default);
        Task<IReadOnlyList<OrderNoteDto>> GetNotesAsync(Guid orderId, CancellationToken ct = default);
        Task<ProcessPaymentResultDto> ProcessPaymentAsync(ProcessPaymentCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<OrderPaymentDto>> GetPaymentsAsync(Guid orderId, CancellationToken ct = default);
        Task<CreateShipmentResultDto> CreateShipmentAsync(CreateShipmentCommand command, CancellationToken ct = default);
        Task<ShipOrderResultDto> ShipOrderAsync(ShipOrderCommand command, CancellationToken ct = default);
        Task<MarkOrderDeliveredResultDto> MarkOrderDeliveredAsync(MarkOrderDeliveredCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<OrderShipmentDto>> GetShipmentsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<ShipmentTrackingDto>> GetShipmentTrackingAsync(Guid orderId, CancellationToken ct = default);
        Task<CreateReturnRequestResultDto> CreateReturnRequestAsync(CreateReturnRequestCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<OrderReturnDto>> GetReturnsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<ReturnManagementDto>> GetReturnManagementAsync(Guid orderId, CancellationToken ct = default);
        Task<ProcessRefundResultDto> ProcessRefundAsync(ProcessRefundCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<OrderRefundDto>> GetRefundsAsync(Guid orderId, CancellationToken ct = default);
        Task<PerformFraudCheckResultDto> PerformFraudCheckAsync(PerformFraudCheckCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<FraudRiskDashboardDto>> GetFraudRiskDashboardAsync(Guid orderId, CancellationToken ct = default);
        Task<PlaceOrderOnHoldResultDto> PlaceOnHoldAsync(PlaceOrderOnHoldCommand command, CancellationToken ct = default);
        Task<ReleaseOrderHoldResultDto> ReleaseHoldAsync(ReleaseOrderHoldCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<OrderHoldManagementDto>> GetOrderHoldsAsync(Guid orderId, CancellationToken ct = default);
        Task<CreateSubscriptionResultDto> CreateSubscriptionAsync(CreateSubscriptionOrderCommand command, CancellationToken ct = default);
        Task<ProcessNextSubscriptionResultDto> ProcessNextSubscriptionAsync(ProcessNextSubscriptionCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<SubscriptionAnalysisDto>> GetSubscriptionAnalysisAsync(Guid? customerId = null, CancellationToken ct = default);
        Task<AddOrderItemsResultDto> AddOrderItemsAsync(AddOrderItemsCommand command, CancellationToken ct = default);
        Task<IReadOnlyList<OrderItemDto>> GetOrderItemsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderSummaryDto>> GetOrderSummariesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CustomerOrderHistoryDto>> GetCustomerOrderHistoryAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RevenueAnalysisDto>> GetRevenueAnalysisAsync(CancellationToken ct = default);
        Task<IReadOnlyList<DailyOrderMetricDto>> GetDailyOrderMetricsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PendingOrderActionDto>> GetPendingOrderActionsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PaymentAnalysisDto>> GetPaymentAnalysisAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ProductOrderPerformanceDto>> GetProductOrderPerformanceAsync(CancellationToken ct = default);
        Task<IReadOnlyList<FulfillmentPerformanceDto>> GetFulfillmentPerformanceAsync(CancellationToken ct = default);
        Task<IReadOnlyList<OrderDiscountDto>> GetDiscountsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderTaxDto>> GetTaxesAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderAdjustmentDto>> GetAdjustmentsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderGiftCardDto>> GetGiftCardsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderLoyaltyPointDto>> GetLoyaltyPointsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderDocumentDto>> GetDocumentsAsync(Guid orderId, CancellationToken ct = default);
        Task<OrderMetricDto?> GetMetricAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderItemOptionDto>> GetItemOptionsAsync(Guid orderItemId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderFulfillmentAssignmentDto>> GetFulfillmentAssignmentsAsync(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderCancellationDto>> GetCancellationsAsync(Guid orderId, CancellationToken ct = default);
    }
}
