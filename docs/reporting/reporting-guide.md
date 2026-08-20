# Reporting và template integration

## Đối tượng đọc

Frontend/BA cấu hình báo cáo, developer viết template Liquid và đội vận hành quản lý template theo tenant.

## Luồng chuẩn

1. Gọi `GET /api/reports/semantic-datasets` để lấy dataset và field allowlist.
2. Chọn field/filter ở UI.
3. Gọi `POST /api/reports/configurations` để lưu cấu hình.
4. Gọi `GET /api/reports/configurations/{code}/form-schema` để dựng form filter.
5. Gọi `POST /api/reports/configurations/{code}/execute` để nhận file.

## Dataset output

```json
{
  "success": true,
  "code": "SUCCESS",
  "data": [
    {
      "code": "orders",
      "name": "Orders",
      "category": "Sales",
      "description": "Order dataset",
      "fields": [
        {"key":"orderNumber","label":"Order number","type":"string","filterable":true},
        {"key":"totalAmount","label":"Total","type":"currency","filterable":true}
      ]
    }
  ]
}
```

Chỉ dùng field có trong `fields`. Dataset query luôn được scope theo tenant và giới hạn số dòng; không gửi SQL từ frontend/đối tác.

## Lưu cấu hình báo cáo

`POST /api/reports/configurations` yêu cầu permission `Reports:Export`.

Input:

```json
{
  "code": "monthly-orders",
  "name": "Monthly orders",
  "datasetCode": "orders",
  "selectedFields": ["orderNumber", "customerName", "totalAmount"],
  "filters": [
    {
      "fieldName": "createdAt",
      "label": "Created date",
      "filterType": "date_range",
      "required": true
    }
  ],
  "customTemplateContent": "<h1>{{ ReportName }}</h1>"
}
```

`customTemplateContent` có thể bỏ trống để server sinh template mặc định. Server từ chối template có JavaScript/event handler, sai cú pháp hoặc vượt giới hạn kích thước.

Output là `ApiResponse<string>` xác nhận lưu thành công.

## Execute và file output

`POST /api/reports/configurations/{code}/execute`:

```json
{
  "criteria": {
    "createdAt_from": "2026-08-01",
    "createdAt_to": "2026-08-31"
  },
  "format": 0
}
```

Giá trị enum hiện tại: `0 = Pdf`, `1 = Html`, `2 = Excel`, `3 = Csv`. Code hiện đã triển khai renderer cho PDF và HTML; Excel/CSV chưa có renderer nên không được coi là output supported.

Response thành công là binary file với `Content-Type` và `Content-Disposition`, không phải `ApiResponse` JSON.

## Liquid model và filter

Model cơ bản khi execute configuration:

```json
{
  "Data": [{"orderNumber":"ORD-001","totalAmount":125000}],
  "TotalRows": 1,
  "ReportName": "Monthly orders",
  "ExecutedAt": "31/08/2026 10:30"
}
```

Ví dụ template:

```liquid
<h1>{{ ReportName }}</h1>
<p>Total rows: {{ TotalRows }}</p>
{% for row in Data %}
  <div>{{ row.orderNumber }} - {{ row.totalAmount | format_currency: 'VND' }}</div>
{% endfor %}
```

Filter được hỗ trợ: `format_currency`, `format_date`, `to_vietnamese_words`, `qr_code`, `sum`, `group_by`. Rendering có giới hạn timeout, recursion, số bước và output size.
