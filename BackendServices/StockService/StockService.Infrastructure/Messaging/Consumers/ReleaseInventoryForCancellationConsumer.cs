using Contracts.Sagas.Cancellation;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles ReleaseInventoryForCancellation command.
/// Releases all inventory reservations for a cancelled order.
/// </summary>
public sealed class ReleaseInventoryForCancellationConsumer : IConsumer<ReleaseInventoryForCancellation>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<ReleaseInventoryForCancellationConsumer> _logger;

    public ReleaseInventoryForCancellationConsumer(
        IReservationRepository reservationRepository,
        IStockRepository stockRepository,
        ILogger<ReleaseInventoryForCancellationConsumer> logger)
    {
        _reservationRepository = reservationRepository;
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReleaseInventoryForCancellation> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "ReleaseInventoryForCancellationConsumer processing for OrderId={OrderId}, CorrelationId={CorrelationId}",
            cmd.OrderId, cmd.CorrelationId);

        var releasedQuantity = 0;

        try
        {
            var reservations = await _reservationRepository.GetByOrderIdAsync(cmd.OrderId, context.CancellationToken);
            var reservationList = reservations.ToList();

            foreach (var reservation in reservationList)
            {
                if (reservation.ReservationStatus is "Pending" or "Reserved" or "Committed")
                {
                    var stockItem = await _stockRepository.GetByIdAsync(
                        reservation.StockItemId, context.CancellationToken);

                    if (stockItem != null)
                    {
                        stockItem.AvailableQuantity += reservation.ReservedQuantity;
                        stockItem.ReservedQuantity -= reservation.ReservedQuantity;
                        stockItem.UpdatedAt = DateTime.UtcNow;
                        await _stockRepository.UpdateAsync(stockItem, context.CancellationToken);
                        releasedQuantity += reservation.ReservedQuantity;
                    }

                    await _reservationRepository.ReleaseAsync(reservation.ReservationId, context.CancellationToken);
                }
            }

            _logger.LogInformation(
                "Released {Quantity} units for cancelled OrderId={OrderId}",
                releasedQuantity, cmd.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error releasing inventory for cancellation OrderId={OrderId}", cmd.OrderId);
        }

        // Always publish completion
        await context.Publish(new InventoryReleasedForCancellation(
            CorrelationId: cmd.CorrelationId,
            OrderId: cmd.OrderId,
            ReleasedQuantity: releasedQuantity,
            OccurredAt: DateTime.UtcNow));
    }
}
