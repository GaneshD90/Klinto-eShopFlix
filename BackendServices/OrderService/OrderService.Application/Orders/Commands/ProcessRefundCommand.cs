using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class ProcessRefundCommand : ICommand<ProcessRefundResultDto>
    {
        public Guid OrderId { get; init; }
        public Guid? ReturnId { get; init; }
        public decimal RefundAmount { get; init; }
        public string RefundType { get; init; } = string.Empty;
        public string RefundMethod { get; init; } = string.Empty;
        public string RefundReason { get; init; } = string.Empty;
    }
}
