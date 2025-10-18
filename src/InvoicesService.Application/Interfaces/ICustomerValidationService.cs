namespace InvoicesService.Application.Interfaces;

public interface ICustomerValidationService
{
    Task<bool> ValidateCustomerExistsAsync(string customerId, CancellationToken cancellationToken = default);
    Task<CustomerInfo?> GetCustomerInfoAsync(string customerId, CancellationToken cancellationToken = default);
}

public class CustomerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
}
