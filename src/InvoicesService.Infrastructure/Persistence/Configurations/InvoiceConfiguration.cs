using InvoicesService.Domain.Entities;
using InvoicesService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoicesService.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices", "billing");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(i => i.InvoiceNumber)
            .HasColumnName("invoice_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(i => i.CustomerIdentification)
            .HasColumnName("customer_identification")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.IssueDate)
            .HasColumnName("issue_date")
            .IsRequired();

        builder.Property(i => i.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(i => i.SubTotal)
            .HasColumnName("sub_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TaxAmount)
            .HasColumnName("tax_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion(
                v => (int)v,
                v => (InvoiceStatus)v)
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasMany(i => i.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.InvoiceNumber)
            .HasDatabaseName("idx_invoice_number")
            .IsUnique();

        builder.HasIndex(i => i.CustomerId)
            .HasDatabaseName("idx_customer_id");

        builder.HasIndex(i => i.IssueDate)
            .HasDatabaseName("idx_issue_date");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("idx_status");
    }
}
