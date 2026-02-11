using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Contracts.Orders;
using OrderService.API.Infrastructure.Idempotency;
using OrderService.Application.DTOs;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Orders.Queries;
using OrderService.Application.Services.Abstractions;

namespace OrderService.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderAppService _orderService;
        private readonly IIdempotencyAppService _idempotency;
        private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromMinutes(30);

        public OrderController(IOrderAppService orderService, IIdempotencyAppService idempotency)
        {
            _orderService = orderService;
            _idempotency = idempotency;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderListItemDto>>> Search([FromQuery] SearchOrdersQuery query, CancellationToken ct)
        {
            query ??= new SearchOrdersQuery();
            var result = await _orderService.SearchAsync(query, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<OrderDetailDto>> GetById(Guid orderId, CancellationToken ct)
        {
            var order = await _orderService.GetByIdAsync(orderId, ct);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpGet("{customerId:guid}")]
        public async Task<ActionResult<PagedResult<OrderListItemDto>>> GetByCustomer(
            Guid customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var query = new GetOrdersByCustomerQuery
            {
                CustomerId = customerId,
                Page = page,
                PageSize = pageSize
            };
            var result = await _orderService.GetByCustomerAsync(query, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDetailDto>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
        {
            var command = new CreateOrderCommand
            {
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                OrderType = request.OrderType,
                OrderSource = request.OrderSource,
                CurrencyCode = request.CurrencyCode,
                IsGuestCheckout = request.IsGuestCheckout,
                CustomerNotes = request.CustomerNotes,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.FirstOrDefault(),
                Items = request.Items.Select(i => new CreateOrderItemInput
                {
                    ProductId = i.ProductId,
                    VariationId = i.VariationId,
                    ProductName = i.ProductName,
                    Sku = i.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    OriginalPrice = i.OriginalPrice,
                    IsGift = i.IsGift,
                    GiftMessage = i.GiftMessage,
                    CustomizationDetails = i.CustomizationDetails,
                    ProductSnapshot = i.ProductSnapshot
                }).ToList(),
                BillingAddress = request.BillingAddress is not null ? new CreateOrderAddressInput
                {
                    FirstName = request.BillingAddress.FirstName,
                    LastName = request.BillingAddress.LastName,
                    CompanyName = request.BillingAddress.CompanyName,
                    AddressLine1 = request.BillingAddress.AddressLine1,
                    AddressLine2 = request.BillingAddress.AddressLine2,
                    City = request.BillingAddress.City,
                    StateProvince = request.BillingAddress.StateProvince,
                    PostalCode = request.BillingAddress.PostalCode,
                    CountryCode = request.BillingAddress.CountryCode,
                    Phone = request.BillingAddress.Phone,
                    Email = request.BillingAddress.Email
                } : null,
                ShippingAddress = request.ShippingAddress is not null ? new CreateOrderAddressInput
                {
                    FirstName = request.ShippingAddress.FirstName,
                    LastName = request.ShippingAddress.LastName,
                    CompanyName = request.ShippingAddress.CompanyName,
                    AddressLine1 = request.ShippingAddress.AddressLine1,
                    AddressLine2 = request.ShippingAddress.AddressLine2,
                    City = request.ShippingAddress.City,
                    StateProvince = request.ShippingAddress.StateProvince,
                    PostalCode = request.ShippingAddress.PostalCode,
                    CountryCode = request.ShippingAddress.CountryCode,
                    Phone = request.ShippingAddress.Phone,
                    Email = request.ShippingAddress.Email
                } : null
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.CreateAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return CreatedAtAction(nameof(GetById), new { orderId = result.OrderId }, result);
        }

        [HttpPost]
        public async Task<ActionResult<CreateOrderFromCartResultDto>> CreateFromCart([FromBody] CreateOrderFromCartRequest request, CancellationToken ct)
        {
            var command = new CreateOrderFromCartCommand
            {
                CartId = request.CartId,
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                OrderSource = request.OrderSource,
                BillingAddressJson = request.BillingAddressJson,
                ShippingAddressJson = request.ShippingAddressJson,
                PaymentMethod = request.PaymentMethod,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.CreateFromCartAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return CreatedAtAction(nameof(GetById), new { orderId = result.OrderId }, result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<ConfirmOrderResultDto>> Confirm(Guid orderId, CancellationToken ct)
        {
            var command = new ConfirmOrderCommand { OrderId = orderId };
            var result = await _orderService.ConfirmAsync(command, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<OrderDetailDto>> Cancel(Guid orderId, [FromBody] CancelOrderRequest request, CancellationToken ct)
        {
            var command = new CancelOrderCommand
            {
                OrderId = orderId,
                CancellationType = request.CancellationType,
                CancellationReason = request.CancellationReason,
                CancelledBy = request.CancelledBy,
                CancelledByType = request.CancelledByType
            };

            var result = await _orderService.CancelAsync(command, ct);
            return Ok(result);
        }

        [HttpPut("{orderId:guid}")]
        public async Task<ActionResult<OrderDetailDto>> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
        {
            var command = new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                NewStatus = request.NewStatus,
                ChangedBy = request.ChangedBy,
                ChangeReason = request.ChangeReason,
                Notes = request.Notes,
                NotifyCustomer = request.NotifyCustomer
            };

            var result = await _orderService.UpdateStatusAsync(command, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderStatusHistoryDto>>> GetStatusHistory(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetStatusHistoryAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderTimelineDto>>> GetTimeline(
            Guid orderId,
            [FromQuery] bool customerVisibleOnly = false,
            CancellationToken ct = default)
        {
            var result = await _orderService.GetTimelineAsync(orderId, customerVisibleOnly, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<OrderNoteDto>> AddNote(Guid orderId, [FromBody] AddOrderNoteRequest request, CancellationToken ct)
        {
            var command = new AddOrderNoteCommand
            {
                OrderId = orderId,
                NoteType = request.NoteType,
                Note = request.Note,
                IsVisibleToCustomer = request.IsVisibleToCustomer,
                CreatedBy = request.CreatedBy
            };

            var result = await _orderService.AddNoteAsync(command, ct);
            return CreatedAtAction(nameof(GetNotes), new { orderId }, result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderNoteDto>>> GetNotes(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetNotesAsync(orderId, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<ProcessPaymentResultDto>> ProcessPayment(Guid orderId, [FromBody] ProcessPaymentRequest request, CancellationToken ct)
        {
            var command = new ProcessPaymentCommand
            {
                OrderId = orderId,
                PaymentMethod = request.PaymentMethod,
                PaymentProvider = request.PaymentProvider,
                Amount = request.Amount,
                TransactionId = request.TransactionId,
                AuthorizationCode = request.AuthorizationCode,
                PaymentGatewayResponse = request.PaymentGatewayResponse
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.ProcessPaymentAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderPaymentDto>>> GetPayments(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetPaymentsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<CreateShipmentResultDto>> CreateShipment(Guid orderId, [FromBody] CreateShipmentRequest request, CancellationToken ct)
        {
            var command = new CreateShipmentCommand
            {
                OrderId = orderId,
                WarehouseId = request.WarehouseId,
                ShippingMethod = request.ShippingMethod,
                CarrierName = request.CarrierName,
                ShipmentItemsJson = request.ShipmentItemsJson
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.CreateShipmentAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpPost("{shipmentId:guid}")]
        public async Task<ActionResult<ShipOrderResultDto>> ShipOrder(Guid shipmentId, [FromBody] ShipOrderRequest request, CancellationToken ct)
        {
            var command = new ShipOrderCommand
            {
                ShipmentId = shipmentId,
                TrackingNumber = request.TrackingNumber,
                TrackingUrl = request.TrackingUrl,
                EstimatedDeliveryDate = request.EstimatedDeliveryDate
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.ShipOrderAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpPost("{shipmentId:guid}")]
        public async Task<ActionResult<MarkOrderDeliveredResultDto>> MarkOrderDelivered(Guid shipmentId, [FromBody] MarkOrderDeliveredRequest request, CancellationToken ct)
        {
            var command = new MarkOrderDeliveredCommand
            {
                ShipmentId = shipmentId,
                DeliverySignature = request.DeliverySignature,
                DeliveryProofImage = request.DeliveryProofImage
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.MarkOrderDeliveredAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderShipmentDto>>> GetShipments(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetShipmentsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<ShipmentTrackingDto>>> GetShipmentTracking(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetShipmentTrackingAsync(orderId, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<CreateReturnRequestResultDto>> CreateReturnRequest(Guid orderId, [FromBody] CreateReturnRequestRequest request, CancellationToken ct)
        {
            var command = new CreateReturnRequestCommand
            {
                OrderId = orderId,
                CustomerId = request.CustomerId,
                ReturnType = request.ReturnType,
                ReturnReason = request.ReturnReason,
                ReturnItemsJson = request.ReturnItemsJson,
                CustomerComments = request.CustomerComments
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.CreateReturnRequestAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderReturnDto>>> GetReturns(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetReturnsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<ReturnManagementDto>>> GetReturnManagement(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetReturnManagementAsync(orderId, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<ProcessRefundResultDto>> ProcessRefund(Guid orderId, [FromBody] ProcessRefundRequest request, CancellationToken ct)
        {
            var command = new ProcessRefundCommand
            {
                OrderId = orderId,
                ReturnId = request.ReturnId,
                RefundAmount = request.RefundAmount,
                RefundType = request.RefundType,
                RefundMethod = request.RefundMethod,
                RefundReason = request.RefundReason
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.ProcessRefundAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderRefundDto>>> GetRefunds(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetRefundsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<PerformFraudCheckResultDto>> PerformFraudCheck(Guid orderId, [FromBody] PerformFraudCheckRequest request, CancellationToken ct)
        {
            var command = new PerformFraudCheckCommand
            {
                OrderId = orderId,
                FraudProvider = request.FraudProvider
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.PerformFraudCheckAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<FraudRiskDashboardDto>>> GetFraudRiskDashboard(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetFraudRiskDashboardAsync(orderId, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<PlaceOrderOnHoldResultDto>> PlaceOnHold(Guid orderId, [FromBody] PlaceOrderOnHoldRequest request, CancellationToken ct)
        {
            var command = new PlaceOrderOnHoldCommand
            {
                OrderId = orderId,
                HoldType = request.HoldType,
                HoldReason = request.HoldReason,
                PlacedBy = request.PlacedBy,
                ExpiresAt = request.ExpiresAt
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.PlaceOnHoldAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpPost("{holdId:guid}")]
        public async Task<ActionResult<ReleaseOrderHoldResultDto>> ReleaseHold(Guid holdId, [FromBody] ReleaseOrderHoldRequest request, CancellationToken ct)
        {
            var command = new ReleaseOrderHoldCommand
            {
                HoldId = holdId,
                ReleasedBy = request.ReleasedBy,
                Notes = request.Notes
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.ReleaseHoldAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderHoldManagementDto>>> GetOrderHolds(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetOrderHoldsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<CreateSubscriptionResultDto>> CreateSubscription(Guid orderId, [FromBody] CreateSubscriptionOrderRequest request, CancellationToken ct)
        {
            var command = new CreateSubscriptionOrderCommand
            {
                OrderId = orderId,
                CustomerId = request.CustomerId,
                Frequency = request.Frequency,
                StartDate = request.StartDate,
                TotalOccurrences = request.TotalOccurrences,
                PaymentMethodId = request.PaymentMethodId
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.CreateSubscriptionAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpPost("{subscriptionId:guid}")]
        public async Task<ActionResult<ProcessNextSubscriptionResultDto>> ProcessNextSubscription(Guid subscriptionId, CancellationToken ct)
        {
            var command = new ProcessNextSubscriptionCommand
            {
                SubscriptionId = subscriptionId
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, command);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.ProcessNextSubscriptionAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SubscriptionAnalysisDto>>> GetSubscriptionAnalysis(
            [FromQuery] Guid? customerId = null,
            CancellationToken ct = default)
        {
            var result = await _orderService.GetSubscriptionAnalysisAsync(customerId, ct);
            return Ok(result);
        }

        // ============ Order Items ============

        [HttpPost("{orderId:guid}")]
        public async Task<ActionResult<AddOrderItemsResultDto>> AddOrderItems(Guid orderId, [FromBody] AddOrderItemsRequest request, CancellationToken ct)
        {
            var command = new AddOrderItemsCommand
            {
                OrderId = orderId,
                OrderItemsJson = request.OrderItemsJson
            };

            var (key, hash) = IdempotencyKeyResolver.Resolve(Request, request);
            var result = await _idempotency.ExecuteAsync(
                key,
                userId: null,
                _ => _orderService.AddOrderItemsAsync(command, ct),
                ttl: IdempotencyTtl,
                requestHash: hash,
                ct: ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderItemDto>>> GetOrderItems(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetOrderItemsAsync(orderId, ct);
            return Ok(result);
        }

        // ============ Analytics & Reporting ============

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetOrderSummaries(CancellationToken ct)
        {
            var result = await _orderService.GetOrderSummariesAsync(ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CustomerOrderHistoryDto>>> GetCustomerOrderHistory(CancellationToken ct)
        {
            var result = await _orderService.GetCustomerOrderHistoryAsync(ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RevenueAnalysisDto>>> GetRevenueAnalysis(CancellationToken ct)
        {
            var result = await _orderService.GetRevenueAnalysisAsync(ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DailyOrderMetricDto>>> GetDailyOrderMetrics(CancellationToken ct)
        {
            var result = await _orderService.GetDailyOrderMetricsAsync(ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PendingOrderActionDto>>> GetPendingOrderActions(CancellationToken ct)
        {
            var result = await _orderService.GetPendingOrderActionsAsync(ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PaymentAnalysisDto>>> GetPaymentAnalysis(CancellationToken ct)
        {
            var result = await _orderService.GetPaymentAnalysisAsync(ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductOrderPerformanceDto>>> GetProductOrderPerformance(CancellationToken ct)
        {
            var result = await _orderService.GetProductOrderPerformanceAsync(ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FulfillmentPerformanceDto>>> GetFulfillmentPerformance(CancellationToken ct)
        {
            var result = await _orderService.GetFulfillmentPerformanceAsync(ct);
            return Ok(result);
        }

        // ============ Ancillary Order Data ============

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderDiscountDto>>> GetDiscounts(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetDiscountsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderTaxDto>>> GetTaxes(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetTaxesAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderAdjustmentDto>>> GetAdjustments(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetAdjustmentsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderGiftCardDto>>> GetGiftCards(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetGiftCardsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderLoyaltyPointDto>>> GetLoyaltyPoints(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetLoyaltyPointsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderDocumentDto>>> GetDocuments(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetDocumentsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<OrderMetricDto>> GetMetric(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetMetricAsync(orderId, ct);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("{orderItemId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderItemOptionDto>>> GetItemOptions(Guid orderItemId, CancellationToken ct)
        {
            var result = await _orderService.GetItemOptionsAsync(orderItemId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderFulfillmentAssignmentDto>>> GetFulfillmentAssignments(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetFulfillmentAssignmentsAsync(orderId, ct);
            return Ok(result);
        }

        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<IReadOnlyList<OrderCancellationDto>>> GetCancellations(Guid orderId, CancellationToken ct)
        {
            var result = await _orderService.GetCancellationsAsync(orderId, ct);
            return Ok(result);
        }
    }
}
