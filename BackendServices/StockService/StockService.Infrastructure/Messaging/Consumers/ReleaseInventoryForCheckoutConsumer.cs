using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles ReleaseInventoryForCheckout command.
/// Called by the Checkout Saga orchestrator as compensation when payment fails.
/// </summary>
public sealed class ReleaseInventoryForCheckoutConsumer : IConsumer<ReleaseInventoryForCheckout>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<ReleaseInventoryForCheckoutConsumer> _logger;

    public ReleaseInventoryForCheckoutConsumer(
        IReservationRepository reservationRepository,
        IStockRepository stockRepository,
        ILogger<ReleaseInventoryForCheckoutConsumer> logger)
    {
        _reservationRepository = reservationRepository;
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReleaseInventoryForCheckout> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "ReleaseInventoryForCheckoutConsumer processing command for OrderId={OrderId}, CorrelationId={CorrelationId}, Reason={Reason}",
            cmd.OrderId, cmd.CorrelationId, cmd.Reason);

        try
        {
            // Find all reservations for this order
            var reservations = await _reservationRepository.GetByOrderIdAsync(cmd.OrderId, context.CancellationToken);
            var reservationList = reservations.ToList();

            if (!reservationList.Any())
            {
                _logger.LogInformation(
                    "No reservations found for OrderId={OrderId} - nothing to release",
                    cmd.OrderId);
            }
            else
            {
                var releasedCount = 0;
                foreach (var reservation in reservationList)
                {
                    if (reservation.ReservationStatus is "Pending" or "Reserved")
                    {
                        // Get the stock item and restore quantity
                        var stockItem = await _stockRepository.GetByIdAsync(
                            reservation.StockItemId, context.CancellationToken);

                        if (stockItem != null)
                        {
                            stockItem.AvailableQuantity += reservation.ReservedQuantity;
                            stockItem.ReservedQuantity -= reservation.ReservedQuantity;
                            stockItem.UpdatedAt = DateTime.UtcNow;
                            await _stockRepository.UpdateAsync(stockItem, context.CancellationToken);
                        }

                        // Release the reservation
                        await _reservationRepository.ReleaseAsync(reservation.ReservationId, context.CancellationToken);
                        releasedCount++;
                    }
                }

                _logger.LogInformation(
                    "Released {Count} reservations for OrderId={OrderId}",
                    releasedCount, cmd.OrderId);
            }

            // Publish completion event
            await context.Publish(new InventoryReleasedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Inventory release completed for OrderId={OrderId}",
                cmd.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error releasing inventory for OrderId={OrderId}", cmd.OrderId);
            
            // Still publish completion - compensation should not fail the saga
            await context.Publish(new InventoryReleasedForCheckout(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                OccurredAt: DateTime.UtcNow));
        }
    }
}
