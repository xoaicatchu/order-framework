# Wolverine Order Framework

Nền tảng ASP.NET Core đa tenant cho order management, RBAC động, reporting và transactional outbox.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Wolverine](https://img.shields.io/badge/WolverineFx-6.29.0-F26B38)
![Build](https://img.shields.io/badge/build-Release%20verified-success)

## Phạm vi hiện tại

Ứng dụng hiện cung cấp:

- REST API cho orders, roles, permissions, audit logs và reports.
- JWT bearer từ Identity Provider bên ngoài; không phát hành token local.
- Multi-tenancy lấy từ claim `tenant_id`, có query filter và tenant membership.
- Unit of Work + Repository ở Application boundary; `DbContext` chỉ nằm trong Persistence/Composition Root.
- Transactional outbox với signal, database lease, retry backoff và at-least-once delivery.
- HTTP idempotency qua header `Idempotency-Key`.
- Hybrid cache với Redis distributed cache cho production.
- Wolverine pre-generated handlers ở Production (`TypeLoadMode.Static`).

## Bắt đầu nhanh ở Development

Yêu cầu: .NET SDK 10 và PowerShell/bash.

```powershell
cd WolverineApp
dotnet restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Jwt__SecretKey = "development-only-secret-key-minimum-32-chars"
dotnet run
```

Mặc định Development dùng SQLite `orders.db`. Schema được migrate khi ứng dụng khởi động. Swagger có tại `http://localhost:5000/swagger` nếu cổng của máy khác thì dùng cổng được log ra bởi ứng dụng.

Kiểm tra nhanh:

```powershell
Invoke-WebRequest http://localhost:5000/health/live
```

`POST /api/auth/token` luôn trả `410 AUTH_PROVIDER_REQUIRED`; hãy dùng token do Identity Provider cấp.

## Chạy bằng Docker

Dockerfile đã có sẵn bước generate Wolverine code trước khi publish:

```bash
docker build -t wolverine-order-framework:latest .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Data Source=/data/orders.db" \
  -e ConnectionStrings__Redis="redis:6379" \
  -e Jwt__Authority="https://identity.example.com" \
  -v wolverine-data:/data \
  wolverine-order-framework:latest
```

Image mặc định chạy Production ở port `8080`, dùng user non-root. Redis, JWT và database phải được truyền qua secret/config của môi trường; không commit secret vào `appsettings.json`.

> Lưu ý: code hiện tại dùng SQLite provider. SQLite phù hợp cho development/smoke test; trước production cần thay provider và connection strategy bằng PostgreSQL/SQL Server managed, sau đó chạy migrations trong deployment pipeline.

## Kiến trúc thư mục

```text
WolverineApp/
├── Domain/                  # Entity, aggregate rule, domain event
├── Application/             # Command/query, DTO, interface, validation
├── Controllers/             # HTTP adapter; không truy cập DbContext
├── Infrastructure/
│   ├── Persistence/         # DbContext, UoW, repository, persistence models
│   ├── Identity/            # tenant, current user, permission service
│   ├── Messaging/           # idempotency và outbox signal
│   ├── Caching/             # hybrid cache implementation
│   ├── Reporting/           # Liquid, template store, PDF/HTML renderer
│   └── BackgroundServices/  # outbox dispatcher
├── Internal/Generated/      # Wolverine generated handler registry
└── Migrations/              # EF Core migrations

WolverineFrontend/           # Angular Report Studio: quản lý và xuất report
```

## Tài liệu theo góc nhìn

- [Mục lục tài liệu](docs/README.md)
- [Tích hợp API cho Frontend/đối tác](docs/integration/api-integration-guide.md)
- [Tổng quan kiến trúc](docs/architecture/system-overview.md)
- [Hướng dẫn developer](docs/development/developer-guide.md)
- [Reporting và template](docs/reporting/reporting-guide.md)
- [Deployment và vận hành](docs/operations/deployment-and-operations.md)
- [Production readiness](docs/operations/production-readiness.md)
- [Frontend Report Studio](WolverineFrontend/README.md)

## Kiểm chứng gần nhất

```text
dotnet build -c Release --no-restore                         PASS, 0 warning, 0 error
dotnet ef migrations has-pending-model-changes --no-build   PASS
dotnet list package --vulnerable --include-transitive       PASS
```

Repository chưa có test project tự động. Vì vậy trạng thái đúng là “sẵn sàng lên staging để kiểm thử”, chưa phải cam kết production-ready tuyệt đối.

## License và trạng thái

Đây là codebase nội bộ/reference implementation. Quyền sử dụng, license và chính sách phát hành cần được chủ sở hữu repository xác nhận trước khi phân phối cho bên ngoài.
