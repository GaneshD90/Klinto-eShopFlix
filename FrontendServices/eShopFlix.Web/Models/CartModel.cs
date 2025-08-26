namespace eShopFlix.Web.Models
{
    public class CartModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public decimal Total { get; set; }
        public decimal Tax { get; set; }
        public decimal GrandTotal { get; set; }
        public List<CartItemModel> CartItems { get; set; }
    }
}
