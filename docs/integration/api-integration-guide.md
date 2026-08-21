# API integration guide

## Đối tượng đọc

Frontend web/mobile, hệ thống đối tác và QA cần gọi REST API của Wolverine Order Framework.

## Thông tin kết nối

- Base URL: do từng môi trường cung cấp, ví dụ `https://api.example.com`.
- Content type request: `application/json`.
- JSON response: camelCase.
- Swagger: chỉ bật ở Development tại `/swagger`.
- Health: `/health/live`, `/health/ready`, `/health`.

## Authentication và tenant

API không có login/token exchange local. Đối tác phải lấy JWT từ Identity Provider đã thống nhất rồi gửi:

```http
Authorization: Bearer <access-token>
```

Token phải có:

```json
{
  "sub": "user-123",
  "tenant_id": "tenant-a",
  "iss": "https://identity.example.com",
  "aud": "EnterpriseDistributedCoreClients"
}
```

Không gửi `X-Tenant-Id` hoặc `X-User-Id` để thay thế claims. Server không tin các giá trị đó.

## Headers nên gửi

```http
Authorization: Bearer <token>
Content-Type: application/json
X-Correlation-Id: checkout-20260820-0001
Idempotency-Key: create-order-<unique-key>
```

`X-Correlation-Id` là tùy chọn; server sẽ tự sinh và trả lại nếu không có. `Idempotency-Key` nên dùng cho mọi `POST`, `PUT`, `DELETE` có thể bị client retry. Key dài 1-200 ký tự và có hiệu lực theo tenant/user/method/path trong 24 giờ.

## Response envelope

Các response JSON nghiệp vụ có dạng:

```json
{
  "success": true,
  "code": "SUCCESS",
  "message": null,
  "data": {}
}
```

Response tạo mới dùng `code: "CREATED"`. Response lỗi thường có dạng:

```json
{
  "success": false,
  "code": "VALIDATION_ERROR",
  "message": "Customer name is required.",
  "errors": {
    "customerName": ["Customer name is required."]
  }
}
```

Các HTTP status cần xử lý:

| Status | Ý nghĩa |
|---:|---|
| `200` | Thành công hoặc yêu cầu cần xác nhận nghiệp vụ |
| `201` | Tạo mới thành công |
| `400` | Validation/business input không hợp lệ |
| `401` | Thiếu hoặc token không hợp lệ |
| `403` | Không có permission |
| `404` | Không tìm thấy resource trong tenant hiện tại |
| `409` | Idempotency key đang xử lý hoặc bị dùng với request hash khác |
| `410` | Local token endpoint đã bị vô hiệu hóa |
| `429` | Rate limit |
| `500` | Lỗi server; client dùng correlation ID để tra log |

## Permission matrix

| Nhóm API | Permission |
|---|---|
| Orders | `Orders:Create`, `Orders:Read`, `Orders:Update`, `Orders:Cancel` |
| Roles | `Roles:Read`, `Roles:Create`, `Roles:Update`, `Roles:Delete`, `Roles:Assign` |
| Audit logs | `AuditLogs:Read` |
| Reports | `Reports:Read`, `Reports:Export` |

`GET /api/roles/permissions/matrix` yêu cầu bearer token và quyền `Roles:Read`. `POST /api/auth/token` luôn trả `410 AUTH_PROVIDER_REQUIRED`; frontend/đối tác phải dùng Authorization Code + PKCE tại Identity Provider.

## Orders API

### Create order

```http
POST /api/orders/create
Authorization: Bearer <token>
Idempotency-Key: order-create-001
```

Input:

```json
{
  "customerName": "Nguyen Van A",
  "customerEmail": "a@example.com",
  "items": [
    {
      "productName": "Product A",
      "sku": "SKU-001",
      "quantity": 2,
      "unitPrice": 125000
    }
  ]
}
```

Validation: phải có ít nhất một item, tối đa 100 item; quantity `1..1,000,000`; unit price `>0` và `<=100,000,000`.

Output `201`:

```json
{
  "success": true,
  "code": "CREATED",
  "message": "Order created successfully.",
  "data": {
    "id": "00000000-0000-0000-0000-000000000001",
    "orderNumber": "ORD-20260820-0001",
    "customerName": "Nguyen Van A",
    "customerEmail": "a@example.com",
    "totalAmount": 250000,
    "status": "Pending",
    "createdAt": "2026-08-20T10:00:00Z",
    "items": [
      {
        "id": "00000000-0000-0000-0000-000000000002",
        "productName": "Product A",
        "sku": "SKU-001",
        "quantity": 2,
        "unitPrice": 125000,
        "total": 250000
      }
    ]
  }
}
```

### Read and list orders

