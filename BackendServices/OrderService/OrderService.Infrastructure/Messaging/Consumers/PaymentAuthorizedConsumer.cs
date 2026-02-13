using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Services.Abstractions;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes PaymentAuthorizedV2 as part of the order saga.
/// When payment is successfully authorized, confirms the order.
/// </summary>
public sealed class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorizedV2>
{
    private readonly IOrderAppService _orderService;
    private readonly ILogger<PaymentAuthorizedConsumer> _logger;

    public PaymentAuthorizedConsumer(
        IOrderAppService orderService,
        ILogger<PaymentAuthorizedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentAuthorizedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "PaymentAuthorizedConsumer received PaymentAuthorizedV2 for OrderId={OrderId}, PaymentId={PaymentId}",
            msg.OrderId, msg.PaymentId);

        try
        {
            // Record the payment on the order
            var paymentCommand = new ProcessPaymentCommand
            {
                OrderId = msg.OrderId,
                PaymentMethod = msg.PaymentMethod,
                PaymentProvider = msg.PaymentProvider,
                Amount = msg.Amount,
                TransactionId = msg.TransactionId,
                AuthorizationCode = msg.AuthorizationCode
            };
            await _orderService.ProcessPaymentAsync(paymentCommand, context.CancellationToken);

            // Confirm the order
            var confirmCommand = new ConfirmOrderCommand { OrderId = msg.OrderId };
            var result = await _orderService.ConfirmAsync(confirmCommand, context.CancellationToken);

            _logger.LogInformation(
                "Order confirmed via saga for OrderId={OrderId}, Status={Status}",
                msg.OrderId, result.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error confirming order for OrderId={OrderId} after payment authorization", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}
