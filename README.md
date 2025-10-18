# Invoices Service - FactuMarket S.A.

Microservicio de gestión de facturas desarrollado con .NET 9 y Clean Architecture.

## Características

- **Clean Architecture**: Separación en capas (Domain, Application, Infrastructure, API)
- **DDD**: Domain-Driven Design con entidades ricas y value objects
- **Outbox Pattern**: Garantía de consistencia transaccional en eventos
- **Dual Database**: Soporte para PostgreSQL (dev) y Oracle (prod)
- **Resiliencia**: Polly para reintentos, circuit breakers y timeouts
- **Validación**: FluentValidation para reglas de negocio
- **Integración**: Comunicación con Customers MS vía HTTP

## Tecnologías

- .NET 9.0
- Entity Framework Core 9.0
- PostgreSQL 16 / Oracle Express 21c
- Docker & Docker Compose
- AutoMapper, FluentValidation, Polly
- xUnit, FluentAssertions, Moq

## Estructura del Proyecto

```
invoices-service/
├── src/
│   ├── InvoicesService.Domain/         # Entidades, Value Objects, Eventos
│   ├── InvoicesService.Application/    # Casos de uso, DTOs, Servicios
│   ├── InvoicesService.Infrastructure/ # Repositorios, EF Core, External APIs
│   └── InvoicesService.API/             # Controllers, Middleware, Startup
├── tests/
│   ├── InvoicesService.Domain.Tests/
│   ├── InvoicesService.Application.Tests/
│   └── InvoicesService.API.Tests/
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## Requisitos Previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (opcional, para usar con Docker Compose)
- PostgreSQL 16+ o Oracle 21c+

## Configuración Rápida con Docker Compose

### 1. Configurar Variables de Ambiente

```bash
# Copiar el archivo de ejemplo
cp .env.docker.example .env.docker

# Editar .env.docker y configurar el tipo de base de datos
DATABASE_TYPE=postgres  # o "oracle"
```

### 2. Iniciar con PostgreSQL (Desarrollo)

```bash
# Solo PostgreSQL
docker-compose --env-file .env.docker --profile postgres up -d

# PostgreSQL + PGAdmin (GUI)
docker-compose --env-file .env.docker --profile postgres --profile dev-tools up -d

# PostgreSQL + Invoices Service
docker-compose --env-file .env.docker --profile postgres --profile invoices up -d
```

### 3. Iniciar con Oracle (Producción)

```bash
# Primero, asegúrate de tener acceso al Oracle Container Registry
# y haber aceptado los términos de licencia

# Cambiar en .env.docker:
# DATABASE_TYPE=oracle
# USE_ORACLE=true

# Iniciar Oracle + Invoices Service
docker-compose --env-file .env.docker --profile oracle --profile invoices up -d
```

### 4. Acceder a los Servicios

- **API**: http://localhost:3002
- **Swagger**: http://localhost:3002/swagger
- **Health Check**: http://localhost:3002/api/v1/health
- **PGAdmin** (si está activo): http://localhost:5050

## Configuración Manual (Sin Docker)

### 1. Configurar Base de Datos

**PostgreSQL**:
```bash
# Crear base de datos
createdb -U postgres invoices_dev

# Aplicar migraciones
dotnet ef database update --project src/InvoicesService.Infrastructure --startup-project src/InvoicesService.API
```

**Oracle**:
```bash
# Asegúrate de tener Oracle instalado y configurado
# Actualiza la cadena de conexión en appsettings.json
```

### 2. Configurar appsettings.json

```json
{
  "DatabaseSettings": {
    "UseOracle": false,
    "PostgresConnection": "Host=localhost;Database=invoices_dev;Username=postgres;Password=postgres",
    "OracleConnection": "User Id=system;Password=Oracle123;Data Source=localhost:1521/XE"
  },
  "ExternalServices": {
    "CustomersApiUrl": "http://localhost:3001"
  }
}
```

### 3. Restaurar Dependencias y Compilar

```bash
dotnet restore
dotnet build
```

### 4. Ejecutar Tests

```bash
dotnet test --verbosity normal
```

### 5. Ejecutar la Aplicación

```bash
dotnet run --project src/InvoicesService.API/InvoicesService.API.csproj
```

## Endpoints Principales

### Health Check
```bash
GET /api/v1/health
```

### Facturas

**Crear Factura**:
```bash
POST /api/v1/invoices
Content-Type: application/json

