namespace InvoicesService.Domain.Exceptions;

public class InvalidCustomerException : Exception
{
    public string CustomerId { get; }

    public InvalidCustomerException(string customerId)
        : base($"Customer with ID '{customerId}' does not exist or is invalid")
    {
        CustomerId = customerId;
    }

    public InvalidCustomerException(string customerId, string message)
        : base(message)
    {
        CustomerId = customerId;
    }
}
