using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class CreateReturnRequestCommand : ICommand<CreateReturnRequestResultDto>
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public string ReturnType { get; init; } = string.Empty;
        public string ReturnReason { get; init; } = string.Empty;
        public string ReturnItemsJson { get; init; } = string.Empty;
        public string CustomerComments { get; init; } = string.Empty;
    }
}
