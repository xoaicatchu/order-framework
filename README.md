# Wolverine Order Framework

ASP.NET Core/.NET 10 backend cho order management, multi-tenancy, RBAC động, reporting, idempotency, hybrid cache và transactional outbox.

## Trạng thái hiện tại

Solution đã được tách theo boundary vật lý và build Release sạch. Development dùng SQLite; production mặc định PostgreSQL, Redis và external OIDC/JWT. Production không tự migrate schema và không seed dữ liệu demo.

Đã chạy migration hardening thành công trên PostgreSQL Supabase được cấu hình cho môi trường này. Backend ở mức ready for controlled staging; production thực tế vẫn cần chốt IdP, secret manager, CORS/proxy, observability và security/load test.

## Cấu trúc

```text
src/
├── Order.Domain/          # Aggregate, entity, rule, domain event
├── Order.Application/     # Use case, DTO, validator, port/interface
├── Order.Infrastructure/ # EF persistence, cache, auth adapter, outbox, reporting
├── Order.WebApi/          # HTTP composition root, controllers, generated Wolverine code
├── Order.ServiceDefaults/ # Service composition boundary
├── Order.Shared/          # Shared contracts tối thiểu
└── Order.AppHost/         # Aspire local orchestration: PostgreSQL + Redis + API
tests/                     # Unit, functional, integration, acceptance
WolverineFrontend/         # Angular Report Studio
```

`Application` không reference `Infrastructure`. EF Core/DbContext chỉ được compose tại Infrastructure/WebApi; repository và Unit of Work là port ở Application. Các query cũ còn dùng `IQueryable` được ghi nhận trong ADR để tiếp tục refactor theo từng use case.

## Chạy development

```powershell
dotnet restore order-framework.slnx
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/Order.WebApi/Order.WebApi.csproj
```

Development tự dùng `src/Order.WebApi/appsettings.Development.json`, SQLite `orders.dev.db`, auto-migrate và seed demo. Swagger ở `/swagger`; health ở `/health/live`, `/health/ready`.

Chạy cả PostgreSQL + Redis qua Aspire:

```powershell
dotnet run --project src/Order.AppHost/Order.AppHost.csproj
```

## Build, test và codegen

```powershell
dotnet build order-framework.slnx --configuration Release
dotnet test order-framework.slnx --configuration Release
dotnet run --project src/Order.WebApi/Order.WebApi.csproj -- codegen write
```

Production dùng Wolverine static codegen. Migration phải chạy bằng deployment job/`dotnet ef database update` với `Database__Provider=postgresql`; không bật auto-migration trong production. Runtime dùng `ConnectionStrings__DefaultConnection` qua transaction pooler (6543); migration dùng `ConnectionStrings__MigrationConnection` qua session pooler (5432).

## Docker

```bash
docker build -t order-framework:local .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=order_framework;Username=postgres;Password=change-me" \
  -e ConnectionStrings__Redis="host.docker.internal:6379" \
  -e Jwt__Authority="https://identity.example.com" \
  order-framework:local
```

Image chạy non-root ở port `8080`. Không đưa secret vào appsettings hoặc image.

## Tài liệu theo góc nhìn

- [Mục lục tài liệu](docs/README.md)
- [Tích hợp API cho frontend/đối tác](docs/integration/api-integration-guide.md)
- [Reporting: dataset, template, input/output](docs/reporting/reporting-guide.md)
- [Tổng quan kiến trúc](docs/architecture/system-overview.md)
- [Developer guide](docs/development/developer-guide.md)
- [Deployment và vận hành](docs/operations/deployment-and-operations.md)
- [Production readiness](docs/operations/production-readiness.md)
- [Frontend Report Studio](WolverineFrontend/README.md)

Không commit/push được thực hiện trong lần refactor này.
