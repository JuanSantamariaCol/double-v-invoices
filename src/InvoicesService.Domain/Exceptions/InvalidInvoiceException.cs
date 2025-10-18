namespace InvoicesService.Domain.Exceptions;

public class InvalidInvoiceException : Exception
{
    public InvalidInvoiceException(string message)
        : base(message)
    {
    }

    public InvalidInvoiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
