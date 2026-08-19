using System.Linq.Expressions;

namespace WolverineApp.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Tạo truy vấn LINQ linh hoạt hỗ trợ cấu hình Tracking và Bỏ qua Query Filter (cho Admin/CMS)
    /// </summary>
    IQueryable<T> Query(bool tracking = false, bool ignoreFilters = false);

    /// <summary>
    /// Shorthand lấy IQueryable nhanh
    /// </summary>
    IQueryable<T> AsQueryable();

    /// <summary>
    /// Tìm entity theo khóa chính (Primary Key)
    /// </summary>
    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy bản ghi đầu tiên thỏa mãn điều kiện
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool tracking = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem có tồn tại bản ghi nào thỏa mãn điều kiện hay không
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm số lượng bản ghi thỏa mãn điều kiện
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một entity
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới danh sách entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu cập nhật entity
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Đánh dấu cập nhật danh sách entities
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Xóa entity (Xóa mềm hoặc Xóa cứng tùy cấu hình DbContext)
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Xóa danh sách entities
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);
}
