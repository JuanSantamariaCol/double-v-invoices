using InvoicesService.Domain.Enums;
using InvoicesService.Domain.Exceptions;

namespace InvoicesService.Domain.Entities;

public class Invoice
{
    public Guid Id { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerIdentification { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<InvoiceItem> _items = new();
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    // EF Core constructor
    private Invoice() { }

    public Invoice(
        string invoiceNumber,
        string customerId,
        string customerName,
        string customerIdentification,
        DateTime issueDate,
        DateTime dueDate,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number cannot be empty", nameof(invoiceNumber));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be empty", nameof(customerName));

        if (string.IsNullOrWhiteSpace(customerIdentification))
            throw new ArgumentException("Customer identification cannot be empty", nameof(customerIdentification));

        if (dueDate < issueDate)
            throw new InvalidInvoiceException("Due date must be greater than or equal to issue date");

        Id = Guid.NewGuid();
        InvoiceNumber = invoiceNumber;
        CustomerId = customerId;
        CustomerName = customerName;
        CustomerIdentification = customerIdentification;
        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = notes;
        Status = InvoiceStatus.Draft;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        SubTotal = 0;
        TaxAmount = 0;
        TotalAmount = 0;
    }

    public void AddItem(InvoiceItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (Status != InvoiceStatus.Draft)
            throw new InvalidInvoiceException($"Cannot add items to invoice with status {Status}");

        _items.Add(item);
        CalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid itemId)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidInvoiceException($"Cannot remove items from invoice with status {Status}");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidInvoiceException($"Item with ID {itemId} not found in invoice");

        _items.Remove(item);
        CalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void CalculateTotal()
    {
        SubTotal = Math.Round(_items.Sum(i => i.SubTotal), 2);
        TaxAmount = Math.Round(_items.Sum(i => i.TaxAmount), 2);
        TotalAmount = Math.Round(SubTotal + TaxAmount, 2);
    }

    public void MarkAsIssued()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidInvoiceException($"Cannot mark invoice as issued from status {Status}");

        if (!_items.Any())
            throw new InvalidInvoiceException("Cannot issue an invoice without items");

        Status = InvoiceStatus.Issued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Issued)
            throw new InvalidInvoiceException($"Cannot mark invoice as paid from status {Status}");

        Status = InvoiceStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new InvalidInvoiceException("Invoice is already cancelled");

        if (Status == InvoiceStatus.Paid)
            throw new InvalidInvoiceException("Cannot cancel a paid invoice");

        Status = InvoiceStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(DateTime issueDate, DateTime dueDate, string? notes)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidInvoiceException($"Cannot update invoice with status {Status}. Only Draft invoices can be updated.");

        if (dueDate < issueDate)
            throw new InvalidInvoiceException("Due date must be greater than or equal to issue date");

        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Validate()
    {
        if (!IsActive)
            throw new InvalidInvoiceException("Cannot validate an inactive invoice");

        if (!_items.Any())
            throw new InvalidInvoiceException("Invoice must have at least one item");

        if (DueDate < IssueDate)
            throw new InvalidInvoiceException("Due date must be greater than or equal to issue date");

        if (TotalAmount <= 0)
            throw new InvalidInvoiceException("Total amount must be greater than zero");
    }

    public void SoftDelete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
