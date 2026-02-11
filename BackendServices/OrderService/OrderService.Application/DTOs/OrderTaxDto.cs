using System;

namespace OrderService.Application.DTOs
{
    public class OrderTaxDto
    {
        public Guid OrderTaxId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? OrderItemId { get; set; }
        public string TaxType { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxJurisdiction { get; set; } = string.Empty;
        public string TaxCalculationMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
