using System.Text.Json;
using System.Text;
using eShopFlix.Web.Models;

namespace eShopFlix.Web.HttpClients
{
    public class CartServiceClient
    {
        HttpClient _client;
        public CartServiceClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<CartModel> GetUserCartAsync(long UserId)
        {
            HttpResponseMessage response = await _client.GetAsync("cart/getusercart/" + UserId);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    return JsonSerializer.Deserialize<CartModel>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            return null;
        }
        public async Task<CartModel> GetCartAsync(long CartId)
        {
            var cart = await _client.GetFromJsonAsync<CartModel>("cart/getcart/" + CartId);
            return cart ?? null;
        }

        public async Task<bool> MakeCartInActiveAsync(long CartId)
        {
            return await _client.GetFromJsonAsync<bool>("cart/makeinactive/" + CartId);
        }

        public async Task<CartModel> AddToCartAsync(CartItemModel item, long UserId)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("cart/additem/" + UserId, content);
            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<CartModel>();
                return cart;
            }
            return null;
        }

        public async Task<int> DeleteCartItemAsync(long CartId, int ItemId)
        {
            var status = await _client.DeleteFromJsonAsync<int>("cart/deleteItem/" + CartId + "/" + ItemId);
            return status;
        }

        public async Task<int> UpdateQuantity(long CartId, int ItemId, int Quantity)
        {
            var status = await _client.GetFromJsonAsync<int>("cart/UpdateQuantity/" + CartId + "/" + ItemId + "/" + Quantity);
            return status;
        }

        public async Task<int> GetCartItemCount(long UserId)
        {
            try
            {
                var counter = await _client.GetFromJsonAsync<int>("cart/GetCartItemCount/" + UserId);
                return counter;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching cart item count", ex);
            }
        }
    }
}
