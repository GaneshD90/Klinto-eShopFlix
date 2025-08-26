using CartService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetUserCart(long UserId);
        Task<int> GetCartItemCount(long UserId);
        Task<IEnumerable<CartItem>> GetCartItems(long CartId);
        Task<Cart> GetCart(long CartId);
         Task<Cart> AddItem(long UserId, CartItem item);
        Task<int> DeleteItem(long CartId, int ItemId);
        Task<bool> MakeInActive(long CartId);
        Task<int> UpdateQuantity(long CartId, int ItemId, int Quantity);
    }
}
