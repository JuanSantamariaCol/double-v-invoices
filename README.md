# Invoices Service

Microservicio para gestión de facturas electrónicas desarrollado con .NET 9.

## ¿Qué hace este proyecto?

Este servicio permite:
- Crear, consultar, actualizar y eliminar facturas
- Validar clientes contra el servicio de Customers
- Almacenar eventos de facturas usando el patrón Outbox
- Soportar PostgreSQL (desarrollo) y Oracle (producción)

## Tecnologías

- **.NET 9** - Framework principal
- **PostgreSQL/Oracle** - Base de datos
- **Entity Framework Core** - ORM
- **Docker** - Contenedorización
- **Swagger** - Documentación de API

## Ejecutar con Docker Compose

### Requisitos previos

- Docker Desktop instalado
- Puerto 3002 disponible (API)
- Puerto 5432 disponible (PostgreSQL)

### Pasos para ejecutar

1. **Clonar el repositorio** (si no lo has hecho)
   ```bash
   cd /ruta/del/proyecto/invoices-service
   ```

2. **Crear el archivo de variables de entorno**

   Copia el archivo de ejemplo:
   ```bash
   cp .env.docker.example .env.docker
   ```

3. **Construir la imagen Docker**
   ```bash
   docker-compose --env-file .env.docker build invoices-service
   ```

4. **Levantar los servicios**

   Con PostgreSQL:
   ```bash
   docker-compose --env-file .env.docker --profile postgres --profile invoices up -d
   ```

   Con Oracle:
   ```bash
   docker-compose --env-file .env.docker --profile oracle --profile invoices up -d
   ```

5. **Verificar que funciona**

   Abre en tu navegador: http://localhost:3002/swagger

   O prueba el endpoint de salud:
   ```bash
   curl http://localhost:3002/api/v1/health
   ```

### Comandos útiles

**Ver logs del servicio:**
```bash
docker-compose logs -f invoices-service
```

**Detener los servicios:**
```bash
docker-compose --env-file .env.docker down
```

**Detener y eliminar todo (incluyendo datos):**
```bash
docker-compose --env-file .env.docker down -v
```

**Reconstruir imagen desde cero:**
```bash
docker-compose --env-file .env.docker build --no-cache invoices-service
```

## Endpoints principales

Una vez que el servicio esté corriendo en http://localhost:3002:

- `GET /api/v1/health` - Verificar estado del servicio
- `POST /api/v1/invoices` - Crear una factura
- `GET /api/v1/invoices` - Listar facturas
- `GET /api/v1/invoices/{id}` - Obtener factura por ID
- `PUT /api/v1/invoices/{id}` - Actualizar factura
- `DELETE /api/v1/invoices/{id}` - Eliminar factura
- `GET /swagger` - Documentación interactiva de la API

## Ejemplo de uso

### Crear una factura

```bash
curl -X POST http://localhost:3002/api/v1/invoices \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "5",
    "issueDate": "2025-01-20T10:00:00Z",
    "dueDate": "2025-02-20T10:00:00Z",
    "notes": "Factura de prueba",
    "items": [
      {
        "productCode": "PROD-001",
        "description": "Producto de prueba",
        "quantity": 2,
        "unitPrice": 50000,
        "taxRate": 0.19
      }
    ]
  }'
```

### Listar facturas

```bash
curl http://localhost:3002/api/v1/invoices?page=1&pageSize=10
```

## Desarrollo sin Docker

Si prefieres ejecutar el servicio directamente con .NET:

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar el servicio
dotnet run --project src/InvoicesService.API/InvoicesService.API.csproj
```

**Nota:** Necesitarás PostgreSQL o Oracle ejecutándose localmente y configurar la cadena de conexión en `appsettings.Development.json`.

## Arquitectura

El proyecto sigue **Clean Architecture** con estas capas:

```
Domain         ’ Entidades de negocio (Invoice, InvoiceItem)
Application    ’ Casos de uso y lógica de aplicación
Infrastructure ’ Acceso a datos y servicios externos
API            ’ Controladores y endpoints REST
```

## Más información

Para detalles sobre desarrollo, migraciones de base de datos y arquitectura completa, consulta el archivo [CLAUDE.md](./CLAUDE.md).

---

**Desarrollado para FactuMarket S.A.**
