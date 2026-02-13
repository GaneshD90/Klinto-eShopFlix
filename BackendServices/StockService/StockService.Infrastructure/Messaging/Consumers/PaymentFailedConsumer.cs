using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes PaymentFailedV2 as saga compensation.
/// Releases any inventory that was reserved for this order.
/// </summary>
public sealed class PaymentFailedConsumer : IConsumer<PaymentFailedV2>
{
    private readonly IReservationRepository _reservationRepo;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(
        IReservationRepository reservationRepo,
        ILogger<PaymentFailedConsumer> logger)
    {
        _reservationRepo = reservationRepo;
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
            // Find all reservations for this order and release them
            var reservations = await _reservationRepo.GetByOrderIdAsync(msg.OrderId, context.CancellationToken);

            var releasedCount = 0;
            foreach (var reservation in reservations)
            {
                if (reservation.ReservationStatus is "Pending" or "Reserved")
                {
                    await _reservationRepo.ReleaseAsync(reservation.ReservationId, context.CancellationToken);
                    releasedCount++;
                }
            }

            _logger.LogInformation(
                "Released {Count} reservations for OrderId={OrderId} due to payment failure",
                releasedCount, msg.OrderId);

            // Publish compensation event so other services know inventory was released
            await context.Publish(new InventoryReleasedV2(
                ReservationId: Guid.NewGuid(),
                OrderId: msg.OrderId,
                ProductId: Guid.Empty,
                Quantity: 0,
                ReleaseReason: $"Payment failed: {msg.Reason}",
                OccurredAt: DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error releasing inventory for OrderId={OrderId} after payment failure", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}
