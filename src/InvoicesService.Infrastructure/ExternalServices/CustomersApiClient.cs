using InvoicesService.Application.Exceptions;
using InvoicesService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace InvoicesService.Infrastructure.ExternalServices;

public class CustomersApiClient : ICustomerValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomersApiClient> _logger;

    public CustomersApiClient(HttpClient httpClient, ILogger<CustomersApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ValidateCustomerExistsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/customers/{customerId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            _logger.LogWarning("Unexpected status code {StatusCode} when validating customer {CustomerId}",
                response.StatusCode, customerId);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error communicating with Customers API for customer {CustomerId}", customerId);
            throw new ServiceUnavailableException("CustomersAPI", "Failed to communicate with Customers service", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout when validating customer {CustomerId}", customerId);
            throw new ServiceUnavailableException("CustomersAPI", "Customers service request timed out", ex);
        }
    }

    public async Task<CustomerInfo?> GetCustomerInfoAsync(string customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/customers/{customerId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Try to parse as JSON:API format (Rails/Customers MS format)
            var jsonApiResponse = JsonSerializer.Deserialize<JsonApiResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (jsonApiResponse?.Data?.Attributes != null)
            {
                // JSON:API format - extract from nested structure
                return new CustomerInfo
                {
                    Id = jsonApiResponse.Data.Id,
                    Name = jsonApiResponse.Data.Attributes.Name,
                    Identification = jsonApiResponse.Data.Attributes.Identification
                };
            }

            // Fallback: Try plain JSON format for backward compatibility
            var plainResponse = JsonSerializer.Deserialize<PlainCustomerResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (plainResponse != null && !string.IsNullOrEmpty(plainResponse.Id))
            {
                return new CustomerInfo
                {
                    Id = plainResponse.Id,
                    Name = plainResponse.Name,
                    Identification = plainResponse.Identification
                };
            }

            _logger.LogWarning("Failed to deserialize customer response for {CustomerId} in any supported format", customerId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error communicating with Customers API for customer {CustomerId}", customerId);
            throw new ServiceUnavailableException("CustomersAPI", "Failed to communicate with Customers service", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout when getting customer info for {CustomerId}", customerId);
            throw new ServiceUnavailableException("CustomersAPI", "Customers service request timed out", ex);
        }
    }

    // JSON:API format models (Rails/Customers MS)
    private class JsonApiResponse
    {
        public JsonApiData? Data { get; set; }
    }

    private class JsonApiData
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public CustomerAttributes? Attributes { get; set; }
    }

    private class CustomerAttributes
    {
        public string Name { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PersonType { get; set; }
        public string? Address { get; set; }
    }

    // Plain JSON format (for backward compatibility)
    private class PlainCustomerResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
    }
}
