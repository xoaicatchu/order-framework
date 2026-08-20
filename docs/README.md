# Tài liệu dự án

Tài liệu được chia theo người đọc và mục đích sử dụng. Mỗi tài liệu phải trả lời rõ: ai dùng, dùng khi nào, input là gì, output/kết quả là gì.

## Theo góc nhìn

| Góc nhìn | Tài liệu | Mục đích |
|---|---|---|
| Kiến trúc | [System overview](architecture/system-overview.md) | Hiểu boundary, luồng request, tenant, outbox và các giới hạn hiện tại |
| Frontend/đối tác | [API integration guide](integration/api-integration-guide.md) | Kết nối API, auth, request/response, lỗi và idempotency |
| Backend developer | [Developer guide](development/developer-guide.md) | Thêm use case, handler, repository, migration và codegen |
| Reporting/BA | [Reporting guide](reporting/reporting-guide.md) | Dataset, template Liquid, render PDF/HTML và giới hạn |
| DevOps/SRE | [Deployment & operations](operations/deployment-and-operations.md) | Docker, environment, health check, Redis, migration và outbox |
| Release reviewer | [Production readiness](operations/production-readiness.md) | Checklist và các hạng mục còn phải hoàn tất trước production |

## Quy tắc quản lý tài liệu

- Không đặt audit/spec/plan đã hết hiệu lực trong thư mục tài liệu vận hành.
- Nội dung mô tả code hiện tại phải trỏ tới file/class thực tế; nội dung tương lai phải ghi rõ là proposal.
- Contract tích hợp chỉ nằm trong `integration/`, không rải trong các báo cáo kiến trúc.
- Khi API thay đổi, cập nhật integration guide cùng commit với code.
