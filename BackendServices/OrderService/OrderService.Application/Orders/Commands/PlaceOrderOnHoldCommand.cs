using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class PlaceOrderOnHoldCommand : ICommand<PlaceOrderOnHoldResultDto>
    {
        public Guid OrderId { get; init; }
        public string HoldType { get; init; } = string.Empty;
        public string HoldReason { get; init; } = string.Empty;
        public Guid? PlacedBy { get; init; }
        public DateTime? ExpiresAt { get; init; }
    }
}
