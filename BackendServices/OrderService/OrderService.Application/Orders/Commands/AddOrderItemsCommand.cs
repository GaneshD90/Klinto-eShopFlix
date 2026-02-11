using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class AddOrderItemsCommand : ICommand<AddOrderItemsResultDto>
    {
        public Guid OrderId { get; init; }
        public string OrderItemsJson { get; init; } = string.Empty;
    }
}
