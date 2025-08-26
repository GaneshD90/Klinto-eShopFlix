using AutoMapper;
using CartService.Application.DTOs;
using CartService.Application.HttpClients;
using CartService.Application.Repositories;
using CartService.Application.Services.Abstractions;
using CartService.Domain.Entities;
using Microsoft.Extensions.Configuration;


namespace CartService.Application.Services.Implementations
{
    public class CartAppService : ICartAppService
    {
        readonly ICartRepository _cartRepository;
        readonly IMapper _mapper;
        readonly IConfiguration _configuration;
        private readonly CatalogServiceClient _catalogServiceClient;


        /// <summary>
        /// Constructor for CartAppService
        /// </summary>
        /// <param name="cartRepository"></param>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="catalogServiceClient"></param>
        public CartAppService(ICartRepository cartRepository, IMapper mapper, IConfiguration configuration, CatalogServiceClient catalogServiceClient)
        {
            _cartRepository = cartRepository;
            _mapper = mapper;
            _configuration = configuration;
            _catalogServiceClient = catalogServiceClient;
        }


        /// <summary>
        /// Populates the CartDTO with details from the Cart entity and product information.    
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        private CartDTO PopulateCartDetails(Cart cart)
        {
            try
            {
                CartDTO cartModel = _mapper.Map<CartDTO>(cart);

                var productIds = cart.CartItems.Select(x => x.ItemId).ToArray();
                var products = _catalogServiceClient.GetByIdsAsync(productIds).Result;

                if (cartModel.CartItems.Count > 0)
                {
                    cartModel.CartItems.ForEach(x =>
                    {
                        var product = products.FirstOrDefault(p => p.ProductId == x.ItemId);
                        if (product != null)
                        {
                            x.Name = product.Name;
                            x.ImageUrl = product.ImageUrl;
                        }
                    });

                    foreach (var item in cartModel.CartItems)
                    {
                        cartModel.Total += item.UnitPrice * item.Quantity;
                    }
                    cartModel.Tax = cartModel.Total * Convert.ToDecimal(_configuration["Tax"]) / 100;
                    cartModel.GrandTotal = cartModel.Total + cartModel.Tax;
                }
                return cartModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Adds an item to the cart for a specific user.
        public async  Task<CartDTO> AddItem(long UserId, CartItem item)
        { 
            CartItem cartItem = new CartItem
            {
                CartId = item.CartId,
                ItemId = item.ItemId,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                
            };
            Cart cart = await _cartRepository.AddItem(UserId,cartItem);
            return _mapper.Map<CartDTO>(cart);
        }

        /// <summary>
        /// Deletes an item from the cart by CartId and ItemId.
        public async Task<int> DeleteItem(int CartId, int ItemId)
        {
            return await _cartRepository.DeleteItem(CartId, ItemId);
        }

        /// <summary>
        /// Retrieves a cart by CartId, including its items, if it is active.   
        /// </summary>
        /// <param name="CartId"></param>
        /// <returns></returns>
        public async Task<CartDTO> GetCart(int CartId)
        {
          Cart cart = await _cartRepository.GetCart(CartId);
            if (cart == null)
            {
                return  null;
            }
            return _mapper.Map<CartDTO>(cart);
        }

        /// <summary>
        /// Gets the total count of items in the user's active cart.
        public async Task<int> GetCartItemCount(long UserId)
        {
            if(UserId> 0)
           return await _cartRepository.GetCartItemCount(UserId);
            
              return 0;
        }

        /// <summary>
        /// Gets the items in the user's cart by CartId.
        public async Task<IEnumerable<CartItemDTO>> GetCartItems(long CartId)
        {
              var data= await _cartRepository.GetCartItems(CartId);
            if (data == null)
            {
                return _mapper.Map<IEnumerable<CartItemDTO>>(null);
                
            }
            return _mapper.Map<IEnumerable<CartItemDTO>>(data);

        }

        public async Task<CartDTO> GetUserCart(long UserId)
        {
           Cart cart = await _cartRepository.GetUserCart(UserId);
            if (cart != null)
            {
                CartDTO cartmodel = PopulateCartDetails(cart);
                return  await Task.FromResult(cartmodel);
            }
            return await Task.FromResult<CartDTO>(null);

        }

        public  async Task<bool> MakeInActive(int CartId)
        {
           return await _cartRepository.MakeInActive(CartId);
        }

        public  async Task<int> UpdateQuantity(int CartId, int ItemId, int Quantity)
        {
            return await _cartRepository.UpdateQuantity(CartId, ItemId, Quantity);
        }
    }
}
