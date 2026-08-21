# Tích hợp báo cáo theo luồng đơn giản

Tích hợp report chỉ cần 3 API theo thứ tự:

1. `GET /api/reports/catalog` — lấy các nguồn dữ liệu và cột được phép dùng.
2. `POST /api/reports` — lưu một báo cáo từ nguồn dữ liệu, cột và điều kiện lọc.
3. `POST /api/reports/{code}/export` — xuất PDF hoặc HTML với giá trị lọc.

Tất cả API yêu cầu bearer token và permission tương ứng. Frontend/đối tác không gửi SQL, không tự tạo dataset và không cần viết Liquid cho luồng thông thường.

## 1. Lấy danh mục nguồn dữ liệu

```http
GET /api/reports/catalog
Authorization: Bearer <access-token>
```

Response:

```json
{
  "success": true,
  "code": "SUCCESS",
  "data": {
    "dataSources": [
      {
        "id": "Sales_Orders_Dataset",
        "name": "Dữ liệu hóa đơn bán hàng",
        "category": "Tài chính",
        "description": "Đơn hàng, khách hàng và doanh thu trong tenant hiện tại.",
        "fields": [
          { "id": "OrderNumber", "name": "Mã đơn hàng", "type": "string", "canFilter": true, "options": null },
          { "id": "OrderTotal", "name": "Tổng tiền", "type": "currency", "canFilter": true, "options": null },
          { "id": "CreatedAt", "name": "Ngày tạo", "type": "date", "canFilter": true, "options": null }
        ]
      }
    ]
  }
}
```

`dataSources` là danh mục đã được đội kỹ thuật đăng ký. `fields` là allowlist; chỉ cho phép gửi `id` xuất hiện trong danh sách này.

## 2. Tạo báo cáo

```http
POST /api/reports
Authorization: Bearer <access-token>
Content-Type: application/json
Idempotency-Key: report-create-unique-key
```

Input tối thiểu:

```json
{
  "name": "Báo cáo đơn hàng tháng",
  "dataSourceId": "Sales_Orders_Dataset",
  "columns": ["OrderNumber", "CustomerName", "OrderTotal", "CreatedAt"],
  "filters": [
    {
      "field": "CreatedAt",
      "type": "date_range",
      "label": "Khoảng ngày tạo",
      "required": true
    },
    {
      "field": "Status",
      "type": "select",
      "label": "Trạng thái",
      "required": false
    }
  ]
}
```

Không cần gửi `code`; server tự sinh mã kỹ thuật và trả về:

```json
{
  "success": true,
  "code": "CREATED",
  "data": {
    "code": "report-6c4d...",
    "name": "Báo cáo đơn hàng tháng",
    "dataSourceId": "Sales_Orders_Dataset"
  }
}
```

Lưu lại `data.code` để dùng khi export. Backend tự tạo mẫu hiển thị mặc định từ các cột đã chọn. Template Liquid chỉ dành cho đội kỹ thuật hoặc tenant cần tùy biến nâng cao.

## 3. Xuất báo cáo

```http
POST /api/reports/report-6c4d.../export
Authorization: Bearer <access-token>
Content-Type: application/json
Idempotency-Key: report-export-unique-key
```

Input:

```json
{
  "format": "pdf",
  "filters": {
    "CreatedAt": {
      "from": "2026-08-01",
      "to": "2026-08-31"
    },
    "Status": "Completed"
  }
}
```

Giá trị `format` hiện hỗ trợ `pdf` và `html`. Response thành công là file binary với `Content-Type` và `Content-Disposition`, không phải `ApiResponse` JSON.

## Dataset được thiết lập ở đâu?

Dataset không được tạo bởi người dùng cuối và không nhận SQL từ frontend. Đội kỹ thuật đăng ký dataset system trong backend gồm:

- mã nguồn dữ liệu;
- tên hiển thị và mô tả;
- danh sách cột được phép;
- truy vấn nền đã có điều kiện `@TenantId`.

Sau khi đăng ký, dataset xuất hiện tự động trong `GET /api/reports/catalog`. Đây là chủ ý bảo mật: người dùng chỉ chọn dữ liệu được cấp quyền, không thể tự chèn SQL hoặc đọc tenant khác.

## API cũ và tương thích

Các endpoint `semantic-datasets`, `configurations`, `form-schema`, `configurations/{code}/execute` vẫn được giữ để không phá client cũ. Client mới nên dùng bộ API ngắn ở trên. Các endpoint template/raw render chỉ dành cho integration nâng cao.
