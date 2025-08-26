namespace eShopFlix.Web.Models
{
    public class PaymentConfirmModel
    {
        public string PaymentId { get; set; }
        public string OrderId { get; set; }
        public string Signature { get; set; }
    }
}

