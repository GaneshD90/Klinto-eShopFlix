using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class CreateSubscriptionOrderCommand : ICommand<CreateSubscriptionResultDto>
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public string Frequency { get; init; } = string.Empty;
        public DateTime? StartDate { get; init; }
        public int? TotalOccurrences { get; init; }
        public Guid? PaymentMethodId { get; init; }
    }
}
