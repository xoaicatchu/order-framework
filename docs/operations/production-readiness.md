# Production readiness

## Kết luận hiện tại

Code đã đủ điều kiện lên staging để kiểm thử tích hợp. Chưa nên tuyên bố production-ready tuyệt đối vì database provider, hạ tầng thật và automated tests chưa hoàn tất.

## Đã hoàn tất trong code

- Application/Controllers dùng Unit of Work + Repository, không truy cập trực tiếp `ApplicationDbContext`.
- `Infrastructure/Data` đổi thành `Infrastructure/Persistence`; persistence models nằm trong `Persistence/Models`.
- JWT local token endpoint bị vô hiệu hóa; identity lấy từ external provider.
- Tenant isolation, active tenant membership, dynamic RBAC và cross-tenant write protection.
- Idempotency HTTP lưu database, có request hash, TTL và replay response.
- Outbox signal, database lease, retry backoff và fallback scan 30 giây.
- L1 cache giới hạn 30 giây; Production yêu cầu Redis.
- Forwarded headers chỉ tin proxy IP được cấu hình.
- Production Wolverine static codegen, 19 generated handler types.
- Migrations hiện không có pending model changes.

## Bắt buộc trước production

- Thay SQLite bằng PostgreSQL/SQL Server managed và chạy migration trong pipeline.
- Cấu hình Redis, Identity Provider, secret manager, ingress proxy và observability thật.
- Bỏ sample seed users/roles hoặc thay bằng provisioning flow an toàn.
- Bổ sung unit, integration và security test project; chạy trong CI.
- Thiết kế dead-letter/alerting cho message outbox không retry được.
- Đảm bảo mọi consumer downstream xử lý idempotent.

## Bằng chứng kiểm tra gần nhất

```text
dotnet build -c Release --no-restore                         PASS
dotnet ef migrations has-pending-model-changes --no-build   PASS
dotnet list package --vulnerable --include-transitive       PASS
Production smoke: Wolverine Static + 19 generated handlers  PASS
```
