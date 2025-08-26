using eShopFlix.Web.Models;
using System.Text;
using System.Text.Json;

namespace eShopFlix.Web.HttpClients
{
    public class PaymentServiceClient
    {
        HttpClient _client;
        public PaymentServiceClient(HttpClient client)
        {
            _client = client;  
        }

        public async Task<string> CreateOrderAsync(RazorPayOrderModel orderModel)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(orderModel), Encoding.UTF8, "application/json");
            var  response =  await _client.PostAsync("payment/CreateOrder", content);
            if (response.IsSuccessStatusCode)
            {
                string OrderId = response.Content.ReadAsStringAsync().Result;
                return OrderId;
            }
            return null;
        }
      public async Task<string> VerifyPaymentAsync(PaymentConfirmModel model)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("payment/VerifyPayment", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return "";
        }

        public async Task<bool> SavePaymentDetailsAsync(TransactionModel model)
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("payment/SavePaymentDetails", content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
