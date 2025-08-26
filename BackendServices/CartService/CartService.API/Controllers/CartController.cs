using CartService.Application.DTOs;
using CartService.Application.Services.Abstractions;
using CartService.Application.Services.Implementations;
using CartService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CartService.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        ICartAppService _cartAppService;

        public CartController(ICartAppService cartAppService)
        {
            _cartAppService = cartAppService;

        }

        [HttpGet("{UserId}")]
        public async Task<IActionResult> GetUserCart(long UserId)
        {
            var cart = await _cartAppService.GetUserCart(UserId);
            return Ok(cart);
        }

        [HttpPost("{UserId}")]
        public  async Task<IActionResult> AddItem(long UserId,CartItem item)
        {
            var cart = await _cartAppService.AddItem(UserId,item);
            return Ok(cart);
        }

        [HttpGet("{CartId}")]
        public async Task<IActionResult> GetCart(int CartId)
        {
            var cart = await _cartAppService.GetCart(CartId);
            return Ok(cart);
        }

        [HttpGet("{UserId}")]
        public async Task<IActionResult> GetCartItemCount(int UserId)
        {
            var count =  await _cartAppService.GetCartItemCount(UserId);
            return Ok(count);
        }

        [HttpGet("{CartId}")]
        public  async Task<IEnumerable<CartItemDTO>> GetItems(int CartId)
        {
            return await _cartAppService.GetCartItems(CartId);
        }

        [HttpGet("{CartId}")]
        public async Task<IActionResult> MakeInActive(int CartId)
        {
            var status = await _cartAppService.MakeInActive(CartId);
            return Ok(status);
        }

        [HttpDelete("{CartId}/{ItemId}")]
        public  async Task<IActionResult> DeleteItem(int CartId, int ItemId)
        {
            var count = await _cartAppService.DeleteItem(CartId, ItemId);
            return Ok(count);
        }

        [HttpGet("{CartId}/{ItemId}/{Quantity}")]
        public  async Task<IActionResult> UpdateQuantity(int CartId, int ItemId, int Quantity)
        {
            var count = await _cartAppService.UpdateQuantity(CartId, ItemId, Quantity);
            return Ok(count);
        }
    }
}
