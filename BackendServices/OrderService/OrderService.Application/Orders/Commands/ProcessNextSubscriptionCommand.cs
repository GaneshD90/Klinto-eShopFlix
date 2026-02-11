using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class ProcessNextSubscriptionCommand : ICommand<ProcessNextSubscriptionResultDto>
    {
        public Guid SubscriptionId { get; init; }
    }
}
