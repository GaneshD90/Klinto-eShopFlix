using eShopFlix.Web.Helpers;
using eShopFlix.Web.HttpClients;
using eShopFlix.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace eShopFlix.Web.Controllers
{
    public class CartController : BaseController
    {
        CartServiceClient _cartServiceClient;
        public CartController(CartServiceClient cartServiceClient)
        {
            _cartServiceClient = cartServiceClient;
        }

        public async Task<IActionResult> Index()
        {
            if (CurrentUser != null)
            {
                CartModel cartModel =   await _cartServiceClient.GetUserCartAsync(CurrentUser.UserId);
                return View(cartModel);
            }
            else
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/" });
            }
        }

        [Route("Cart/AddToCart/{ItemId}/{UnitPrice}/{Quantity}")]
        public async Task<IActionResult> AddToCart(int ItemId, decimal UnitPrice, int Quantity)
        {
            CartItemModel cartItemModel = new CartItemModel
            {
                ItemId = ItemId,
                Quantity = Quantity,
                UnitPrice = UnitPrice
            };

            CartModel cartModel = await _cartServiceClient.AddToCartAsync(cartItemModel, CurrentUser.UserId);
            if (cartModel != null)
            {
                return Json(new { status = "success", count = cartModel.CartItems.Count });
            }
            return Json(new { status = "failed", count = 0 });
        }

        [Route("Cart/UpdateQuantity/{Id}/{Quantity}/{CartId}")]
        public async Task<IActionResult> UpdateQuantity(int Id, int Quantity, long CartId)
        {
            int count = await _cartServiceClient.UpdateQuantity(CartId, Id, Quantity);
            return Json(count);
        }

        [Route("Cart/DeleteItem/{Id}/{CartId}")]
        public async Task<IActionResult> DeleteItem(int Id, long CartId)
        {
            int count =  await _cartServiceClient.DeleteCartItemAsync(CartId, Id);
            return Json(count);
        }

        public async Task<IActionResult> GetCartCount()
        {
            if (CurrentUser != null)
            {
                var count =  await _cartServiceClient.GetCartItemCount(CurrentUser.UserId);
                return Json(count);
            }
            return Json(0);
        }
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(AddressModel model)
        {
            if (ModelState.IsValid)
            {
                TempData.Set("Address", model);
                return RedirectToAction("Index", "Payment");
            }
            return View();
        }
    }
}