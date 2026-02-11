using System;

namespace OrderService.Application.DTOs
{
    public class OrderLoyaltyPointDto
    {
        public Guid LoyaltyPointId { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public int PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public decimal PointValue { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
