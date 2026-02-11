using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class AddOrderItemsRequest
    {
        public string OrderItemsJson { get; set; } = string.Empty;
    }
}
