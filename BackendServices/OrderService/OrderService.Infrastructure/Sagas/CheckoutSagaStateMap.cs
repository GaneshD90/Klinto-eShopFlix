using Contracts.Sagas.Checkout;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// Entity Framework Core mapping for CheckoutSagaState.
/// Configures how the saga state is persisted to the database.
/// </summary>
public class CheckoutSagaStateMap : IEntityTypeConfiguration<CheckoutSagaState>
{
    public void Configure(EntityTypeBuilder<CheckoutSagaState> entity)
    {
        entity.ToTable("CheckoutSagaState");

        // Primary key
        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CorrelationId)
            .ValueGeneratedNever();

        // State tracking
        entity.Property(x => x.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        // Order context
        entity.Property(x => x.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(x => x.CustomerId)
            .IsRequired();

        entity.Property(x => x.CustomerEmail)
            .HasMaxLength(255)
            .IsRequired();

        entity.Property(x => x.TotalAmount)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        entity.Property(x => x.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        entity.Property(x => x.ItemCount)
            .IsRequired();

        entity.Property(x => x.CartId);

        // Progress tracking
        entity.Property(x => x.StartedAt);
        entity.Property(x => x.InventoryReservedAt);
        entity.Property(x => x.ReservationId);
        entity.Property(x => x.PaymentAuthorizedAt);
        entity.Property(x => x.PaymentId);

        entity.Property(x => x.TransactionId)
            .HasMaxLength(100);

        entity.Property(x => x.ConfirmedAt);
        entity.Property(x => x.CompletedAt);

        // Failure context
        entity.Property(x => x.FailureReason)
            .HasMaxLength(500);

        entity.Property(x => x.FailedStep)
            .HasMaxLength(50);

        entity.Property(x => x.CompensationStepsExecuted);

        // Concurrency
        entity.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Indexes
        entity.HasIndex(x => x.CurrentState);
        entity.HasIndex(x => x.OrderNumber);
        entity.HasIndex(x => x.CustomerId);
        entity.HasIndex(x => x.StartedAt);
    }
}
