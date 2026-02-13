using Contracts.Events.V1;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CartService.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes UserRegisteredV1 to create a cart for new users or sync user data.
/// </summary>
public sealed class UserRegisteredConsumer : IConsumer<UserRegisteredV1>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredV1> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "UserRegisteredConsumer received UserRegisteredV1 for UserId={UserId}, Email={Email}",
            msg.UserId, msg.User?.Email);

        // In a full implementation:
        // 1. Create a cart for the new user (or lazy-create on first add)
        // 2. Store user preferences
        // 3. Apply any welcome promotions

        await Task.CompletedTask;
    }
}

/// <summary>
/// Consumes UserUpdatedV1 to update user data in carts.
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
            "UserUpdatedConsumer received UserUpdatedV1 for UserId={UserId}",
            msg.UserId);

        // Update any cached user data in cart service

        await Task.CompletedTask;
    }
}
