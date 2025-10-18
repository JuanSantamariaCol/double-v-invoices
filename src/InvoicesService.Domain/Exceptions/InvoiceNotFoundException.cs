namespace InvoicesService.Domain.Exceptions;

public class InvoiceNotFoundException : Exception
{
    public Guid InvoiceId { get; }

    public InvoiceNotFoundException(Guid invoiceId)
        : base($"Invoice with ID '{invoiceId}' was not found")
    {
        InvoiceId = invoiceId;
    }

    public InvoiceNotFoundException(string invoiceNumber)
        : base($"Invoice with number '{invoiceNumber}' was not found")
    {
    }
}
