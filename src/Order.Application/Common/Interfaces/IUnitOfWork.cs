using System.Data.Common;

namespace WolverineApp.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Lấy Generic Repository cho một Entity Type cụ thể
    /// </summary>
    IRepository<T> GetRepository<T>() where T : class;

    DbConnection GetDbConnection();

    /// <summary>
    /// Lưu toàn bộ thay đổi trong phiên làm việc vào Database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bắt đầu một Database Transaction thủ công
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit Database Transaction hiện tại
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback Database Transaction hiện tại nếu có lỗi
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Thực thi một hành động trong Transaction có Retry Policy và tự động Commit/Rollback
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> action,
        CancellationToken cancellationToken = default);
}
