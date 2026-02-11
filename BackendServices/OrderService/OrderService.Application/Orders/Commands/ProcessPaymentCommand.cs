using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class ProcessPaymentCommand : ICommand<ProcessPaymentResultDto>
    {
        public Guid OrderId { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public string PaymentProvider { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string TransactionId { get; init; } = string.Empty;
        public string AuthorizationCode { get; init; } = string.Empty;
        public string PaymentGatewayResponse { get; init; } = string.Empty;
    }
}
