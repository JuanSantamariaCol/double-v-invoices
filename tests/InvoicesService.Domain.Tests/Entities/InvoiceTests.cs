using FluentAssertions;
using InvoicesService.Domain.Entities;
using InvoicesService.Domain.Enums;
using InvoicesService.Domain.Exceptions;

namespace InvoicesService.Domain.Tests.Entities;

public class InvoiceTests
{
    [Fact]
    public void CreateInvoice_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var invoice = new Invoice(
            "INV-2025-000001",
            "CUST-001",
            "Customer Name",
            "123456789",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            "Test notes");

        // Assert
        invoice.Should().NotBeNull();
        invoice.InvoiceNumber.Should().Be("INV-2025-000001");
        invoice.CustomerId.Should().Be("CUST-001");
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.IsActive.Should().BeTrue();
        invoice.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_ToInvoice_ShouldCalculateTotalsCorrectly()
    {
        // Arrange
        var invoice = new Invoice(
            "INV-2025-000001",
            "CUST-001",
            "Customer Name",
            "123456789",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30));

        var item = new InvoiceItem(
            "PROD-001",
            "Test Product",
            10,
            100,
            0.19m);

        // Act
        invoice.AddItem(item);

        // Assert
        invoice.Items.Should().HaveCount(1);
        invoice.SubTotal.Should().Be(1000);
        invoice.TaxAmount.Should().Be(190);
        invoice.TotalAmount.Should().Be(1190);
    }

    [Fact]
    public void MarkAsIssued_WhenDraft_ShouldChangeStatus()
    {
        // Arrange
        var invoice = new Invoice(
            "INV-2025-000001",
            "CUST-001",
            "Customer Name",
            "123456789",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30));

        invoice.AddItem(new InvoiceItem("PROD-001", "Test", 1, 100, 0.19m));

        // Act
        invoice.MarkAsIssued();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void MarkAsIssued_WithoutItems_ShouldThrowException()
    {
        // Arrange
        var invoice = new Invoice(
            "INV-2025-000001",
            "CUST-001",
            "Customer Name",
            "123456789",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30));

        // Act & Assert
        var act = () => invoice.MarkAsIssued();
        act.Should().Throw<InvalidInvoiceException>()
            .WithMessage("Cannot issue an invoice without items");
    }

    [Fact]
    public void Cancel_Invoice_ShouldMarkAsInactive()
    {
        // Arrange
        var invoice = new Invoice(
            "INV-2025-000001",
            "CUST-001",
            "Customer Name",
            "123456789",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30));

        // Act
        invoice.Cancel();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
        invoice.IsActive.Should().BeFalse();
    }
}
