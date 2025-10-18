# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Invoices Service is a .NET 9 microservice for managing electronic invoices at FactuMarket S.A. It implements Clean Architecture with Domain-Driven Design principles and includes the Outbox Pattern for reliable event publishing.

## Development Commands

### Build & Run
```bash
# Restore and build
dotnet restore
dotnet build

# Run the API (port 3002)
dotnet run --project src/InvoicesService.API/InvoicesService.API.csproj

# Run with Docker Compose (PostgreSQL)
docker-compose --env-file .env.docker --profile postgres --profile invoices up -d

# Run with Docker Compose (Oracle)
docker-compose --env-file .env.docker --profile oracle --profile invoices up -d
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/InvoicesService.Domain.Tests
dotnet test tests/InvoicesService.Application.Tests

# Run with verbose output
dotnet test --verbosity normal

# Run single test
dotnet test --filter "CreateInvoice_WithValidData_ShouldSucceed"
```

### Database Migrations
```bash
# Create new migration
dotnet ef migrations add MigrationName \
  --project src/InvoicesService.Infrastructure \
  --startup-project src/InvoicesService.API

# Apply migrations
dotnet ef database update \
  --project src/InvoicesService.Infrastructure \
  --startup-project src/InvoicesService.API

# Revert to specific migration
dotnet ef database update PreviousMigrationName \
  --project src/InvoicesService.Infrastructure \
  --startup-project src/InvoicesService.API
```

## Architecture

### Clean Architecture Layers

The project follows strict Clean Architecture with dependency flow: **Domain ← Application ← Infrastructure ← API**

**Domain Layer** (`InvoicesService.Domain`):
- Contains core business entities with encapsulated logic (Invoice, InvoiceItem, OutboxMessage)
- Value Objects (Money, InvoiceNumber) - immutable objects representing domain concepts
- Domain Events (InvoiceCreatedEvent, InvoiceUpdatedEvent, InvoiceDeletedEvent)
- Repository interfaces (NO implementations)
- Domain Exceptions (InvalidInvoiceException, InvalidCustomerException)
- **Key Rule**: No dependencies on other layers or frameworks

**Application Layer** (`InvoicesService.Application`):
- Use cases and business workflows implemented in services (InvoiceService)
- DTOs for requests/responses with clear naming: CreateInvoiceRequest, InvoiceResponse
- FluentValidation validators (CreateInvoiceValidator, UpdateInvoiceValidator)
- AutoMapper profiles for entity-DTO mapping
- Interface definitions for external services (ICustomerValidationService)
- **Key Rule**: Depends only on Domain layer

**Infrastructure Layer** (`InvoicesService.Infrastructure`):
- EF Core DbContext (InvoicesDbContext) with fluent configurations per entity
- Repository implementations (InvoiceRepository, OutboxRepository)
- Unit of Work pattern implementation for transactional consistency
- External service clients (CustomersApiClient with Polly resilience)
- Database provider switching: PostgreSQL (dev) / Oracle (prod)
- DependencyInjection.cs registers all infrastructure services
- **Key Rule**: Implements interfaces from Application/Domain

**API Layer** (`InvoicesService.API`):
- REST controllers (InvoicesController, HealthController)
- Middleware pipeline: RequestLoggingMiddleware → ExceptionHandlingMiddleware
- Startup configuration in Program.cs
- Swagger/OpenAPI documentation
- **Key Rule**: Only depends on Application layer contracts

### Critical Architectural Patterns

**Outbox Pattern Implementation**:
- Invoice creation/update generates OutboxMessage in SAME transaction as Invoice
- See InvoiceService.cs lines 82-97: both Invoice and OutboxMessage saved via UnitOfWork
- OutboxMessages table stores domain events for eventual publishing to message broker
- Status: 'pending' → ready for background processor (not yet implemented)
- Guarantees: Event published IFF invoice persisted (transactional consistency)

**Dual Database Support**:
- Configured in DependencyInjection.cs based on `UseOracle` flag
- PostgreSQL: Development/Testing (via Npgsql)
- Oracle: Production (via Oracle.EntityFrameworkCore)
- Same DbContext works with both providers
- Connection strings in appsettings.json or .env.docker

**External Service Integration**:
- CustomersApiClient validates customers via HTTP to Customers MS (port 3001)
- Supports JSON:API format (Rails) AND plain JSON (.NET) - auto-detection
- Polly resilience: 3 retries with exponential backoff + circuit breaker
- Timeout: 10 seconds
- See CustomersApiClient.cs lines 67-82 for JSON:API parsing logic

