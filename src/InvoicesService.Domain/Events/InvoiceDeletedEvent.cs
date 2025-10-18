namespace InvoicesService.Domain.Events;

public class InvoiceDeletedEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
