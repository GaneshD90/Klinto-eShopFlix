using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Services.Abstractions;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes InventoryReservationFailedV2 as part of the order saga.
/// When stock reservation fails, cancels the order.
/// </summary>
public sealed class InventoryReservationFailedConsumer : IConsumer<InventoryReservationFailedV2>
{
    private readonly IOrderAppService _orderService;
    private readonly ILogger<InventoryReservationFailedConsumer> _logger;

    public InventoryReservationFailedConsumer(
        IOrderAppService orderService,
        ILogger<InventoryReservationFailedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryReservationFailedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "InventoryReservationFailedConsumer received for OrderId={OrderId}, Reason={Reason}",
            msg.OrderId, msg.Reason);

        try
        {
            var cancelCommand = new CancelOrderCommand
            {
                OrderId = msg.OrderId,
                CancellationType = "System",
                CancellationReason = $"Inventory reservation failed: {msg.Reason}",
                CancelledByType = "System"
            };

            await _orderService.CancelAsync(cancelCommand, context.CancellationToken);

            _logger.LogInformation(
                "Order cancelled via saga due to inventory reservation failure for OrderId={OrderId}",
                msg.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling order for OrderId={OrderId} after inventory reservation failure",
                msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}
