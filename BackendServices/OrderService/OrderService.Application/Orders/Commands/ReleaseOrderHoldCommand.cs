using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class ReleaseOrderHoldCommand : ICommand<ReleaseOrderHoldResultDto>
    {
        public Guid HoldId { get; init; }
        public Guid? ReleasedBy { get; init; }
        public string Notes { get; init; } = string.Empty;
    }
}
