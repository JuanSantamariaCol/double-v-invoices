using FluentValidation;
using FluentValidation.AspNetCore;
using InvoicesService.API.Middleware;
using InvoicesService.Application.Interfaces;
using InvoicesService.Application.Mappings;
using InvoicesService.Application.Services;
using InvoicesService.Application.Validators;
using InvoicesService.Infrastructure;
using InvoicesService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateInvoiceValidator>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(InvoiceMappingProfile));

// Application Services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Infrastructure (DbContext, Repositories, External Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Invoices Service API",
        Version = "v1",
        Description = "API for managing electronic invoices"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(3001);
});

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<InvoicesDbContext>();
        app.Logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoices Service API V1");
    });
}

// Middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Invoices Service is starting...");
app.Logger.LogInformation("Listening on port 3001");

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program { }
