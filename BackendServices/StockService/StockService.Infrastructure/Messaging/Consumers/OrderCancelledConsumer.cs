using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes OrderCancelledV2 to release any reservations for the cancelled order.
/// This is a compensation consumer that restores inventory availability.
/// </summary>
public sealed class OrderCancelledConsumer : IConsumer<OrderCancelledV2>
{
    private readonly IReservationRepository _reservationRepo;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        IReservationRepository reservationRepo,
        ILogger<OrderCancelledConsumer> logger)
    {
        _reservationRepo = reservationRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "OrderCancelledConsumer received OrderCancelledV2 for OrderId={OrderId}, Reason={Reason}",
            msg.OrderId, msg.CancellationReason);

        try
        {
            // Find all reservations for this order and release them
            var reservations = await _reservationRepo.GetByOrderIdAsync(msg.OrderId, context.CancellationToken);
            var reservationList = reservations.ToList();

            if (!reservationList.Any())
            {
                _logger.LogDebug(
                    "No reservations found for OrderId={OrderId} - nothing to release",
                    msg.OrderId);
                return;
            }

            var releasedCount = 0;
            foreach (var reservation in reservationList)
            {
                // Release any reservation that hasn't been committed/shipped
                if (reservation.ReservationStatus is "Pending" or "Reserved" or "Committed")
                {
                    await _reservationRepo.ReleaseAsync(reservation.ReservationId, context.CancellationToken);
                    releasedCount++;
                }
            }

            _logger.LogInformation(
                "Released {Count} of {Total} reservations for cancelled OrderId={OrderId}",
                releasedCount, reservationList.Count, msg.OrderId);

            // Publish compensation event so other services know inventory was released
            if (releasedCount > 0)
            {
                await context.Publish(new InventoryReleasedV2(
                    ReservationId: Guid.NewGuid(),
                    OrderId: msg.OrderId,
                    ProductId: Guid.Empty, // Aggregate
                    Quantity: 0, // Would need to sum quantities
                    ReleaseReason: $"Order cancelled: {msg.CancellationReason}",
                    OccurredAt: DateTime.UtcNow));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error releasing reservations for cancelled OrderId={OrderId}", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}
