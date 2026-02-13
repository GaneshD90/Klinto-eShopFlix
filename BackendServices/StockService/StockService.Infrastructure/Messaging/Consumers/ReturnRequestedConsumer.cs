using Contracts.Events.V2;
using MassTransit;
using Microsoft.Extensions.Logging;
using StockService.Application.Repositories;

namespace StockService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes ReturnRequestedV2 to prepare inventory for restock.
/// When a customer requests a return, this consumer prepares the stock system
/// to receive the items back into inventory.
/// </summary>
public sealed class ReturnRequestedConsumer : IConsumer<ReturnRequestedV2>
{
    private readonly IReservationRepository _reservationRepo;
    private readonly ILogger<ReturnRequestedConsumer> _logger;

    public ReturnRequestedConsumer(
        IReservationRepository reservationRepo,
        ILogger<ReturnRequestedConsumer> logger)
    {
        _reservationRepo = reservationRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReturnRequestedV2> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "ReturnRequestedConsumer received ReturnRequestedV2 for OrderId={OrderId}, ReturnId={ReturnId}, ReturnType={ReturnType}",
            msg.OrderId, msg.ReturnId, msg.ReturnType);

        try
        {
            // In a full implementation, you would:
            // 1. Create a "pending restock" record for each item being returned
            // 2. Update inventory projections to show expected incoming stock
            // 3. Generate restock tasks for warehouse workers
            
            // For now, log the return request for audit
            _logger.LogInformation(
                "Return request {ReturnNumber} for OrderId={OrderId} received. Reason: {Reason}. " +
                "Warehouse will prepare for restock upon receipt.",
                msg.ReturnNumber, msg.OrderId, msg.ReturnReason);

            // If this is an exchange or immediate return, we might want to
            // pre-allocate bin locations for the returned items
            if (msg.ReturnType == "Exchange" || msg.ReturnType == "Immediate")
            {
                _logger.LogInformation(
                    "Priority return ({ReturnType}) - flagging for expedited processing",
                    msg.ReturnType);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ReturnRequestedV2 for OrderId={OrderId}", msg.OrderId);
            throw; // Let MassTransit retry
        }
    }
}
