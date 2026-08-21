# Production readiness

## Kết luận

Backend hiện ở mức **ready for controlled staging**. Migration PostgreSQL đã được chạy thành công trên Supabase; Redis/IdP, observability và load/security test production vẫn cần được xác nhận trước go-live.

## Đã xử lý trong code

- Tách solution thành Domain, Application, Infrastructure, WebApi, ServiceDefaults, Shared, AppHost và các test project.
- Application không reference Infrastructure; các adapter persistence/auth/cache/outbox/reporting nằm ở Infrastructure.
- Production mặc định PostgreSQL; SQLite chỉ cho Development/Test. `Database:AutoMigrate=false` và `SeedDemoData=false` là mặc định.
- Permission discovery chạy theo API assembly; permission matrix không còn anonymous.
- Root permission có đường xử lý riêng; tenant role không tự nhận quyền `System:Root`.
- Hybrid cache hỗ trợ L1/L2; production yêu cầu Redis khi `Cache:RequireDistributedCache=true`.
- HTTP idempotency hash cả body và query string, scope theo tenant/user/method/path, replay response thành công và xử lý conflict.
- Outbox dùng signal để đánh thức worker, database lease atomic, retry backoff và fallback reconciliation có cấu hình; không poll dày 30 giây.
- PDF invoice giữ layout chuyên biệt; report khác không còn bị render nhầm thành invoice mà dùng bảng fallback theo data model.
- Aspire AppHost có PostgreSQL, Redis và WebApi; Docker image build từ project mới và chạy non-root.
- Có unit, functional, integration-contract và acceptance-metadata tests; Release build không warning/error.

## Còn bắt buộc trước production

1. PostgreSQL migration đã được kiểm tra trên Supabase bằng session-mode pooler; trước go-live vẫn cần backup/restore drill và kiểm tra schema/index/query provider-specific trên môi trường staging gần production.
2. Cấu hình external OIDC issuer/audience/HTTPS, claims `sub`/`tenant_id`, secret manager, CORS origin và forwarded proxy thật.
3. Cấp Redis HA, kiểm tra eviction/invalidation khi scale nhiều instance.
4. Chạy outbox consumer test với failure, duplicate delivery, lease expiry và dead-letter/alerting.
5. Chạy security test: tenant escape, RBAC root, idempotency race, request size/rate limit, report SQL/template abuse.
6. Xác nhận QuestPDF license phù hợp với mô hình phân phối/vận hành.
7. Xác nhận dataset SQL và report output trên PostgreSQL thật; SQLite chỉ là test/dev provider.

## Lệnh kiểm chứng cục bộ

```powershell
dotnet restore order-framework.slnx
dotnet build order-framework.slnx --configuration Release --no-restore
dotnet test order-framework.slnx --configuration Release --no-build --no-restore
dotnet run --project src/Order.WebApi/Order.WebApi.csproj -- codegen write
```

Migration chạy từ deployment job với `Database__Provider=postgresql` và `ConnectionStrings__MigrationConnection` (session-mode pooler/`DIRECT_URL`); request runtime dùng `ConnectionStrings__DefaultConnection` (transaction-mode pooler/`DATABASE_URL`). Không dựa vào startup auto-migrate.
