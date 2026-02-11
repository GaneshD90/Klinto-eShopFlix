using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class CancelOrderCommand : ICommand<OrderDetailDto>
    {
        public Guid OrderId { get; init; }
        public string CancellationType { get; init; } = "Customer";
        public string CancellationReason { get; init; } = string.Empty;
        public Guid? CancelledBy { get; init; }
        public string CancelledByType { get; init; } = "Customer";
    }
}