```http
GET /api/orders/{id}
GET /api/orders/list?pageIndex=1&pageSize=20&status=Pending&search=ORD-
```

`status` nhận `Pending`, `Confirmed`, `Processing`, `Shipped`, `Delivered`, `Cancelled`. `pageIndex` bắt đầu từ 1; mặc định `pageSize=10`.

Output list:

```json
{
  "success": true,
  "code": "SUCCESS",
  "data": {
    "items": [],
    "pageIndex": 1,
    "pageSize": 20,
    "totalCount": 0,
    "totalPages": 0
  }
}
```

### Update status

```http
PUT /api/orders/{id}/status
Idempotency-Key: order-status-001
```

Input:

```json
{ "status": "Confirmed" }
```

Status phải đi theo transition domain hợp lệ; không coi endpoint này là setter tự do. Output là `ApiResponse<OrderDto>`.

### Cancel order

Lần gọi đầu có thể để `isConfirmed=false` để hiển thị xác nhận:

```http
DELETE /api/orders/{id}/cancel?isConfirmed=false
Idempotency-Key: order-cancel-001
```

Output `200`:

```json
{
  "success": false,
  "code": "REQUIRES_CONFIRMATION",
  "message": "Bạn có chắc chắn muốn hủy đơn hàng...",
  "data": { "id": "...", "orderNumber": "ORD-...", "totalAmount": 250000 }
}
```

Sau khi user xác nhận, gọi lại với `isConfirmed=true`. Kết quả thành công là `ApiResponse<OrderDto>`.

### Statistics

```http
GET /api/orders/statistics/summary
```

Kết quả là `ApiResponse<OrderStatisticsDto>`; UI nên đọc trực tiếp các field được trả trong `data` vì DTO có thể mở rộng theo phiên bản.

## Roles và permissions API

### Permission catalog

```http
GET /api/roles/permissions/matrix
GET /api/roles/permissions
```

Matrix dùng để dựng permission selector sau khi user đã đăng nhập. Endpoint `/permissions` cũng yêu cầu authentication.

### Create/update role

```http
POST /api/roles
Idempotency-Key: role-create-001
```

Input:

```json
{
  "name": "Sales viewer",
  "description": "Read-only sales access",
  "permissions": ["Orders:Read", "Reports:Read"]
}
```

`name` tối đa 100 ký tự. Không được gán `System:Root`; permission phải tồn tại trong catalog.

Update dùng:

```http
PUT /api/roles/{id}
Idempotency-Key: role-update-001
```

Input có cùng shape với create. System role không được sửa/xóa.

Output:

```json
{
  "success": true,
  "code": "CREATED",
  "data": {
    "id": "00000000-0000-0000-0000-000000000010",
    "name": "Sales viewer",
    "description": "Read-only sales access",
    "tenantId": "tenant-a",
    "isSystemRole": false,
    "permissions": ["Orders:Read", "Reports:Read"],
    "createdAt": "2026-08-20T10:00:00Z"
  }
}
```

### Assign roles to user

```http
POST /api/roles/assign
Idempotency-Key: role-assign-001
```

Input:

```json
{
  "userId": "user-123",
  "roleIds": ["00000000-0000-0000-0000-000000000010"]
}
```

User phải là active member của tenant hiện tại; role IDs cũng phải thuộc tenant hiện tại. Output là `ApiResponse<bool>` với `data: true`.

## Auth và audit API

```http
GET /api/auth/me
GET /api/auditlogs/list?pageIndex=1&pageSize=20
```

`/api/auth/me` trả user ID, username, tenant ID, permissions và claims đã được token gửi lên. `/api/auditlogs/list` trả `PagedResult<AuditLogDto>`.

## Reports API

Luồng tích hợp đơn giản nằm ở [Reporting guide](../reporting/reporting-guide.md). Client mới chỉ cần ba endpoint:

```http
GET  /api/reports/catalog
POST /api/reports
POST /api/reports/{code}/export
```

`catalog` trả nguồn dữ liệu và allowlist cột. `POST /api/reports` tạo báo cáo mà không cần tự đặt code hoặc viết Liquid. `export` nhận `format: pdf|html` và object `filters`, sau đó trả file binary. Các endpoint configuration/template/raw render cũ vẫn được giữ để tương thích, nhưng không phải luồng tích hợp khuyến nghị.

## Retry strategy cho đối tác

1. Retry timeout/network error với exponential backoff.
2. Giữ nguyên `Idempotency-Key` khi retry cùng một mutation.
3. Không retry với `400`, `401`, `403`, `404` nếu chưa sửa input/token/permission.
4. Với `409` do đang processing, chờ rồi gọi lại cùng key; với request hash khác, tạo key mới sau khi xác định nghiệp vụ.
5. Ghi lại `X-Correlation-Id` để đối soát với đội vận hành.
