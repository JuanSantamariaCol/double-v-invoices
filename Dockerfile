# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files (no need for .sln in Docker)
COPY src/InvoicesService.Domain/InvoicesService.Domain.csproj ./src/InvoicesService.Domain/
COPY src/InvoicesService.Application/InvoicesService.Application.csproj ./src/InvoicesService.Application/
COPY src/InvoicesService.Infrastructure/InvoicesService.Infrastructure.csproj ./src/InvoicesService.Infrastructure/
COPY src/InvoicesService.API/InvoicesService.API.csproj ./src/InvoicesService.API/

# Restore dependencies (restore from API project includes all dependencies)
RUN dotnet restore src/InvoicesService.API/InvoicesService.API.csproj

# Copy all source code
COPY src/ ./src/

# Build the application
RUN dotnet build src/InvoicesService.API/InvoicesService.API.csproj -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish src/InvoicesService.API/InvoicesService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r invoices && useradd -r -g invoices invoices

# Copy published files
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R invoices:invoices /app

# Switch to non-root user
USER invoices

# Expose port
EXPOSE 3002

# Environment variables
ENV ASPNETCORE_URLS=http://+:3002
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:3002/api/v1/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "InvoicesService.API.dll"]
