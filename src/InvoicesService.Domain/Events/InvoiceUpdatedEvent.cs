namespace InvoicesService.Domain.Events;

public class InvoiceUpdatedEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
