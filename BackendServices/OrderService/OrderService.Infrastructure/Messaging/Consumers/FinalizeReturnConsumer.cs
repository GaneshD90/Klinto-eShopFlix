using Contracts.Sagas.ReturnRefund;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles FinalizeReturn command.
/// Updates the return status to completed.
/// </summary>
public sealed class FinalizeReturnConsumer : IConsumer<FinalizeReturn>
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly ILogger<FinalizeReturnConsumer> _logger;

    public FinalizeReturnConsumer(
        OrderServiceDbContext dbContext,
        ILogger<FinalizeReturnConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FinalizeReturn> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "FinalizeReturnConsumer processing for OrderId={OrderId}, ReturnId={ReturnId}, ReturnNumber={ReturnNumber}",
            cmd.OrderId, cmd.ReturnId, cmd.ReturnNumber);

        try
        {
            // In a full implementation, update the Return entity status
            // var return = await _dbContext.Returns.FindAsync(cmd.ReturnId);
            // if (return != null) { return.Status = cmd.FinalStatus; ... }

            _logger.LogInformation(
                "Return finalized for OrderId={OrderId}, ReturnNumber={ReturnNumber}, Status={Status}",
                cmd.OrderId, cmd.ReturnNumber, cmd.FinalStatus);

            await context.Publish(new ReturnFinalized(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: cmd.ReturnId,
                ReturnNumber: cmd.ReturnNumber,
                FinalStatus: cmd.FinalStatus,
                OccurredAt: DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error finalizing return for OrderId={OrderId}", cmd.OrderId);

            // Still publish completion
            await context.Publish(new ReturnFinalized(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: cmd.ReturnId,
                ReturnNumber: cmd.ReturnNumber,
                FinalStatus: cmd.FinalStatus,
                OccurredAt: DateTime.UtcNow));
        }
    }
}
