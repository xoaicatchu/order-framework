# Deployment và vận hành

## Đối tượng đọc

DevOps, SRE và đội triển khai môi trường staging/production.

## Docker

```bash
docker build -t wolverine-order-framework:latest .
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Data Source=/data/orders.db" \
  -e ConnectionStrings__Redis="redis:6379" \
  -e Jwt__Authority="https://identity.example.com" \
  -v wolverine-data:/data \
  wolverine-order-framework:latest
```

Image chạy `Production`, port `8080`, user non-root và đã chứa Wolverine generated handlers.

## Environment bắt buộc

| Key | Bắt buộc | Ý nghĩa |
|---|---:|---|
| `ConnectionStrings__DefaultConnection` | Có | Connection string database |
| `ConnectionStrings__Redis` | Production | Distributed cache |
| `Jwt__Authority` | Khuyến nghị | OIDC/OAuth authority của Identity Provider |
| `Jwt__SecretKey` | Thay thế Authority | Chỉ dùng khi hệ thống xác thực JWT bằng symmetric key |
| `Jwt__Issuer` | Khi dùng secret | Issuer phải khớp token |
| `Jwt__Audience` | Khi dùng secret | Audience phải khớp token |
| `Jwt__RequireHttpsMetadata` | Có | Giữ `true` ở production |
| `Database__SchemaManagement` | Có | Production phải là `migrate` |
| `ForwardedHeaders__KnownProxies__0` | Theo ingress | IP của proxy/load balancer tin cậy |

Không đặt secret trong file config commit vào repository.

## Database và migration

1. Backup database.
2. Chạy migration trong pipeline với connection string secret.
3. Kiểm tra migration history.
4. Start application.
5. Kiểm tra `/health/ready`.

Code hiện đang đăng ký SQLite provider. Trước production cần thay provider trong `Program.cs`/project package và kiểm thử toàn bộ migration trên database managed.

## Health endpoints

| Endpoint | Dùng cho | Kết quả |
|---|---|---|
| `GET /health/live` | Liveness | `200 {"status":"Healthy"}` khi process sống |
| `GET /health/ready` | Readiness | Kiểm tra database và memory |
| `GET /health` | Tổng hợp | Health report tổng hợp |

Health response cố ý chỉ trả status, không lộ connection string hay exception.

## Outbox vận hành

- Sau transaction commit, signal đánh thức worker ngay.
- Nếu không có signal, worker fallback scan mỗi 30 giây.
- Claim dùng `LockOwner`/`LockedUntilUtc`; lease tự hết hạn khi worker chết.
- Retry dùng backoff; message lỗi vĩnh viễn cần được quan sát và xử lý theo consumer policy.
- Consumer phải idempotent vì delivery là at-least-once.

## Logging và correlation

Ứng dụng ghi structured JSON ra console. Client có thể gửi `X-Correlation-Id`; nếu không gửi, server tự sinh và trả lại cùng header. Khi xử lý incident, tìm theo correlation ID, tenant ID và user ID trong hệ thống log tập trung.
