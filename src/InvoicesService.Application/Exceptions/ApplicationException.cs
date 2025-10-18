namespace InvoicesService.Application.Exceptions;

public class ApplicationException : Exception
{
    public ApplicationException(string message)
        : base(message)
    {
    }

    public ApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class ServiceUnavailableException : ApplicationException
{
    public ServiceUnavailableException(string serviceName, string message)
        : base($"Service '{serviceName}' is unavailable: {message}")
    {
        ServiceName = serviceName;
    }

    public ServiceUnavailableException(string serviceName, string message, Exception innerException)
        : base($"Service '{serviceName}' is unavailable: {message}", innerException)
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; }
}
