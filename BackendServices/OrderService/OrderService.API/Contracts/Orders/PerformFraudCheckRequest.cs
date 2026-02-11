using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class PerformFraudCheckRequest
    {
        public string FraudProvider { get; set; } = string.Empty;
    }
}
