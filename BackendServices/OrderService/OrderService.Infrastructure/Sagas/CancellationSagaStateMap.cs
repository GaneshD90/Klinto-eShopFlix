using Contracts.Sagas.Cancellation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// Entity Framework Core mapping for CancellationSagaState.
/// </summary>
public class CancellationSagaStateMap : IEntityTypeConfiguration<CancellationSagaState>
{
    public void Configure(EntityTypeBuilder<CancellationSagaState> entity)
    {
        entity.ToTable("CancellationSagaState");

        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CorrelationId).ValueGeneratedNever();

        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();

        // Order context
        entity.Property(x => x.OrderId).IsRequired();
        entity.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.CustomerId).IsRequired();
        entity.Property(x => x.CustomerEmail).HasMaxLength(255).IsRequired();
        entity.Property(x => x.OrderAmount).HasColumnType("decimal(18,4)").IsRequired();
        entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        // Cancellation context
        entity.Property(x => x.CancellationType).HasMaxLength(50);
        entity.Property(x => x.CancellationReason).HasMaxLength(500);
        entity.Property(x => x.CancelledBy);

        // Progress tracking
        entity.Property(x => x.RequestedAt);
        entity.Property(x => x.StockReleasedAt);
        entity.Property(x => x.RefundInitiatedAt);
        entity.Property(x => x.CompletedAt);

        // Payment context
        entity.Property(x => x.PaymentId);
        entity.Property(x => x.RefundId);
        entity.Property(x => x.RefundAmount).HasColumnType("decimal(18,4)");
        entity.Property(x => x.RefundTransactionId).HasMaxLength(100);

        // Failure context
        entity.Property(x => x.FailureReason).HasMaxLength(500);
        entity.Property(x => x.FailedStep).HasMaxLength(50);

        // Concurrency
        entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        // Indexes
        entity.HasIndex(x => x.CurrentState);
        entity.HasIndex(x => x.OrderId);
        entity.HasIndex(x => x.RequestedAt);
    }
}
