# Wolverine Order Framework

ASP.NET Core/.NET 10 backend cho order management với multi-tenancy, dynamic RBAC, reporting, idempotency, hybrid cache và transactional outbox. Xây dựng theo **Clean Architecture** với layer separation rõ ràng.

## Trạng thái dự án

- ✅ Clean Architecture với Domain-Driven Design
- ✅ Multi-tenancy isolation đầy đủ
- ✅ Dynamic RBAC (Role-Based Access Control)
- ✅ Transactional Outbox pattern
- ✅ Hybrid Cache (HybridCache)
- ✅ HTTP Idempotency
- ✅ PostgreSQL + Redis orchestration (Aspire)
- ✅ Production-ready deployment (Docker)
- 🟡 Angular Report Studio (proto)

---

## 📐 Kiến trúc hệ thống (C4 Model)

### Level 1: System Context

```
┌─────────────────────────────────────────────────────┐
│                   Wolverine System                   │
│  (Order Management, Reporting, Multi-Tenant)        │
└──────────────┬──────────────────────────────────────┘
               │
       ┌───────┼───────┐
       │       │       │
   ┌───▼──┐ ┌──▼───┐ ┌─▼────────┐
   │ Web  │ │ OIDC/│ │ External │
   │Portal│ │ JWT  │ │ Services │
   └──────┘ └──────┘ └──────────┘
       │
   ┌───▼─────────────────┐
   │  Wolverine API      │
   │  (REST + gRPC)      │
   └─────────────────────┘
```

