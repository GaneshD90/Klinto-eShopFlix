using CartService.Application.Repositories;
using CartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Infrastructure.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        CartServiceDbContext _db;
        public CartRepository(CartServiceDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Adds an item to the cart. If the cart does not exist, it retrieves the user's active cart.
        public async Task<Cart> AddItem(long UserId, CartItem item)
        {
            Cart cart = new Cart();
         
            if (item.CartId > 0) 
                cart = _db.Carts.Find(item.CartId);
            else
               cart=  await _db.Carts.Where(x=>x.UserId == UserId && x.IsActive ==true).FirstOrDefaultAsync();

            if (cart != null)
            {
                CartItem cartItem = await _db.CartItems.Where(x => x.CartId == cart.Id && x.ItemId == item.ItemId).FirstOrDefaultAsync();
                if (cartItem != null)
                {
                    // Update existing item quantity
                    cartItem.Quantity += item.Quantity;
                    await _db.SaveChangesAsync();
                    return cart;
                }
                else
                {
                    // Add new item to car
                    cart.CartItems.Add(item);
                    await _db.SaveChangesAsync();
                    return cart;
                }

            }
            else
            {
                cart = new Cart
                {
                    UserId = UserId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                cart.CartItems.Add(item);
                _db.Carts.Add(cart);
               await  _db.SaveChangesAsync();
                return cart;
            }


        }


        /// <summary>
        /// Deletes an item from the cart by CartId and ItemId.
        public  async Task<int> DeleteItem(long CartId, int ItemId)
        {
      
            return await _db.CartItems
                .Where(x => x.CartId == CartId && x.ItemId == ItemId)
                .ExecuteDeleteAsync();
        }

        /// <summary>
        /// Retrieves a cart by CartId, including its items, if it is active.
        public async Task<Cart> GetCart(long CartId)
        {
            return await _db.Carts.Include(c => c.CartItems).Where(c => c.Id == CartId && c.IsActive).FirstOrDefaultAsync();
        }


        /// <summary>
        /// Gets the total count of items in the user's active cart.        
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<int> GetCartItemCount(long UserId)
        {
            try
            {
                Cart cart = await _db.Carts.Include(c => c.CartItems).Where(c => c.UserId == UserId && c.IsActive).FirstOrDefaultAsync();
                if (cart != null)
                {
                    int counter = cart.CartItems.Sum(c => c.Quantity);
                    return counter;
                }
            }
            
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                // Console.WriteLine($"Error retrieving cart item count: {ex.Message}");
            }
            return 0;
        }

        /// <summary>
        /// Retrieves all items in a cart by CartId.
        public async  Task<IEnumerable<CartItem>> GetCartItems(long CartId)
        {
           return await _db.CartItems.Where(c => c.CartId == CartId).ToListAsync();
        }

        /// <summary>   
        /// Retrieves the user's active cart, including its items.
        public async Task<Cart> GetUserCart(long UserId)
        {
        return await _db.Carts.Include(c => c.CartItems).Where(c => c.UserId == UserId && c.IsActive == true).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Marks a cart as inactive by setting its IsActive property to false. 
        /// </summary>
        /// <param name="CartId"></param>
        /// <returns></returns>
        public async  Task<bool> MakeInActive(long CartId)
        {
             Cart cart = await _db.Carts.FindAsync(CartId);
            if (cart != null)
            {
                cart.IsActive = false;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the quantity of a specific item in the cart.
        public async Task<int> UpdateQuantity(long CartId, int ItemId, int Quantity)
        {
            CartItem cartItem = await _db.CartItems.Where(x => x.CartId == CartId && x.ItemId == ItemId).FirstOrDefaultAsync();
            if(cartItem != null)
            {
                cartItem.Quantity += Quantity;
                await _db.SaveChangesAsync();
                return 1;
            }

            return 0;
        }
    }
}
