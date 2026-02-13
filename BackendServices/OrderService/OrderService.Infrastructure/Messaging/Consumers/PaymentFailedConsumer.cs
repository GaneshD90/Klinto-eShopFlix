using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Services.Abstractions;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes PaymentFailedV2 as part of the order saga.
/// When payment fails, cancels the order with the failure reason.
/// </summary>
public sealed class PaymentFailedConsumer : IConsumer<PaymentFailedV2>
{
    private readonly IOrderAppService _orderService;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(
        IOrderAppService orderService,
        ILogger<PaymentFailedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "PaymentFailedConsumer received PaymentFailedV2 for OrderId={OrderId}, Reason={Reason}",
            msg.OrderId, msg.Reason);

        try
        {
            var cancelCommand = new CancelOrderCommand
            {
                OrderId = msg.OrderId,
                CancellationType = "System",
                CancellationReason = $"Payment failed: {msg.Reason}",
                CancelledByType = "System"
            };

            await _orderService.CancelAsync(cancelCommand, context.CancellationToken);

            _logger.LogInformation(
                "Order cancelled via saga due to payment failure for OrderId={OrderId}", msg.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error cancelling order for OrderId={OrderId} after payment failure", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}
