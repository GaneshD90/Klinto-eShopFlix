using eShopFlix.Web.Helpers;
using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Controllers
{
    public class PaymentController : BaseController
    {
        private readonly CartServiceClient _cartServiceClient;
        private readonly PaymentServiceClient _paymentServiceClient;
        private readonly OrderServiceClient _orderServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            CartServiceClient cartServiceClient,
            PaymentServiceClient paymentServiceClient,
            OrderServiceClient orderServiceClient,
            IConfiguration configuration,
            ILogger<PaymentController> logger)
        {
            _cartServiceClient = cartServiceClient;
            _paymentServiceClient = paymentServiceClient;
            _orderServiceClient = orderServiceClient;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var cartModel = await _cartServiceClient.GetUserCartAsync(CurrentUser.UserId);
            if (cartModel != null)
            {
                var payment = new PaymentModel
                {
                    Cart = cartModel,
                    Currency = "INR",
                    Description = string.Join(",", cartModel.CartItems.Select(x => x.Name)),
                    GrandTotal = cartModel.GrandTotal,
                    RazorpayKey = _configuration["RazorPay:Key"]
                };
                var razorpayOrder = new RazorPayOrderModel
                {
                    Amount = Convert.ToInt32(payment.GrandTotal * 100),
                    Currency = payment.Currency,
                    Receipt = Guid.NewGuid().ToString()
                };
                payment.OrderId = await _paymentServiceClient.CreateOrderAsync(razorpayOrder) ?? string.Empty;
                payment.Receipt = razorpayOrder.Receipt; // ensure we round-trip the receipt to the Status action
                return View(payment);

            }
            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> Status(IFormCollection form)
        {
            if (!string.IsNullOrEmpty(form["rzp_paymentid"]))
            {
                string paymentId = form["rzp_paymentid"]!;
                string rzpOrderId = form["rzp_orderid"]!;
                string signature = form["rzp_signature"]!;
                string transactionId = form["Receipt"]!;
                string currency = form["Currency"]!;

                var payment = new PaymentConfirmModel
                {
                    PaymentId = paymentId,
                    OrderId = rzpOrderId,
                    Signature = signature
                };
                string status = await _paymentServiceClient.VerifyPaymentAsync(payment);
                if (status == "captured" || status == "completed")
                {
                    var cart = await _cartServiceClient.GetUserCartAsync(CurrentUser!.UserId);
                    if (cart == null)
                    {
                        ViewBag.Message = "Cart not found after payment. Please contact support with your payment id.";
                        return View();
                    }

                    // Save payment to PaymentService (existing behavior)
                    var model = new TransactionModel
                    {
                        CartId = cart.Id,
                        Total = cart.Total,
                        Tax = cart.Tax,
                        GrandTotal = cart.GrandTotal,
                        CreatedDate = DateTime.Now,
                        Status = status,
                        TransactionId = transactionId,
                        Currency = currency,
                        Email = CurrentUser.Email,
                        Id = paymentId,
                        UserId = CurrentUser.UserId
                    };

                    bool result = await _paymentServiceClient.SavePaymentDetailsAsync(model);
                    if (result)
                    {
                        // Create order in OrderService from the cart
                        await CreateOrderFromCartAsync(cart, paymentId, transactionId, currency, model);

                        await _cartServiceClient.MakeCartInActiveAsync(cart.Id);
                        TempData.Set("Receipt", model);
                        return RedirectToAction("Receipt");
                    }
                }
            }
            ViewBag.Message = "Due to some technical issues we are not able to receive order details. We will contact you soon..";
            return View();
        }

        /// <summary>
        /// Creates an order in OrderService from the finalized cart and records the payment against it.
        /// This is best-effort: if OrderService is down, the payment receipt is still shown
        /// and the order can be reconciled later from the PaymentService transaction record.
        /// </summary>
        private async Task CreateOrderFromCartAsync(
            CartModel cart, string paymentId, string transactionId, string currency, TransactionModel model)
        {
            try
            {
                // Build address JSON from checkout TempData if available
                string? shippingAddressJson = null;
                var address = TempData.Peek<AddressModel>("Address");
                if (address != null)
                {
                    shippingAddressJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        FirstName = CurrentUser!.Name,
                        LastName = string.Empty,
                        AddressLine1 = address.Street,
                        AddressLine2 = address.Locality,
                        City = address.City,
                        PostalCode = address.ZipCode,
                        CountryCode = "IN",
                        Phone = address.PhoneNumber
                    });
                }

                // Create the order — CartId is Guid in OrderService, so we create a deterministic one
                var cartGuid = CreateDeterministicGuid(cart.Id);
                var customerGuid = CreateDeterministicGuid(CurrentUser!.UserId);

                var orderResult = await _orderServiceClient.CreateFromCartAsync(
                    cartId: cartGuid,
                    customerId: customerGuid,
                    customerEmail: CurrentUser.Email,
                    orderSource: "Web",
                    shippingAddressJson: shippingAddressJson,
                    paymentMethod: "Razorpay");

                if (orderResult is { IsSuccess: true, OrderId: not null })
                {
                    model.OrderId = orderResult.OrderId;
                    model.OrderNumber = orderResult.OrderNumber;

                    _logger.LogInformation(
                        "Order {OrderNumber} created for CartId {CartId}, PaymentId {PaymentId}",
                        orderResult.OrderNumber, cart.Id, paymentId);

                    // Record the payment against the order in OrderService
                    await _orderServiceClient.ProcessPaymentAsync(
                        orderId: orderResult.OrderId.Value,
                        paymentMethod: "Razorpay",
                        paymentProvider: "Razorpay",
                        amount: cart.GrandTotal,
                        transactionId: paymentId);
                }
                else
                {
                    _logger.LogWarning(
                        "Order creation returned non-success for CartId {CartId}: {Message}",
                        cart.Id, orderResult?.Message ?? "null response");
                }
            }
            catch (Exception ex)
            {
                // Best-effort: log but don't fail the payment receipt flow
                _logger.LogError(ex,
                    "Failed to create order for CartId {CartId}. Payment {PaymentId} was captured successfully. Manual reconciliation may be needed.",
                    cart.Id, paymentId);
            }
        }

        private static Guid CreateDeterministicGuid(long id)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(id).CopyTo(bytes, 0);
            bytes[8] = 0xE5; // eShop marker
            bytes[9] = 0x0F; // Flix marker
            return new Guid(bytes);
        }

        public IActionResult Receipt()
        {
            TransactionModel model = TempData.Get<TransactionModel>("Receipt");
            return View(model);
        }
    }
}
