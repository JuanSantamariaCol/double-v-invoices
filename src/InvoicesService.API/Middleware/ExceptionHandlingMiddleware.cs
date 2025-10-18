using FluentValidation;
using InvoicesService.Application.Exceptions;
using InvoicesService.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace InvoicesService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        object error;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                error = new
                {
                    errors = validationEx.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    }).ToList()
                };
                break;

            case InvoiceNotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                error = new
                {
                    error = new
                    {
                        code = "INVOICE_NOT_FOUND",
                        message = notFoundEx.Message
                    }
                };
                break;

            case InvalidCustomerException invalidCustomerEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                error = new
                {
                    error = new
                    {
                        code = "INVALID_CUSTOMER",
                        message = invalidCustomerEx.Message
                    }
                };
                break;

            case InvalidInvoiceException invalidInvoiceEx:
                statusCode = HttpStatusCode.UnprocessableEntity;
                error = new
                {
                    error = new
                    {
                        code = "INVALID_INVOICE",
                        message = invalidInvoiceEx.Message
                    }
                };
                break;

            case ServiceUnavailableException serviceUnavailableEx:
                statusCode = HttpStatusCode.ServiceUnavailable;
                error = new
                {
                    error = new
                    {
                        code = "SERVICE_UNAVAILABLE",
                        message = serviceUnavailableEx.Message
                    }
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                error = new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An unexpected error occurred"
                    }
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(result);
    }
}
