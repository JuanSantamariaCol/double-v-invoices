using InvoicesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoicesService.Infrastructure.Persistence.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items", "billing");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(i => i.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.Property(i => i.ProductCode)
            .HasColumnName("product_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TaxRate)
            .HasColumnName("tax_rate")
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(i => i.SubTotal)
            .HasColumnName("sub_total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TaxAmount)
            .HasColumnName("tax_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Total)
            .HasColumnName("total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
