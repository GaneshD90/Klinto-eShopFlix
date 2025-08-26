using eShopFlix.Web.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eShopFlix.Web.HttpClients
{
    public class AuthServiceClient
    {
        HttpClient _httpClient;
        public AuthServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserModel> LoginAsync(LoginModel model)
        {

            StringContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    return JsonSerializer.Deserialize<UserModel>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            return null;
        }
        public async Task<bool> RegisterAsync(SignUpModel model)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("auth/SignUp", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

    }
}
