using System;

namespace OrderService.Application.DTOs
{
    public class OrderDiscountDto
    {
        public Guid OrderDiscountId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? OrderItemId { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public Guid? PromotionId { get; set; }
        public string DiscountName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