### Level 2: Container Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Wolverine Framework                         │
│                                                                   │
│  ┌──────────────────┐  ┌────────────────────────────────────┐   │
│  │  WolverineFrontend│  │     Order.WebApi (ASP.NET)        │   │
│  │   (Angular SPA)   │  │  - REST Controllers               │   │
│  │                   │  │  - Wolverine Message Handler      │   │
│  │  - Report Studio  │  │  - Middleware Pipeline            │   │
│  │  - Live API Conn  │  │  - Composition Root               │   │
│  └──────────────────┘  └────────────────────────────────────┘   │
│        │                            │                             │
│        └────────────────┬───────────┘                             │
│                         │ HTTP/JSON                              │
│  ┌──────────────────────▼──────────────────────────────────┐    │
│  │  Order.Application (Business Logic Layer)              │    │
│  │  ┌─────────────────────────────────────────────────┐   │    │
│  │  │  Commands & Queries (CQRS)                      │   │    │
│  │  │  - CancelOrder, CreateOrder, UpdateOrderStatus  │   │    │
│  │  │  - CreateRole, AssignUserRoles                  │   │    │
│  │  │  - GetOrderStatistics, ListOrders               │   │    │
│  │  └─────────────────────────────────────────────────┘   │    │
│  │  ┌─────────────────────────────────────────────────┐   │    │
│  │  │  Validators & DTOs                              │   │    │
│  │  │  - Input validation (FluentValidation)          │   │    │
│  │  │  - DTO mapping & transformation                 │   │    │
│  │  └─────────────────────────────────────────────────┘   │    │
│  │  ┌─────────────────────────────────────────────────┐   │    │
│  │  │  Application Ports (Interfaces)                 │   │    │
│  │  │  - IRepository, IUnitOfWork                      │   │    │
│  │  │  - ICacheService, IIdempotencyService           │   │    │
│  │  │  - IReportEngine, ITenantProvider               │   │    │
│  │  └─────────────────────────────────────────────────┘   │    │
│  └──────────────────────────────────────────────────────────┘   │
│                         │                                        │
│                         │ Port/Adapter (Dependency Inversion)   │
│  ┌──────────────────────▼──────────────────────────────────┐   │
│  │  Order.Infrastructure (Technology Layer)              │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Persistence Adapters                             │  │   │
│  │  │ - EF Core (ApplicationDbContext)                 │  │   │
│  │  │ - Repository<T> (Generic Repository Pattern)    │  │   │
│  │  │ - Unit of Work                                  │  │   │
│  │  │ - DbInitializer (Seeding)                       │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Caching & Messaging                              │  │   │
│  │  │ - HybridCache (Memory + Distributed)             │  │   │
│  │  │ - IdempotencyService                             │  │   │
│  │  │ - OutboxSignal                                   │  │   │
│  │  │ - OutboxBackgroundProcessor (Hosted Service)     │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Identity & Authorization                         │  │   │
│  │  │ - OIDC/JWT Integration                           │  │   │
│  │  │ - DynamicPermissionPolicyProvider                │  │   │
│  │  │ - PermissionService (RBAC Logic)                 │  │   │
│  │  │ - TenantProvider (Multi-tenancy Isolation)       │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Reporting                                        │  │   │
│  │  │ - LiquidReportEngine (Template Engine)           │  │   │
│  │  │ - SemanticDatasetService (Field Catalog)         │  │   │
│  │  │ - Renderers (HTML, PDF via QuestPDF)            │  │   │
│  │  │ - TemplateStores (DB & FileSystem)              │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Middleware                                       │  │   │
│  │  │ - Correlation ID                                │  │   │
│  │  │ - Idempotency Key                               │  │   │
│  │  │ - Cache Invalidation                            │  │   │
│  │  │ - Validation Exception Handling                 │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────────┘   │
│                         │                                        │
│  ┌──────────────────────▼──────────────────────────────────┐   │
│  │  Order.Domain (Business Rules Layer)                   │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Aggregates & Entities                            │  │   │
│  │  │ - Order (Aggregate Root)                         │  │   │
│  │  │ - OrderItem (Value)                              │  │   │
│  │  │ - OrderStatus (Enum)                             │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Domain Events                                    │  │   │
│  │  │ - OrderCreatedDomainEvent                        │  │   │
│  │  │ - OrderCancelledDomainEvent                      │  │   │
│  │  │ - OrderStatusChangedDomainEvent                  │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Identity & Security                              │  │   │
│  │  │ - AppRole, AppPermission                         │  │   │
│  │  │ - AppUserRole, AppRolePermission                 │  │   │
│  │  │ - IMultiTenant, ISoftDeletable                  │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────┐  │   │
│  │  │ Reporting Domain Models                          │  │   │
│  │  │ - ReportConfiguration                            │  │   │
│  │  │ - ReportTemplate (Liquid)                        │  │   │
│  │  │ - SemanticDataset (Field Catalog)               │  │   │
│  │  └──────────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
        │                                           │
        │                                    ┌──────┴──────┐
        │                                    │             │
    ┌───▼────────┐              ┌────────────▼─┐  ┌──────▼────┐
    │ PostgreSQL │              │ Redis Cluster│  │  External │
    │ (Dev/Prod) │              │ (Cache/Msgs) │  │  OIDC IdP  │
    └────────────┘              └──────────────┘  └────────────┘
