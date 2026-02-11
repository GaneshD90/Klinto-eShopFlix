using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class UpdateOrderStatusCommand : ICommand<OrderDetailDto>
    {
        public Guid OrderId { get; init; }
        public string NewStatus { get; init; } = string.Empty;
        public Guid? ChangedBy { get; init; }
        public string? ChangeReason { get; init; }
        public string? Notes { get; init; }
        public bool NotifyCustomer { get; init; }
    }
}
