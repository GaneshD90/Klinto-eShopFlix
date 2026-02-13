using Contracts.Events.V1;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace OrderService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes UserUpdatedV1 to update customer data in orders.
/// </summary>
public sealed class UserUpdatedConsumer : IConsumer<UserUpdatedV1>
{
    private readonly ILogger<UserUpdatedConsumer> _logger;

    public UserUpdatedConsumer(ILogger<UserUpdatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserUpdatedV1> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "UserUpdatedConsumer received UserUpdatedV1 for UserId={UserId}, Email={Email}",
            msg.UserId, msg.User?.Email);

        // In a full implementation:
        // 1. Update customer email on pending orders if changed
        // 2. Update customer name on pending orders
        // 3. Update contact information

        await Task.CompletedTask;
    }
}

/// <summary>
/// Consumes UserDeletedV1 to handle user deletion impact on orders.
/// </summary>
public sealed class UserDeletedConsumer : IConsumer<UserDeletedV1>
{
    private readonly ILogger<UserDeletedConsumer> _logger;

    public UserDeletedConsumer(ILogger<UserDeletedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserDeletedV1> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "UserDeletedConsumer received UserDeletedV1 for UserId={UserId}",
            msg.UserId);

        // In a full implementation:
        // 1. Cancel any pending orders for this user
        // 2. Anonymize completed order data per GDPR
        // 3. Log for audit purposes

        await Task.CompletedTask;
    }
}