```

### Level 3: Component Diagram (Order.Application)

```
┌───────────────────────────────────────────────────────┐
│            Order.Application Layer                     │
│                                                         │
│  Commands                  Queries                     │
│  ├─ CreateOrder           ├─ GetOrderById            │
│  ├─ CancelOrder           ├─ ListOrders              │
│  ├─ UpdateOrderStatus     ├─ GetOrderStatistics      │
│  ├─ CreateRole            ├─ GetRoles                │
│  ├─ AssignUserRoles       ├─ GetPermissions          │
│  │                         └─ GetPermissionsMatrix    │
│  │                                                    │
│  └─ Handlers (Wolverine)   └─ Handlers (Wolverine)   │
│                                                         │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Application Ports (Interfaces)                  │ │
│  │                                                  │ │
│  │  ┌────────────────────────────────────────────┐ │ │
│  │  │ IRepository<T>        - Generic CRUD       │ │ │
│  │  │ IUnitOfWork            - Transaction mgmt  │ │ │
│  │  │ ICacheService          - Cache operations  │ │ │
│  │  │ IIdempotencyService    - Idempotency key  │ │ │
│  │  │ IPermissionService     - Permission check │ │ │
│  │  │ ITenantProvider        - Tenant isolation │ │ │
│  │  │ ITenantMembershipService - User-Tenant    │ │ │
│  │  │ ICurrentUserProvider   - Request context  │ │ │
│  │  │ IReportEngine          - Liquid engine    │ │ │
│  │  │ ISemanticDatasetService - Field catalog   │ │ │
│  │  │ IDocumentRenderer      - PDF/HTML render  │ │ │
│  │  │ IOutboxSignal          - Outbox signal    │ │ │
│  │  └────────────────────────────────────────────┘ │ │
│  └──────────────────────────────────────────────────┘ │
│                                                         │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Common Infrastructure                           │ │
│  │  ├─ DTOs (Order, Role, AuditLog)               │ │
│  │  ├─ Mappings (AutoMapper Config)               │ │
│  │  ├─ Validators (FluentValidation)              │ │
│  │  ├─ Exceptions (BusinessConfirmation, etc.)    │ │
│  │  ├─ Authorization Attributes                   │ │
│  │  └─ Cache Keys                                 │ │
│  └──────────────────────────────────────────────────┘ │
│                                                         │
└───────────────────────────────────────────────────────┘
```

### Level 4: Infrastructure Components

**Persistence Layer:**
- `ApplicationDbContext` - EF Core DbContext with Fluent API
- `Repository<T>` - Generic repository implementing Active Record pattern
- `UnitOfWork` - Transaction coordination
- `DbInitializer` - Seeding and migrations
- **Interceptors**: `AuditableEntityInterceptor` (CreatedBy, ModifiedBy timestamps)

**Caching Layer:**
- `HybridCacheService` - Wraps .NET 9 HybridCache (L1: Memory, L2: Redis)
- `CacheKeys` - Centralized cache key definitions
- **Invalidation**: CacheInvalidationMiddleware

**Identity & Authorization:**
- `CurrentUserProvider` - Extracts user from JWT claims
- `TenantProvider` - Tenant isolation from header/claim
- `PermissionService` - Dynamic permission checking
- `DynamicPermissionPolicyProvider` - ASP.NET Core authorization policy
- `TenantMembershipService` - User-tenant associations

**Messaging & Reliability:**
- `IdempotencyService` - HTTP idempotency via Idempotency-Key header
- `OutboxSignal` - Domain event to outbox
- `OutboxBackgroundProcessor` - Polling-based outbox relay
- **Health Checks**: Outbox, Memory

**Reporting:**
- `LiquidReportEngine` - Liquid template rendering
- `SemanticDatasetService` - Field catalog API (tenant-safe)
- `Renderers`: HtmlDocumentRenderer, QuestPdfDocumentRenderer
- `TemplateStores`: DbReportTemplateStore, FileSystemReportTemplateStore

---

## 📁 Cấu trúc Thư mục

```
order-framework/
├── src/
│   ├── Order.Domain/
│   │   ├── Orders/              # Order Aggregate
│   │   ├── Identity/            # RBAC entities
│   │   ├── Audit/              # Audit log
│   │   ├── Reporting/          # Report domain models
│   │   ├── Events/             # Domain events
│   │   └── Common/             # Base classes, interfaces
│   │
│   ├── Order.Application/
│   │   ├── Commands/           # CQRS Commands
│   │   ├── Queries/            # CQRS Queries
│   │   ├── Common/             # Ports, DTOs, mappings
│   │   ├── Events/             # Application event handlers
│   │   └── Queries.Reporting/  # Report queries (future)
│   │
│   ├── Order.Infrastructure/
│   │   ├── Persistence/        # EF Core, Repository, UoW
│   │   ├── Caching/            # HybridCache service
│   │   ├── Identity/           # Auth providers, permission service
│   │   ├── Auth/               # OIDC/JWT adapters
│   │   ├── Messaging/          # Idempotency, Outbox
│   │   ├── Middleware/         # HTTP pipeline
│   │   ├── Reporting/          # Liquid, PDF/HTML renderers
│   │   ├── BackgroundServices/ # Outbox processor
│   │   ├── Health/             # Health check implementations
│   │   ├── Migrations/         # EF Core migrations
│   │   └── Options/            # Configuration classes
│   │
│   ├── Order.WebApi/
│   │   ├── Controllers/        # HTTP REST endpoints
│   │   ├── Program.cs          # Host configuration
│   │   ├── Generated/          # Wolverine codegen (static)
│   │   └── appsettings.*.json  # Environment configs
│   │
│   ├── Order.ServiceDefaults/  # Shared service registration
│   ├── Order.Shared/           # Shared DTOs (if needed)
│   └── Order.AppHost/          # Aspire local orchestration
│
├── tests/
│   ├── Order.Domain.UnitTests/
│   ├── Order.Application.UnitTests/
│   ├── Order.Application.FunctionalTests/
│   ├── Order.Infrastructure.IntegrationTests/
│   └── Order.WebApi.AcceptanceTests/
│
├── WolverineFrontend/          # Angular Report Studio
│   ├── src/app/
│   ├── package.json
│   └── angular.json
│
├── docs/                       # Comprehensive documentation
│   ├── architecture/           # System design, decisions
│   ├── integration/            # API contracts
│   ├── development/            # Developer onboarding
│   ├── reporting/              # Report engine guide
│   ├── operations/             # Deployment, observability
│   └── README.md               # Doc index
│
└── Root Files:
    ├── order-framework.slnx    # Solution file
    ├── Directory.Build.props   # Global project settings
    ├── Directory.Packages.props# Centralized NuGet versions
    ├── Dockerfile             # Production container
    ├── global.json            # .NET SDK version lock
    └── api-tests.http         # REST Client tests
