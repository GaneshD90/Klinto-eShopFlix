using eShopFlix.Web.Models;
using System.Text.Json;

namespace eShopFlix.Web.HttpClients
{
    public class CatalogServiceClient
    {
        HttpClient _client;
        public CatalogServiceClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<ProductModel>> GetProducts()
        {
            var response = await _client.GetAsync("catalog/getall");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<ProductModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return Enumerable.Empty<ProductModel>();
        }
    }
}
