using CartService.Application.DTOs;
using CartService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Services.Abstractions
{
    public interface ICartAppService
    {
        Task<CartDTO> GetUserCart(long UserId);
        Task<int> GetCartItemCount(long UserId);
        Task<IEnumerable<CartItemDTO>> GetCartItems(long CartId);
        Task<CartDTO> GetCart(int CartId);
        Task<CartDTO> AddItem(long UserId, CartItem item);
        Task<int> DeleteItem(int CartId, int ItemId);
        Task<bool> MakeInActive(int CartId);
        Task<int> UpdateQuantity(int CartId, int ItemId, int Quantity);
    }
}