```

---

## 🏛️ Layer Boundary Rules

### Domain Layer (Order.Domain)
- ✅ No external dependencies
- ✅ Pure C#, business logic only
- ✅ DTOs prohibited (use domain models)
- ✅ Framework-agnostic

### Application Layer (Order.Application)
- ✅ **Depends on Domain only**
- ✅ CQRS Command/Query patterns
- ✅ Validators (FluentValidation)
- ✅ DTOs for input/output
- ✅ Ports (interfaces) to Infrastructure
- ❌ No direct EF Core, no direct database access
- ❌ No HTTP/Controller references

### Infrastructure Layer (Order.Infrastructure)
- ✅ Implements Application ports
- ✅ Depends on Domain + Application
- ✅ EF Core, Redis, OIDC adapters
- ✅ Background services
- ❌ No Application references circular

### WebApi Layer (Order.WebApi)
- ✅ HTTP entry point (Controllers)
- ✅ Composition root (Wolverine + DI)
- ✅ Middleware pipeline
- ✅ Depends on all layers
- ✅ Wolverine static codegen handlers

---

## 🚀 Quick Start

### Prerequisites
- .NET 10 SDK
- PostgreSQL 14+ (production) / SQLite (dev auto-created)
- Redis 6+ (optional, production)
- Node.js 18+ (for Angular)

### Development

```bash
# Restore dependencies
dotnet restore order-framework.slnx

# Run WebAPI (SQLite + auto-migration)
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/Order.WebApi/Order.WebApi.csproj

# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
# Health: http://localhost:5000/health/live
```

### With Aspire (PostgreSQL + Redis)

```bash
dotnet run --project src/Order.AppHost/Order.AppHost.csproj

# Opens dashboard at http://localhost:18888
```

### Frontend (Angular)

```bash
cd WolverineFrontend
pnpm install
pnpm start

# Opens http://localhost:4200
```

### Build, Test, Codegen

```bash
# Release build
dotnet build order-framework.slnx --configuration Release

# Run all tests
dotnet test order-framework.slnx --configuration Release

# Generate Wolverine handlers (static codegen)
dotnet run --project src/Order.WebApi/Order.WebApi.csproj -- codegen write
```

### Docker

```bash
# Build image
docker build -t order-framework:local .

# Run (with PostgreSQL + Redis)
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=db.example.com;Database=order_framework;Username=postgres;Password=***" \
  -e ConnectionStrings__Redis="redis.example.com:6379" \
  -e Jwt__Authority="https://identity.example.com" \
  order-framework:local
