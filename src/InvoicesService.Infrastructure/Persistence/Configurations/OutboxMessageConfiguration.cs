using InvoicesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoicesService.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "billing");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(o => o.AggregateId)
            .HasColumnName("aggregate_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.AggregateType)
            .HasColumnName("aggregate_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Payload)
            .HasColumnName("payload")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(o => o.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        // Indexes
        builder.HasIndex(o => new { o.Status, o.CreatedAt })
            .HasDatabaseName("idx_status_created_at");
    }
}
