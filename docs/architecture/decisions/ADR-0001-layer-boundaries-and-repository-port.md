# ADR-0001: Layer boundaries and repository port

## Decision

`Order.Domain` giữ nghiệp vụ thuần. `Order.Application` giữ use case, DTO, validator và interface/port. `Order.Infrastructure` giữ EF Core, persistence records, cache, authentication adapter, outbox worker và report renderer. `Order.WebApi` chỉ là HTTP/composition adapter.

Application không reference Infrastructure. Controllers không inject `ApplicationDbContext`; write/read access đi qua `IUnitOfWork` và `IRepository<T>`.

## Known transition

Một số query handler hiện vẫn trả `IQueryable<T>` từ repository và gọi async LINQ của EF Core. Đây là compatibility boundary để giữ projection/filter/paging hiện có trong khi tách solution; nó vẫn làm Application biết EF query provider. Không gọi trực tiếp DbContext không có nghĩa boundary đã hoàn hảo.

## Follow-up

Tách từng use case sang query port trả DTO/list/page cụ thể, sau đó bỏ `IQueryable`, `Microsoft.EntityFrameworkCore` khỏi Application và chuyển toàn bộ query composition vào Infrastructure. Architecture test hiện đã chặn Application reference trực tiếp tới Infrastructure.
