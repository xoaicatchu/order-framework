namespace WolverineApp.Domain.Common;

public static class Permissions
{
    public static class Orders
    {
        public const string Read = "Orders.Read";
        public const string Create = "Orders.Create";
        public const string Update = "Orders.Update";
        public const string Cancel = "Orders.Cancel";
    }

    public static class AuditLogs
    {
        public const string Read = "AuditLogs.Read";
    }
}
