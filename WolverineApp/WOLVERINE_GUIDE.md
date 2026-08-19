# Wolverine Configuration & Implementation Guide

## Wolverine Integration Highlights

### 1. Wolverine Packages
```xml
<PackageReference Include="WolverineFx" Version="6.29.0" />
<PackageReference Include="WolverineFx.RuntimeCompilation" Version="6.29.0" />
```

### 2. Wolverine Startup Configuration

**Program.cs:**
```csharp
builder.Host.UseWolverine(opts =>
{
    // Tất cả messages được publish (không chỉ request-response)
    opts.PublishAllMessages();
});
```

### 3. Command Definitions

Wolverine sử dụng CQRS (Command Query Responsibility Segregation) pattern:

**Commands/CreateOrderCommand.cs:**
```csharp
public class CreateOrderCommand
{
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public List<OrderItemRequest> Items { get; set; }
}

public class UpdateOrderStatusCommand
{
    public Guid OrderId { get; set; }
    public string Status { get; set; }
}

public class CancelOrderCommand
{
    public Guid OrderId { get; set; }
}
```

### 4. Handler Implementation

Wolverine handlers tuân theo naming convention: `[CommandName]Handler`

**Handlers/CreateOrderHandler.cs:**
```csharp
public class CreateOrderCommandHandler
{
    private readonly ApplicationDbContext _dbContext;

    public CreateOrderCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Handler method phải có tên là Handle hoặc Handle<T>
    public async Task<Guid> Handle(CreateOrderCommand command)
    {
        // Logic xử lý
        // Trả về Guid (Order ID)
    }
}

public class UpdateOrderStatusCommandHandler
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateOrderStatusCommandHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateOrderStatusCommand command)
    {
        // Logic xử lý, không trả về gì
    }
}
```

### 5. Mediator Pattern Implementation

Wolverine hoạt động như một **Service Mediator** giữa request và handlers:

**Flow:**
```
Request (Controller)
    ↓
Wolverine MessageBus.InvokeAsync()
    ↓
Tìm Handler phù hợp (Convention-based)
    ↓
Inject dependencies & Execute Handler
    ↓
Return Result (nếu có)
```

### 6. Dependency Injection

Wolverine tự động khám phá và đăng ký handlers thông qua **Reflection**:

```csharp
// Wolverine tự động tìm tất cả public classes kết thúc bằng "Handler"
// và đăng ký chúng vào service container

// Ví dụ:
// - CreateOrderCommandHandler → Xử lý CreateOrderCommand
// - UpdateOrderStatusCommandHandler → Xử lý UpdateOrderStatusCommand
// - CancelOrderCommandHandler → Xử lý CancelOrderCommand
```

### 7. Handler Discovery & Configuration

Wolverine tìm handlers dựa trên:

1. **Naming Convention** - Classes kết thúc bằng "Handler"
2. **Method Naming** - Phương thức có tên `Handle` hoặc `Handle<T>`
3. **Signature** - Constructor injection & Method parameters

**Cảnh báo (nếu gặp):**
```
warn: Wolverine.Configuration.HandlerDiscovery
      Wolverine found no handlers.
```

**Giải pháp:**
- Đảm bảo classes là public
- Đảm bảo tên class kết thúc bằng "Handler"
- Đảm bảo có method `Handle`
- Rebuild project

### 8. Service Location Issue (Resolved)

**Vấn đề:**
```
Found service locations while generating code for Message Handler...
ServiceLocationPolicy.NotAllowed is in effect
```

**Nguyên Nhân:** Wolverine không thích DBContext được inject thông qua lambda factory

**Giải Pháp Hiện Tại:** Thực hiện logic trực tiếp trong Controller (workaround)

**Giải Pháp Tốt Hơn (Future):**
- Sử dụng IServiceProvider.CreateScope() trong handler
- Hay configure ServiceLocationPolicy = ServiceLoadingPolicy.Allow
- Hay sử dụng DbContext factory pattern

