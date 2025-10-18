using InvoicesService.Application.Interfaces;
using InvoicesService.Domain.Interfaces;
using InvoicesService.Infrastructure.BackgroundServices;
using InvoicesService.Infrastructure.ExternalServices;
using InvoicesService.Infrastructure.Persistence;
using InvoicesService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace InvoicesService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration
        var connectionString = configuration.GetConnectionString("OracleConnection");
        var useOracle = !string.IsNullOrEmpty(connectionString) && configuration.GetValue<bool>("UseOracle", false);

        if (useOracle)
        {
            services.AddDbContext<InvoicesDbContext>(options =>
                options.UseOracle(connectionString));
        }
        else
        {
            // Use PostgreSQL for development
            var postgresConnection = configuration.GetConnectionString("PostgreSqlConnection")
                ?? "Host=localhost;Port=5432;Database=invoices_dev;Username=postgres;Password=postgres";

            services.AddDbContext<InvoicesDbContext>(options =>
                options.UseNpgsql(postgresConnection));
        }

        // Repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // External Services
        var customersApiBaseUrl = configuration.GetValue<string>("ExternalServices:CustomersApi:BaseUrl") ?? "http://localhost:3001";
        var customersApiTimeout = configuration.GetValue<int>("ExternalServices:CustomersApi:Timeout", 10);
        var customersApiRetryCount = configuration.GetValue<int>("ExternalServices:CustomersApi:RetryCount", 3);

        services.AddHttpClient<ICustomerValidationService, CustomersApiClient>(client =>
        {
            client.BaseAddress = new Uri(customersApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(customersApiTimeout);
        })
        .AddPolicyHandler(GetRetryPolicy(customersApiRetryCount))
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Background Services
        services.AddHostedService<OutboxPublisherService>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Retrying request (attempt {retryAttempt}) after {timespan.TotalSeconds} seconds");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
