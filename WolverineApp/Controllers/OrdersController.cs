using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineApp.Application.Commands.Orders.CancelOrder;
using WolverineApp.Application.Commands.Orders.CreateOrder;
using WolverineApp.Application.Commands.Orders.UpdateOrderStatus;
using WolverineApp.Application.Common.Models;
using WolverineApp.Application.DTOs.Orders;
using WolverineApp.Application.Queries.Orders.GetOrderById;
using WolverineApp.Application.Queries.Orders.GetOrderStatistics;
using WolverineApp.Application.Queries.Orders.ListOrders;
using WolverineApp.Domain.Identity;

namespace WolverineApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[PermissionResource("Orders", "Sales")]
public class OrdersController : ControllerBase
{
    private readonly IMessageBus _bus;

    public OrdersController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// [Command] Tạo đơn hàng mới (Quyền: Orders:Create)
    /// </summary>
    [HttpPost("create")]
    [HasPermission("Orders", "Create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var response = await _bus.InvokeAsync<OrderDto>(command);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<OrderDto>.Created(response, "Tạo đơn hàng thành công."));
    }

    /// <summary>
    /// [Query] Lấy chi tiết đơn hàng theo ID (Quyền: Orders:Read)
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission("Orders", "Read")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var response = await _bus.InvokeAsync<OrderDto>(new GetOrderByIdQuery(id));
        return Ok(ApiResponse<OrderDto>.Ok(response));
    }

    /// <summary>
    /// [Query] Lấy danh sách đơn hàng có phân trang (Quyền: Orders:Read)
    /// </summary>
    [HttpGet("list")]
    [HasPermission("Orders", "Read")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var response = await _bus.InvokeAsync<PagedResult<OrderDto>>(new ListOrdersQuery(pageIndex, pageSize, status, search));
        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(response));
    }

    /// <summary>
    /// [Command] Cập nhật trạng thái đơn hàng (Quyền: Orders:Update)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [HasPermission("Orders", "Update")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var response = await _bus.InvokeAsync<OrderDto>(new UpdateOrderStatusCommand(id, request.Status));
        return Ok(ApiResponse<OrderDto>.Ok(response, "Cập nhật trạng thái đơn hàng thành công."));
    }

    /// <summary>
    /// [Command] Hủy đơn hàng (Quyền: Orders:Cancel)
    /// </summary>
    [HttpDelete("{id:guid}/cancel")]
    [HasPermission("Orders", "Cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromQuery] bool isConfirmed = false)
    {
        var response = await _bus.InvokeAsync<OrderDto>(new CancelOrderCommand(id, isConfirmed));
        return Ok(ApiResponse<OrderDto>.Ok(response, "Đơn hàng đã được hủy thành công."));
    }

    /// <summary>
    /// [Query] Lấy thống kê tổng hợp đơn hàng (Quyền: Orders:Read)
    /// </summary>
    [HttpGet("statistics/summary")]
    [HasPermission("Orders", "Read")]
    public async Task<IActionResult> GetOrderStatistics()
    {
        var response = await _bus.InvokeAsync<OrderStatisticsDto>(new GetOrderStatisticsQuery());
        return Ok(ApiResponse<OrderStatisticsDto>.Ok(response));
    }
}