### 9. IMessageBus - Mediator Interface

```csharp
// Inject IMessageBus vào controller
public class OrdersController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public OrdersController(IMessageBus messageBus, ApplicationDbContext dbContext)
    {
        _messageBus = messageBus;
    }

    // Sử dụng để gửi commands
    var orderId = await _messageBus.InvokeAsync<Guid>(command);
}
```

## Command Query Separation

### Commands (Write Operations)
- CreateOrderCommand
- UpdateOrderStatusCommand
- CancelOrderCommand

**Tính chất:**
- Thay đổi state
- Handler trả về void hoặc generic type
- Xử lý sequentially

### Queries (Read Operations)
- Bằng cách fetch trực tiếp từ database
- Không qua Wolverine bus (tối ưu performance)

## Advanced Features (Not Implemented Yet)

### 1. Async Message Handling
```csharp
// Messages được xử lý bất đồng bộ
await _messageBus.SendAsync(command);  // Fire & forget
```

### 2. Message Routing
```csharp
opts.PublishAllMessages();  // Publish tất cả messages
```

### 3. Saga Pattern
```csharp
// Long-running transactions
```

### 4. Event Sourcing
```csharp
// Lưu tất cả events thay vì just state
```

## Performance Considerations

1. **Handler Caching** - Wolverine cache handlers tự động
2. **Dynamic Compilation** - RuntimeCompilation package cho dynamic code gen
3. **Message Routing** - Wolverine tối ưu routing
4. **Scoped Dependencies** - DbContext được tạo per-request

## Troubleshooting

### Problem: Handlers không được tìm thấy
**Solution:**
```
1. Đảm bảo class kết thúc bằng "Handler"
2. Đảm bảo class là public
3. Đảm bảo có method Handle(Command)
4. Clean & Rebuild: dotnet clean && dotnet build
```

### Problem: Service Location Error
**Solution:**
```
1. Sử dụng IServiceProvider nếu cần many dependencies
2. Hay sử dụng factory pattern
3. Hay inject Func<Type> thay vì Type trực tiếp
```

### Problem: Command không được xử lý
**Solution:**
```
1. Kiểm tra command & handler được đăng ký
2. Kiểm tra logger output
3. Ensure IMessageBus được inject đúng
```

## Best Practices

1. **One Handler per Command** - Chỉ một handler xử lý mỗi command type
2. **Handlers Should Be Stateless** - Handlers không nên giữ state
3. **Use DTOs** - Sử dụng DTOs để transfer data
4. **Validate in Handler** - Validation logic ở handler
5. **Async/Await** - Handlers nên async nếu có I/O operations
6. **Logging** - Log command execution cho debugging
7. **Error Handling** - Throw meaningful exceptions

## Example: Tạo Handler Mới

### 1. Define Command
```csharp
public class MyCommand
{
    public string Data { get; set; }
}
```

### 2. Create Handler
```csharp
public class MyCommandHandler
{
    private readonly IMyService _service;

    public MyCommandHandler(IMyService service)
    {
        _service = service;
    }

    public async Task<Result> Handle(MyCommand command)
    {
        // Logic here
        return result;
    }
}
```

### 3. Use in Controller
```csharp
var result = await _messageBus.InvokeAsync<Result>(new MyCommand { Data = "..." });
```

## Wolverine vs MediatR

| Feature | Wolverine | MediatR |
|---------|-----------|---------|
| Message Bus | ✅ Built-in | ❌ Not built-in |
| Performance | 🔥 Faster (compiled) | Good |
| Learning Curve | Moderate | Easy |
| Flexibility | High | High |
| Async Support | ✅ Native | ✅ Native |
| Saga Pattern | ✅ Yes | Library needed |

## References

- Wolverine Official Docs: https://wolverinefx.net
- Handler Discovery: https://wolverinefx.net/guide/handlers/discovery.html
- Code Generation: https://wolverinefx.net/guide/codegen.html
- Message Bus: https://wolverinefx.net/guide/messaging.html
