using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappers
{
    public class OrderMapper : Profile
    {
        public OrderMapper()
        {
            CreateMap<Order, OrderDetailDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.OrderAddresses));

            CreateMap<Order, OrderListItemDto>()
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems != null ? src.OrderItems.Count : 0));

            CreateMap<OrderItem, OrderItemDto>();

            CreateMap<OrderAddress, OrderAddressDto>();

            CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();

            CreateMap<OrderTimeline, OrderTimelineDto>();

            CreateMap<OrderNote, OrderNoteDto>();

            CreateMap<OrderPayment, OrderPaymentDto>();

            CreateMap<OrderShipment, OrderShipmentDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderShipmentItems));

            CreateMap<OrderShipmentItem, OrderShipmentItemDto>();

            CreateMap<VwShipmentTracking, ShipmentTrackingDto>();

            CreateMap<OrderReturn, OrderReturnDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderReturnItems));

            CreateMap<OrderReturnItem, OrderReturnItemDto>();

            CreateMap<OrderRefund, OrderRefundDto>();

            CreateMap<VwReturnManagement, ReturnManagementDto>();

            CreateMap<VwFraudRiskDashboard, FraudRiskDashboardDto>();

            CreateMap<VwOrderHoldManagement, OrderHoldManagementDto>();

            CreateMap<VwSubscriptionAnalysis, SubscriptionAnalysisDto>();

            CreateMap<VwOrderSummary, OrderSummaryDto>();

            CreateMap<VwCustomerOrderHistory, CustomerOrderHistoryDto>();

            CreateMap<VwRevenueAnalysis, RevenueAnalysisDto>();

            CreateMap<VwDailyOrderMetric, DailyOrderMetricDto>();

            CreateMap<VwPendingOrdersAction, PendingOrderActionDto>();

            CreateMap<VwPaymentAnalysis, PaymentAnalysisDto>();

            CreateMap<VwProductOrderPerformance, ProductOrderPerformanceDto>();

            CreateMap<VwFulfillmentPerformance, FulfillmentPerformanceDto>();

            CreateMap<OrderDiscount, OrderDiscountDto>();

            CreateMap<OrderTaxis, OrderTaxDto>();

            CreateMap<OrderAdjustment, OrderAdjustmentDto>();

            CreateMap<OrderGiftCard, OrderGiftCardDto>();

            CreateMap<OrderLoyaltyPoint, OrderLoyaltyPointDto>();

            CreateMap<OrderDocument, OrderDocumentDto>();

            CreateMap<OrderMetric, OrderMetricDto>();

            CreateMap<OrderItemOption, OrderItemOptionDto>();

            CreateMap<OrderFulfillmentAssignment, OrderFulfillmentAssignmentDto>();

            CreateMap<OrderCancellation, OrderCancellationDto>();
        }
    }
}
