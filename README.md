# Wolverine Order Framework

ASP.NET Core/.NET 10 backend cho order management với multi-tenancy, dynamic RBAC, reporting, idempotency, hybrid cache và transactional outbox.

## 📐 Kiến trúc

**Clean Architecture** theo 4 tầng:

```
┌─────────────────────────────────────────┐
│  Order.WebApi (HTTP Entry Point)        │
│  - REST Controllers                     │
│  - Wolverine Message Handlers (static)  │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  Order.Application (Business Logic)     │
│  - CQRS: Commands & Queries             │
│  - Application Ports (Interfaces)       │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  Order.Infrastructure (Adapters)        │
│  - EF Core + Repository Pattern         │
│  - HybridCache (Memory + Redis)         │
│  - OIDC/JWT, Permission Service         │
│  - Liquid Reporting, PDF/HTML Renderers │
│  - Outbox Pattern + Background Processor│
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│  Order.Domain (Business Rules)          │
│  - Aggregates: Order, OrderItem         │
│  - RBAC: Role, Permission, User         │
│  - Domain Events (publishing)           │
│  - Reporting Models                     │
└─────────────────────────────────────────┘
```

**Layer Dependencies:**
- Application → Domain only
- Infrastructure → Domain + Application
- WebApi → All layers (composition root)
- **No circular dependencies**

---

## 📁 Cấu trúc

```
src/
├── Order.Domain/              # Business rules (aggregates, events)
├── Order.Application/         # Use cases (CQRS commands/queries)
├── Order.Infrastructure/      # Adapters (persistence, cache, auth, reporting)
├── Order.WebApi/              # HTTP entry point (controllers, DI setup)
├── Order.ServiceDefaults/     # Shared service registration
└── Order.AppHost/             # Aspire local orchestration

tests/                          # Unit, Functional, Integration, Acceptance

WolverineFrontend/             # Angular Report Studio
```

---

## 🚀 Quick Start

### Development (SQLite auto-created)

```bash
dotnet restore order-framework.slnx
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/Order.WebApi/Order.WebApi.csproj
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/health/live

### With PostgreSQL + Redis (Aspire)

```bash
dotnet run --project src/Order.AppHost/Order.AppHost.csproj
```

Opens dashboard at http://localhost:18888

### Frontend (Angular)

```bash
cd WolverineFrontend
pnpm install
pnpm start
```

Open http://localhost:4200

### Build & Test

```bash
dotnet build order-framework.slnx --configuration Release
dotnet test order-framework.slnx --configuration Release
dotnet run --project src/Order.WebApi/Order.WebApi.csproj -- codegen write
```

### Docker

```bash
docker build -t order-framework:local .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=db;Database=order_framework;Username=postgres;Password=***" \
  -e ConnectionStrings__Redis="redis:6379" \
  -e Jwt__Authority="https://identity.example.com" \
  order-framework:local
```

---

## 🔐 Tính năng chính

| Tính năng | Triển khai |
|----------|-----------|
| Multi-Tenancy | IMultiTenant interface, tenant isolation |
| Dynamic RBAC | AppRole/AppPermission, DynamicPermissionPolicyProvider |
| OIDC/JWT | External identity provider |
| Domain Events | Publishing + Outbox pattern |
| HTTP Idempotency | Idempotency-Key header |
| Caching | HybridCache (L1: Memory, L2: Redis) |
| Transactional Outbox | Polling-based relay |
| Soft Delete | Global query filter |
| Audit Trail | CreatedBy, ModifiedBy, timestamps |
| Reporting | Liquid templates, PDF/HTML rendering |

---

## 📚 Tài liệu

Xem `docs/README.md` để truy cập toàn bộ tài liệu theo vai trò:

- **Architects**: [System Overview](docs/architecture/system-overview.md)
- **Backend Devs**: [Developer Guide](docs/development/developer-guide.md)
- **Frontend/Partners**: [API Integration](docs/integration/api-integration-guide.md)
- **BA/Reporting**: [Reporting Guide](docs/reporting/reporting-guide.md)
- **DevOps/SRE**: [Deployment & Operations](docs/operations/deployment-and-operations.md)
- **Release Managers**: [Production Readiness](docs/operations/production-readiness.md)

---

## 🧪 Testing

- `Order.Domain.UnitTests` - Business rules
- `Order.Application.UnitTests` - Command/Query handlers
- `Order.Application.FunctionalTests` - Architecture boundaries
- `Order.Infrastructure.IntegrationTests` - Persistence, caching, outbox
- `Order.WebApi.AcceptanceTests` - End-to-end API

---

## 💾 Tech Stack

C# 13 | .NET 10 | ASP.NET Core | Wolverine | PostgreSQL | Redis | EF Core | FluentValidation | AutoMapper | Liquid | QuestPDF | Angular 19

---

**Last Updated**: 2026-08-22
