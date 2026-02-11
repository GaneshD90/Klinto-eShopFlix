using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class PerformFraudCheckCommand : ICommand<PerformFraudCheckResultDto>
    {
        public Guid OrderId { get; init; }
        public string FraudProvider { get; init; } = string.Empty;
    }
}
