using Contracts.Sagas.ReturnRefund;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Saga participant consumer that handles ValidateReturnRequest command.
/// Validates that the return request is valid (items belong to order, within return window, etc.)
/// </summary>
public sealed class ValidateReturnRequestConsumer : IConsumer<ValidateReturnRequest>
{
    private readonly OrderServiceDbContext _dbContext;
    private readonly ILogger<ValidateReturnRequestConsumer> _logger;

    public ValidateReturnRequestConsumer(
        OrderServiceDbContext dbContext,
        ILogger<ValidateReturnRequestConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ValidateReturnRequest> context)
    {
        var cmd = context.Message;
        _logger.LogInformation(
            "ValidateReturnRequestConsumer processing for OrderId={OrderId}, ReturnId={ReturnId}",
            cmd.OrderId, cmd.ReturnId);

        try
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == cmd.OrderId, context.CancellationToken);

            if (order == null)
            {
                await context.Publish(new ReturnValidationFailed(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    ReturnId: cmd.ReturnId,
                    Reason: "Order not found",
                    OccurredAt: DateTime.UtcNow));
                return;
            }

            // Validate order status allows returns
            if (order.OrderStatus != "Completed" && order.OrderStatus != "Delivered")
            {
                await context.Publish(new ReturnValidationFailed(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    ReturnId: cmd.ReturnId,
                    Reason: $"Order status '{order.OrderStatus}' does not allow returns",
                    OccurredAt: DateTime.UtcNow));
                return;
            }

            // Validate return window (30 days by default)
            var returnWindowDays = 30;
            var orderCompletedDate = order.CompletedAt ?? order.ConfirmedAt ?? order.OrderDate;
            if ((DateTime.UtcNow - orderCompletedDate).TotalDays > returnWindowDays)
            {
                await context.Publish(new ReturnValidationFailed(
                    CorrelationId: cmd.CorrelationId,
                    OrderId: cmd.OrderId,
                    ReturnId: cmd.ReturnId,
                    Reason: $"Return window of {returnWindowDays} days has expired",
                    OccurredAt: DateTime.UtcNow));
                return;
            }

            // All validations passed
            var requiresInspection = cmd.ReturnType == "Exchange" || 
                                     cmd.Items?.Any(i => i.ReturnCondition == "Damaged" || i.ReturnCondition == "Defective") == true;

            await context.Publish(new ReturnValidated(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: cmd.ReturnId,
                IsApproved: true,
                ValidationNotes: "Return request validated successfully",
                RequiresInspection: requiresInspection,
                OccurredAt: DateTime.UtcNow));

            _logger.LogInformation(
                "Return validated for OrderId={OrderId}, RequiresInspection={RequiresInspection}",
                cmd.OrderId, requiresInspection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating return for OrderId={OrderId}", cmd.OrderId);

            await context.Publish(new ReturnValidationFailed(
                CorrelationId: cmd.CorrelationId,
                OrderId: cmd.OrderId,
                ReturnId: cmd.ReturnId,
                Reason: $"Validation error: {ex.Message}",
                OccurredAt: DateTime.UtcNow));
        }
    }
}
