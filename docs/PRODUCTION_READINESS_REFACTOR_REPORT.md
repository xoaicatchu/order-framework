# Production-readiness refactor report

Ngày kiểm tra: 2026-08-20

## Kết luận

Code đã được refactor đáng kể và không còn tình trạng Application/Controller tự truy cập `ApplicationDbContext`. Các lỗi kiến trúc, security, cache và outbox chính đã được xử lý. Có thể đưa lên staging để kiểm thử, nhưng chưa nên gọi là production-ready tuyệt đối cho tới khi hoàn tất cấu hình hạ tầng thật và bổ sung test tự động.

## Thay đổi chính

### 1. Ranh giới kiến trúc và tên thư mục

- Domain entity vẫn nằm trong `Domain`.
- Persistence model nằm trong `Infrastructure/Persistence/Models`: `OutboxRecord`, `ProcessedMessageRecord`, `HttpIdempotencyRecord`.
- `Infrastructure/Data` được đổi thành `Infrastructure/Persistence`.
- `Infrastructure/Services` được tách theo bounded concern:
  - `Infrastructure/Identity`: `TenantProvider`, `CurrentUserProvider`, `PermissionService`.
  - `Infrastructure/Caching`: `HybridCacheService`.
  - `Infrastructure/Messaging`: `IdempotencyService`, `OutboxSignal`.
- Xóa `IApplicationDbContext` dư thừa; Application chỉ dùng port `IUnitOfWork` và `IRepository<T>`.

### 2. Unit of Work / Repository

Đã thay các truy cập DbContext trong application handlers, role handlers, role queries, `ReportsController`, reporting adapters, idempotency middleware/service, outbox processor và seed bằng repository/UoW.

`ApplicationDbContext` hiện chỉ còn ở persistence implementation, composition root, migration và health check. Đây là các vị trí hợp lệ của infrastructure.

### 3. Authentication, tenant và RBAC

- Tắt endpoint tự phát hành token giả; `/api/auth/token` trả `410 AUTH_PROVIDER_REQUIRED`.
- Bỏ secret JWT hard-code; bắt buộc `Jwt:Authority` hoặc secret từ secret manager.
- Không còn nhận tenant/user từ header hoặc giá trị mặc định trong request.
- Bỏ root wildcard bypass trong authorization.
- Role/permission query được scope theo tenant; cấm gán `System:Root` qua API.
- Cross-tenant write bị chặn trong HTTP request; startup/background internal work được tách khỏi request context.
- Bổ sung `TenantMembershipRecord`, unique theo `(TenantId, UserId)` và kiểm tra membership active khi gán role cũng như khi resolve permission.
- Forwarded headers chỉ tin các proxy IP được khai báo trong `ForwardedHeaders:KnownProxies`; không còn mở mặc định cho forwarded header từ nguồn bất kỳ.
- Startup seeding chuyển qua UoW.

### 4. Domain và API behavior

- Order status không còn setter tùy ý; chỉ cho phép transition hợp lệ qua `Confirm`, `StartProcessing`, `Ship`, `Deliver`, `Cancel`.
- Thêm giới hạn số item, quantity và unit price.
- Idempotency HTTP chuyển từ `ConcurrentDictionary` RAM sang bảng `HttpIdempotencyRecords`, có request hash, TTL, unique key theo tenant/user/method/path và replay JSON response.
- Outbox được đánh thức bằng signal sau khi transaction commit; fallback scan 30 giây thay cho polling 2 giây.
- Outbox có LockOwner, LockedUntilUtc, NextAttemptAtUtc và claim atomic trong database, tránh nhiều replica lấy cùng message.
- Retry có exponential backoff; lease tự hết hạn nếu worker chết.
- Worker được đánh thức bằng `OutboxSignal` sau commit và chỉ fallback scan mỗi 30 giây; một lần wake-up sẽ drain hết các batch đang sẵn sàng.
- Cache L1 bị giới hạn tối đa 30 giây; production fail-fast nếu chưa cấu hình Redis distributed cache.

### 5. Reporting và deployment

- Bỏ `UnsafeMemberAccessStrategy`; giới hạn Liquid template size/output/steps/recursion và cache có giới hạn.
- Dataset bắt buộc `@TenantId`, chỉ select field allowlist, filter allowlist và giới hạn 10.000 dòng.
- Health response chỉ trả status, không lộ memory/exception details.
- Swagger chỉ bật Development.
- Schema management dùng migration; `EnsureCreated` chỉ còn cho Development.
- Template `.liquid` được copy vào output/publish.
- Production dùng Wolverine `TypeLoadMode.Static`; đã generate handler registry/codegen artifacts, loại bỏ runtime handler compilation và assembly scan khi boot.
- EF Core/health-check packages đã nâng lên `10.0.11` để đồng bộ dependency.

## Kiểm chứng đã chạy

- `dotnet build --no-restore`: pass, 0 warning, 0 error.
- `dotnet build -c Release --no-restore`: pass, 0 warning, 0 error.
- `dotnet ef migrations has-pending-model-changes --no-build`: không có model changes pending.
- `dotnet ef migrations list`: 6 migration hợp lệ.
- `dotnet ef database update` trên database tạm: pass.
- `dotnet list WolverineApp.csproj package --vulnerable --include-transitive`: không có package vulnerable theo các source hiện tại.
- Runtime smoke trên database tạm:
  - `POST /api/auth/token`: `410`.
  - `GET /api/roles` không bearer token: `401`.
  - `GET /health/live`: `200 {"status":"Healthy"}`.
- Runtime Production smoke xác nhận Wolverine `Static`, load `19` pre-generated handler types và bỏ qua assembly scan.
- `dotnet publish -c Release`: pass; `Invoice_A4.liquid` có trong publish output.
- `dotnet test --no-build`: không có test project/regression test để chạy.

## Việc bắt buộc trước production

1. Dùng PostgreSQL/SQL Server managed thay SQLite mặc định và chạy migration qua deployment pipeline.
2. Consumers vẫn phải idempotent vì crash đúng sau lúc publish nhưng trước lúc đánh dấu ProcessedOnUtc vẫn có thể tạo at-least-once redelivery.
3. Thêm unit/integration/security tests và chạy trong CI; hiện repository chưa có test project nên chưa có regression gate.
4. Production bắt buộc cấu hình Redis/distributed cache; L1 cache mỗi node chỉ tồn tại tối đa 30 giây.
5. Cấu hình đúng `ForwardedHeaders:KnownProxies` theo ingress/load balancer thật, Identity Provider, secret manager, logging/metrics/tracing và bỏ sample seed users trước khi triển khai thật.
6. Khi đổi handler/message contract phải chạy lại `dotnet run -- codegen write` và commit thư mục `Internal/Generated`; CI/CD nên fail nếu codegen tạo diff.

## Ghi chú cảnh báo còn lại

- Cảnh báo EF SQLite `PRAGMA foreign_keys = 0` xuất phát từ migration cũ và là cảnh báo non-transactional operation của SQLite, không phải lỗi build. Production vẫn nên dùng PostgreSQL/SQL Server managed.
- Không còn cảnh báo Dynamic codegen trong production smoke. Development vẫn dùng Dynamic mode để thuận tiện phát triển.
