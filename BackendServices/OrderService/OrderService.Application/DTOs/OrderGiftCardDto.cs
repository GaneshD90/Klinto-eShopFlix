using System;

namespace OrderService.Application.DTOs
{
    public class OrderGiftCardDto
    {
        public Guid GiftCardId { get; set; }
        public Guid OrderId { get; set; }
        public string GiftCardCode { get; set; } = string.Empty;
        public decimal InitialAmount { get; set; }
        public decimal AppliedAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
