# Developer guide

## Setup

```bash
cd D:\Github\wolverine
dotnet restore
dotnet build
dotnet run
```

Development cần `Jwt__SecretKey` hoặc `Jwt__Authority`. Không dùng endpoint local token để tạo token giả.

## Thêm use case

1. Tạo command/query trong `Application/Commands` hoặc `Application/Queries`.
2. Tạo handler nhận `IUnitOfWork`, `IRepository<T>` hoặc application port cần thiết.
3. Thêm validator nếu input có invariant.
4. Controller chỉ gọi `IMessageBus`; không inject `ApplicationDbContext`.
5. Nếu là HTTP endpoint cần thêm permission attribute phù hợp.
6. Chạy codegen và build:

```bash
dotnet run -- codegen write
dotnet build -c Release --no-restore
```

Generated files nằm tại `src/Order.WebApi/Generated/WolverineHandlers` và phải cập nhật cùng thay đổi handler.

## Persistence rule

Application code dùng:

```csharp
var repository = _unitOfWork.GetRepository<Order>();
var order = await repository.Query(tracking: true)
    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

await _unitOfWork.SaveChangesAsync(cancellationToken);
```

Không thêm `ApplicationDbContext` vào handler/controller. Nếu infrastructure cần raw SQL, đi qua `IUnitOfWork.GetDbConnection()`.

## Migrations

```bash
dotnet ef migrations add DescribeTheChange
dotnet ef database update
```

Trước khi commit:

```bash
dotnet ef migrations has-pending-model-changes --project src/Order.Infrastructure/Order.Infrastructure.csproj --startup-project src/Order.WebApi/Order.WebApi.csproj
```

Production chạy migrations qua deployment pipeline, không dùng `EnsureCreated`.

## API response và lỗi

HTTP business response dùng `ApiResponse<T>`:

```json
{
  "success": true,
  "code": "SUCCESS",
  "message": null,
  "data": {}
}
```

Validation/business errors dùng `VALIDATION_ERROR`, `BAD_REQUEST`, `NOT_FOUND`, `FORBIDDEN` hoặc `INTERNAL_ERROR`. Không trả stack trace cho client.

## Code review checklist

- Không truy cập `DbContext` ngoài Persistence/Composition Root.
- Không lấy tenant/user từ header tự do hoặc fallback mặc định.
- Có tenant scope cho mọi read/write.
- Mutation có idempotency khi client có thể retry.
- Domain transition được kiểm tra trong domain/handler.
- Generated Wolverine code đã được regenerate.
- Migration, docs integration và changelog được cập nhật nếu contract thay đổi.
