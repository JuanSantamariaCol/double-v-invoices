namespace InvoicesService.Domain.Entities;

public class InvoiceItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }

    // EF Core constructor
    private InvoiceItem() { }

    public InvoiceItem(
        string productCode,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code cannot be empty", nameof(productCode));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        if (taxRate < 0 || taxRate > 1)
            throw new ArgumentException("Tax rate must be between 0 and 1", nameof(taxRate));

        Id = Guid.NewGuid();
        ProductCode = productCode;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;

        CalculateTotal();
    }

    public void CalculateTotal()
    {
        SubTotal = Math.Round(Quantity * UnitPrice, 2);
        TaxAmount = Math.Round(SubTotal * TaxRate, 2);
        Total = Math.Round(SubTotal + TaxAmount, 2);
    }

    public void UpdateQuantity(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        Quantity = quantity;
        CalculateTotal();
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        UnitPrice = unitPrice;
        CalculateTotal();
    }
}
