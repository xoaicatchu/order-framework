namespace WolverineApp.Domain.Identity;

public record PermissionDefinition(
    string Code,
    string Name,
    string Description,
    string Module
);

public static class SystemPermissions
{
    // Module: Orders (Quản lý Đơn hàng)
    public const string OrdersRead = "Orders.Read";
    public const string OrdersCreate = "Orders.Create";
    public const string OrdersUpdate = "Orders.Update";
    public const string OrdersCancel = "Orders.Cancel";

    // Module: Roles & Permissions (Phân quyền & Vai trò Động)
    public const string RolesRead = "Roles.Read";
    public const string RolesCreate = "Roles.Create";
    public const string RolesUpdate = "Roles.Update";
    public const string RolesDelete = "Roles.Delete";
    public const string RolesAssign = "Roles.Assign";

    // Module: Audit Logs (Lịch sử Hoạt động)
    public const string AuditLogsRead = "AuditLogs.Read";

    // Module: System Super Admin (Root User)
    public const string SystemRoot = "System.Root";

    public static readonly List<PermissionDefinition> All =
    [
        // Orders
        new(OrdersRead, "Xem đơn hàng", "Cho phép tìm kiếm và xem chi tiết danh sách đơn hàng", "Orders"),
        new(OrdersCreate, "Tạo đơn hàng", "Cho phép lập đơn hàng mới trong hệ thống", "Orders"),
        new(OrdersUpdate, "Cập nhật đơn hàng", "Cho phép sửa đổi thông tin và trạng thái đơn hàng", "Orders"),
        new(OrdersCancel, "Hủy đơn hàng", "Cho phép thực hiện quy trình hủy đơn hàng", "Orders"),

        // Roles & IAM
        new(RolesRead, "Xem vai trò & quyền", "Cho phép xem danh sách vai trò của đơn vị", "Roles"),
        new(RolesCreate, "Tạo vai trò mới", "Cho phép admin đơn vị tự tạo vai trò động", "Roles"),
        new(RolesUpdate, "Cập nhật vai trò", "Cho phép tùy biến gán/bỏ quyền cho vai trò của đơn vị", "Roles"),
        new(RolesDelete, "Xóa vai trò", "Cho phép xóa các vai trò tùy biến không còn sử dụng", "Roles"),
        new(RolesAssign, "Gán vai trò cho người dùng", "Cho phép phân vai trò cho nhân sự trong đơn vị", "Roles"),

        // Audit Logs
        new(AuditLogsRead, "Xem nhật ký kiểm toán", "Cho phép tra cứu lịch sử thay đổi dữ liệu", "AuditLogs"),

        // Root Super Admin
        new(SystemRoot, "Quản trị toàn hệ thống", "Quyền tối cao của Root User (System Admin)", "System")
    ];
}
