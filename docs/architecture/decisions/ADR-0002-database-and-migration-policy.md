# ADR-0002: PostgreSQL production, SQLite development

## Decision

Production dùng PostgreSQL qua Npgsql. SQLite chỉ dành cho Development/Test để khởi động nhanh và test local. `Database:Provider` chọn provider; production từ chối SQLite.

Schema migration là deployment concern. Production mặc định `Database:AutoMigrate=false` và `Database:RequireExternalMigration=true`; deployment job phải chạy migration trước rollout. Demo seed chỉ chạy khi Development/Test hoặc có cờ provisioning explicit.

## Consequence

Mọi dataset SQL và migration phải được kiểm tra trên PostgreSQL thật. Không dùng `EnsureCreated` cho production. Connection string và secret lấy từ secret manager/environment, không đặt trong source.
