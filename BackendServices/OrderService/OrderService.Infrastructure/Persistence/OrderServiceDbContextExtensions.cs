using Contracts.Sagas.Cancellation;
using Contracts.Sagas.Checkout;
using Contracts.Sagas.ReturnRefund;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Sagas;

namespace OrderService.Infrastructure.Persistence;

/// <summary>
/// Partial class extension for OrderServiceDbContext to add saga state configuration.
/// </summary>
public partial class OrderServiceDbContext
{
    /// <summary>
    /// DbSet for persisting checkout saga state.
    /// </summary>
    public DbSet<CheckoutSagaState> CheckoutSagaStates { get; set; } = null!;

    /// <summary>
    /// DbSet for persisting cancellation saga state.
    /// </summary>
    public DbSet<CancellationSagaState> CancellationSagaStates { get; set; } = null!;

    /// <summary>
    /// DbSet for persisting return/refund saga state.
    /// </summary>
    public DbSet<ReturnRefundSagaState> ReturnRefundSagaStates { get; set; } = null!;
}

/// <summary>
/// Extension methods to apply saga state configurations.
/// </summary>
public static class OrderServiceDbContextSagaExtensions
{
    /// <summary>
    /// Applies all saga state entity configurations to the model builder.
    /// </summary>
    public static void ApplyAllSagaConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CheckoutSagaStateMap());
        modelBuilder.ApplyConfiguration(new CancellationSagaStateMap());
        modelBuilder.ApplyConfiguration(new ReturnRefundSagaStateMap());
    }

    /// <summary>
    /// Applies the checkout saga state entity configuration to the model builder.
    /// </summary>
    public static void ApplyCheckoutSagaConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CheckoutSagaStateMap());
    }

    /// <summary>
    /// Applies the cancellation saga state entity configuration to the model builder.
    /// </summary>
    public static void ApplyCancellationSagaConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CancellationSagaStateMap());
    }

    /// <summary>
    /// Applies the return/refund saga state entity configuration to the model builder.
    /// </summary>
    public static void ApplyReturnRefundSagaConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ReturnRefundSagaStateMap());
    }
}
