using CartService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CartService.Application.HttpClients
{
    public class CatalogServiceClient
    {
        HttpClient _client;
        public CatalogServiceClient(HttpClient client)
        {
            _client = client;
        }
        public async Task<IEnumerable<ProductDTO>> GetByIdsAsync(int[] productIds)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(productIds), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("catalog/getbyids", content);
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                if (data != null)
                {
                    return JsonSerializer.Deserialize<IEnumerable<ProductDTO>>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            return null;
        }
    }
}