{
  "customerId": "5",
  "issueDate": "2025-01-15T10:00:00Z",
  "dueDate": "2025-02-15T10:00:00Z",
  "notes": "Factura de prueba",
  "items": [
    {
      "productCode": "PROD-001",
      "description": "Software License",
      "quantity": 10,
      "unitPrice": 50000,
      "taxRate": 0.19
    }
  ]
}
```

**Obtener Factura por ID**:
```bash
GET /api/v1/invoices/{id}
```

**Listar Facturas (con paginación)**:
```bash
GET /api/v1/invoices?page=1&pageSize=10
```

**Actualizar Factura**:
```bash
PUT /api/v1/invoices/{id}
```

**Eliminar Factura**:
```bash
DELETE /api/v1/invoices/{id}
```

## Variables de Ambiente (Docker Compose)

Ver archivo `.env.docker.example` para todas las variables disponibles.

**Principales**:
- `DATABASE_TYPE`: `postgres` o `oracle`
- `USE_ORACLE`: `true` o `false`
- `POSTGRES_CONNECTION`: Cadena de conexión PostgreSQL
- `ORACLE_CONNECTION`: Cadena de conexión Oracle
- `CUSTOMERS_API_URL`: URL del servicio de clientes
- `INVOICES_PORT`: Puerto del servicio (default: 3002)

## Comandos Útiles de Docker Compose

```bash
# Ver logs
docker-compose --env-file .env.docker logs -f invoices-service

# Detener servicios
docker-compose --env-file .env.docker down

# Detener y eliminar volúmenes (¡CUIDADO! Borra datos)
docker-compose --env-file .env.docker down -v

# Reconstruir imagen
docker-compose --env-file .env.docker build --no-cache invoices-service

# Ver estado de servicios
docker-compose --env-file .env.docker ps
```

## Integración con Customers Service

El servicio se integra con el microservicio de Customers para validar clientes:
- **Formato soportado**: JSON:API (Rails) y JSON plano (.NET)
- **URL**: Configurable vía `CUSTOMERS_API_URL`
- **Resiliencia**: 3 reintentos con backoff exponencial, timeout 10s

## Outbox Pattern

Las facturas generan eventos de dominio que se almacenan en la tabla `outbox_messages`:
- Garantiza consistencia transaccional
- Listo para integración con message brokers (RabbitMQ, Kafka, etc.)
- Requiere implementar BackgroundProcessor para publicación

## Testing

```bash
# Ejecutar todos los tests
dotnet test

# Con coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov

# Solo un proyecto específico
dotnet test tests/InvoicesService.Domain.Tests
```

## Migraciones de Base de Datos

```bash
# Crear nueva migración
dotnet ef migrations add MigrationName --project src/InvoicesService.Infrastructure --startup-project src/InvoicesService.API

# Aplicar migraciones
dotnet ef database update --project src/InvoicesService.Infrastructure --startup-project src/InvoicesService.API

# Revertir última migración
dotnet ef database update PreviousMigrationName --project src/InvoicesService.Infrastructure --startup-project src/InvoicesService.API
```

## Producción

### Build de Imagen Docker

```bash
docker build -t invoices-service:latest .
```

### Consideraciones de Producción

1. **Secrets**: Usa Azure Key Vault, AWS Secrets Manager o similar
2. **Database**: Configura Oracle con alta disponibilidad
3. **Monitoring**: Implementa Application Insights o Prometheus
4. **Logs**: Centraliza con ELK Stack o similar
5. **Outbox Processor**: Implementa background worker para publicar eventos

## Licencia

Propietario - FactuMarket S.A.
