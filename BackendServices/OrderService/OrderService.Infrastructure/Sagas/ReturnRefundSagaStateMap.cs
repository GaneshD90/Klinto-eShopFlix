using Contracts.Sagas.ReturnRefund;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderService.Infrastructure.Sagas;

/// <summary>
/// Entity Framework Core mapping for ReturnRefundSagaState.
/// </summary>
public class ReturnRefundSagaStateMap : IEntityTypeConfiguration<ReturnRefundSagaState>
{
    public void Configure(EntityTypeBuilder<ReturnRefundSagaState> entity)
    {
        entity.ToTable("ReturnRefundSagaState");

        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CorrelationId).ValueGeneratedNever();

        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();

        // Order context
        entity.Property(x => x.OrderId).IsRequired();
        entity.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.CustomerId).IsRequired();
        entity.Property(x => x.CustomerEmail).HasMaxLength(255).IsRequired();

        // Return context
        entity.Property(x => x.ReturnId);
        entity.Property(x => x.ReturnNumber).HasMaxLength(50);
        entity.Property(x => x.ReturnType).HasMaxLength(50);
        entity.Property(x => x.ReturnReason).HasMaxLength(500);
        entity.Property(x => x.CustomerComments).HasMaxLength(1000);
        entity.Property(x => x.TotalItemsToReturn);

        // Progress tracking
        entity.Property(x => x.RequestedAt);
        entity.Property(x => x.ValidatedAt);
        entity.Property(x => x.ReceivedAt);
        entity.Property(x => x.RestockedAt);
        entity.Property(x => x.RefundProcessedAt);
        entity.Property(x => x.CompletedAt);

        // Validation context
        entity.Property(x => x.IsValidated);
        entity.Property(x => x.ValidationNotes).HasMaxLength(500);
        entity.Property(x => x.RequiresInspection);

        // Restock context
        entity.Property(x => x.ItemsRestocked);
        entity.Property(x => x.WarehouseId);

        // Refund context
        entity.Property(x => x.PaymentId);
        entity.Property(x => x.RefundId);
        entity.Property(x => x.RefundAmount).HasColumnType("decimal(18,4)");
        entity.Property(x => x.CurrencyCode).HasMaxLength(3);
        entity.Property(x => x.RefundTransactionId).HasMaxLength(100);
        entity.Property(x => x.RefundMethod).HasMaxLength(50);

        // Failure context
        entity.Property(x => x.FailureReason).HasMaxLength(500);
        entity.Property(x => x.FailedStep).HasMaxLength(50);

        // Concurrency
        entity.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        // Indexes
        entity.HasIndex(x => x.CurrentState);
        entity.HasIndex(x => x.OrderId);
        entity.HasIndex(x => x.ReturnNumber);
        entity.HasIndex(x => x.RequestedAt);
    }
}
