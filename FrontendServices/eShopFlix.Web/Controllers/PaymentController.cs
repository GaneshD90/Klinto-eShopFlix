using eShopFlix.Web.Helpers;
using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Controllers
{
    public class PaymentController : BaseController
    {
        readonly CartServiceClient _cartServiceClient;
        readonly PaymentServiceClient _paymentServiceClient;
        readonly IConfiguration _configuration;
        public PaymentController(CartServiceClient cartServiceClient,PaymentServiceClient paymentServiceClient, IConfiguration configuration )
        {
            _cartServiceClient = cartServiceClient;
            _paymentServiceClient= paymentServiceClient;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            CartModel cartModel = _cartServiceClient.GetUserCartAsync(CurrentUser.UserId).Result;
            if (cartModel != null)
            {
                PaymentModel payment = new PaymentModel();
                payment.Cart= cartModel;
                payment.Currency = "INR";
                payment.Description = string.Join(",", cartModel.CartItems.Select(x => x.Name));
                payment.GrandTotal =cartModel.GrandTotal;

                payment.RazorpayKey = _configuration["Razorpay:Key"];
                RazorPayOrderModel razorpayOrder = new RazorPayOrderModel
                {
                    Amount = Convert.ToInt32(payment.GrandTotal * 100),
                    Currency = payment.Currency,
                    Receipt = Guid.NewGuid().ToString()
                };
                payment.OrderId = _paymentServiceClient.CreateOrderAsync(razorpayOrder).Result;
                return View(payment);

            }
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Status (IFormCollection form)
        {
            if (!string.IsNullOrEmpty(form["rzp_paymentid"]))
            {
                string paymentId = form["rzp_paymentid"];
                string orderId = form["rzp_orderid"];
                string signature = form["rzp_signature"];
                string transactionId = form["Receipt"];
                string currency = form["Currency"];

                PaymentConfirmModel payment = new PaymentConfirmModel
                {
                    PaymentId = paymentId,
                    OrderId = orderId,
                    Signature = signature
                };
                string status = _paymentServiceClient.VerifyPaymentAsync(payment).Result;
                if (status == "captured" || status == "completed")
                {
                    CartModel cart = _cartServiceClient.GetUserCartAsync(CurrentUser.UserId).Result;
                    TransactionModel model = new TransactionModel();

                    model.CartId = cart.Id;
                    model.Total = cart.Total;
                    model.Tax = cart.Tax;
                    model.GrandTotal = cart.GrandTotal;
                    model.CreatedDate = DateTime.Now;

                    model.Status = status;
                    model.TransactionId = transactionId;
                    model.Currency = currency;
                    model.Email = CurrentUser.Email;
                    model.Id = paymentId;
                    model.UserId = CurrentUser.UserId;

                    bool result = _paymentServiceClient.SavePaymentDetailsAsync(model).Result;
                    if (result)
                    {

                        _cartServiceClient.MakeCartInActiveAsync(cart.Id).Wait();

                        TempData.Set("Receipt", model);
                        return RedirectToAction("Receipt");
                    }
                }
            }
            ViewBag.Message = "Due to some technical issues we are not able to receive order details. We will contact you soon..";
            return View();
        }

        public IActionResult Receipt()
        {
            TransactionModel model = TempData.Get<TransactionModel>("Receipt");
            return View(model);
        }
    }
}