```

---

## 🔐 Key Features

| Feature | Implementation | Status |
|---------|-----------------|--------|
| **Multi-Tenancy** | IMultiTenant interface, tenant isolation in queries | ✅ |
| **Dynamic RBAC** | AppRole, AppPermission, DynamicPermissionPolicyProvider | ✅ |
| **OIDC/JWT** | ASP.NET Core Authentication, external IdP integration | ✅ |
| **Domain Events** | OrderCreatedDomainEvent, published to outbox | ✅ |
| **Idempotency** | Idempotency-Key header, stored responses | ✅ |
| **Caching** | HybridCache (L1: Memory, L2: Redis) | ✅ |
| **Outbox Pattern** | Transactional, polling-based relay | ✅ |
| **Soft Delete** | ISoftDeletable, global query filter | ✅ |
| **Audit Trail** | BaseAuditableEntity (CreatedBy, ModifiedBy) | ✅ |
| **Reporting** | Liquid templates, PDF/HTML rendering | ✅ |

---

## 📚 Documentation

All documentation is maintained in `docs/`:

| Document | Audience | Purpose |
|----------|----------|---------|
| [System Overview](docs/architecture/system-overview.md) | Architects, Senior Devs | C4 diagrams, request flows, boundaries |
| [API Integration Guide](docs/integration/api-integration-guide.md) | Frontend, Partners | REST endpoints, auth, request/response |
| [Developer Guide](docs/development/developer-guide.md) | Backend Devs | Add use cases, handlers, migrations |
| [Reporting Guide](docs/reporting/reporting-guide.md) | BA, Reporting Teams | Dataset, Liquid templates, rendering |
| [Deployment & Operations](docs/operations/deployment-and-operations.md) | DevOps, SRE | Docker, environment, health, migration |
| [Production Readiness](docs/operations/production-readiness.md) | Release Managers | Checklist, known limitations |
| [Architecture Decisions](docs/architecture/decisions/) | All | ADR-0001, ADR-0002 rationale |

---

## 🧪 Testing Strategy

```
Domain Layer:
├── Order.Domain.UnitTests
│   └── Order status transitions, rules validation
│
Application Layer:
├── Order.Application.UnitTests
│   └── Command/Query handlers (mocked repos)
├── Order.Application.FunctionalTests
│   └── Architecture boundary tests (no layer violations)
│
Infrastructure Layer:
├── Order.Infrastructure.IntegrationTests
│   └── Persistence, caching, outbox processing
│
WebApi Layer:
└── Order.WebApi.AcceptanceTests
    └── End-to-end API + authorization metadata
```

---

## 📊 Tech Stack

| Layer | Technology |
|-------|-----------|
| **Language** | C# 13, .NET 10 |
| **Web Framework** | ASP.NET Core 10 Minimal APIs |
| **CQRS/Messaging** | Wolverine (static codegen) |
| **Database** | PostgreSQL (prod), SQLite (dev) |
| **ORM** | Entity Framework Core 10 |
| **Caching** | HybridCache + Redis |
| **Identity** | OIDC/JWT (external IdP) |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **Reporting** | Liquid templates + QuestPDF |
| **Frontend** | Angular 19 + TypeScript |
| **Containerization** | Docker + Aspire |
| **Testing** | xUnit, Moq, Testcontainers |

---

## 📋 Production Checklist

Before going live:

- [ ] Secret manager configured (not appsettings)
- [ ] PostgreSQL connection string verified
- [ ] Redis cluster configured + health checks
- [ ] OIDC/JWT external IdP tested
- [ ] Outbox background processor monitored
- [ ] Health check endpoints enabled
- [ ] Correlation ID logging in all services
- [ ] API rate limiting configured
- [ ] CORS policy restricted
- [ ] Database migration strategy documented
- [ ] Backup & recovery tested

See [Production Readiness](docs/operations/production-readiness.md) for details.

---

## 📝 Contributing

1. Follow the Clean Architecture boundaries
2. Add tests before/alongside features
3. Update `docs/` when adding major features
4. Run codegen before committing: `dotnet run --project src/Order.WebApi/Order.WebApi.csproj -- codegen write`
5. All layer boundaries enforced by `ArchitectureBoundaryTests`

---

## 📄 License

[Add your license]

---

## 📞 Support

- 📖 Read the docs: `docs/README.md`
- 🐛 Report issues: [GitHub Issues]
- 💬 Discussions: [GitHub Discussions]

---

**Last Updated**: 2026-08-22 | **Version**: 1.0.0-alpha
