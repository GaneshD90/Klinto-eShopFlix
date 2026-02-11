using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class ConfirmOrderCommand : ICommand<ConfirmOrderResultDto>
    {
        public Guid OrderId { get; init; }
    }
}