**Rich Domain Entities**:
- Invoice entity has private setters and encapsulated business logic
- Methods like AddItem(), CalculateTotals(), MarkAsIssued() enforce invariants
- Items collection exposed as IReadOnlyCollection to prevent external mutations
- EF Core uses private constructors for rehydration

## Configuration

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "PostgreSqlConnection": "...",
    "OracleConnection": "..."
  },
  "UseOracle": false,
  "ExternalServices": {
    "CustomersApi": {
      "BaseUrl": "http://localhost:3001",
      "Timeout": 10,
      "RetryCount": 3
    }
  }
}
```

### Docker Compose Profiles
- `postgres`: PostgreSQL database only
- `oracle`: Oracle database only
- `invoices`: Invoices Service
- `dev-tools`: PGAdmin for database management

Switch databases by editing `.env.docker`:
```bash
DATABASE_TYPE=postgres  # or "oracle"
USE_ORACLE=false        # or "true"
```

## Testing Strategy

### Unit Tests Location
- Domain tests: `tests/InvoicesService.Domain.Tests/Entities/`
- Application tests: `tests/InvoicesService.Application.Tests/Services/`
- Uses xUnit, FluentAssertions, Moq

### Test Naming Convention
```
MethodName_Scenario_ExpectedBehavior
```
Example: `CreateInvoiceAsync_WithInvalidCustomer_ShouldThrowInvalidCustomerException`

### Mocking External Dependencies
- Mock ICustomerValidationService for customer validation
- Mock IInvoiceRepository for data access
- Use InMemory database only for integration tests (not unit tests)

## Key Business Rules

1. **Invoice Creation**:
   - Customer MUST exist (validated via Customers MS)
   - Due date >= Issue date
   - Invoice must have at least 1 item
   - Invoice number auto-generated: INV-YYYY-NNNNNN

2. **Invoice Items**:
   - Quantity must be > 0
   - UnitPrice must be > 0
   - TaxRate typically 0.19 (19% VAT in Colombia)
   - Calculations: SubTotal = Quantity × UnitPrice, TaxAmount = SubTotal × TaxRate

3. **Invoice Totals**:
   - Automatically recalculated when items added/removed
   - SubTotal = sum of all item SubTotals
   - TaxAmount = sum of all item TaxAmounts
   - TotalAmount = SubTotal + TaxAmount

4. **Outbox Events**:
   - Generated for: invoice.created, invoice.updated, invoice.deleted
   - Payload contains: InvoiceId, InvoiceNumber, CustomerId, CustomerName, TotalAmount, Status

## Important Implementation Notes

**When adding new entities**:
1. Create entity in Domain/Entities with private setters
2. Add EF Core configuration in Infrastructure/Persistence/Configurations
3. Create migration via `dotnet ef migrations add`
4. Add repository interface in Domain/Interfaces
5. Implement repository in Infrastructure/Persistence/Repositories
6. Register in DependencyInjection.cs

**When modifying Invoice creation/update**:
- ALWAYS use UnitOfWork.BeginTransactionAsync() before changes
- ALWAYS create OutboxMessage in same transaction
- ALWAYS call UnitOfWork.CommitTransactionAsync() at end
- See InvoiceService.CreateInvoiceAsync for reference pattern

**When integrating with external services**:
- Define interface in Application/Interfaces
- Implement in Infrastructure/ExternalServices
- Add Polly policies in DependencyInjection.cs
- Configure HttpClient with base URL and timeout

## Database Schema

**billing.invoices** - Main invoice table
**billing.invoice_items** - Line items (1-to-many)
**billing.outbox_messages** - Event outbox for reliable publishing

All tables use UUID primary keys. Schema name: `billing`

## Endpoints

- `GET /api/v1/health` - Health check
- `POST /api/v1/invoices` - Create invoice
- `GET /api/v1/invoices` - List invoices (paginated)
- `GET /api/v1/invoices/{id}` - Get invoice by ID
- `PUT /api/v1/invoices/{id}` - Update invoice
- `DELETE /api/v1/invoices/{id}` - Delete invoice (soft delete)
- `GET /swagger` - API documentation (Development only)

## Environment Variables (Docker)

See `.env.docker.example` for full list. Key variables:
- `DATABASE_TYPE`: postgres | oracle
- `USE_ORACLE`: true | false
- `POSTGRES_CONNECTION`: PostgreSQL connection string
- `ORACLE_CONNECTION`: Oracle connection string
- `CUSTOMERS_API_URL`: URL of Customers microservice
