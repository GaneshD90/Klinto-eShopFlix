using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class MarkOrderDeliveredCommand : ICommand<MarkOrderDeliveredResultDto>
    {
        public Guid ShipmentId { get; init; }
        public string DeliverySignature { get; init; } = string.Empty;
        public string DeliveryProofImage { get; init; } = string.Empty;
    }
}
