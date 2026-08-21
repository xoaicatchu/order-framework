namespace WolverineApp.Application.DTOs.Roles;

public record MatrixColumnDto(string Key);

public record MatrixRowDto(
    string Module,
    string Resource,
    Dictionary<string, string> Actions
);

public record PermissionMatrixDto(
    List<MatrixColumnDto> Columns,
    List<MatrixRowDto> Rows
);

public record PermissionDto(
    Guid Id,
    string Code,
    string Module,
    string Resource,
    string Action,
    bool IsAutoDiscovered
);
