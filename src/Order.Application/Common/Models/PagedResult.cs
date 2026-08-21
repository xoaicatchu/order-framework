namespace WolverineApp.Application.Common.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex < 1 ? 1 : pageIndex;
        PageSize = pageSize < 1 ? 10 : pageSize;
    }

    /// <summary>
    /// Chuyển đổi dữ liệu Entity sang DTO mà vẫn giữ nguyên thông tin phân trang
    /// </summary>
    public PagedResult<TDestination> Map<TDestination>(Func<T, TDestination> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedResult<TDestination>(mappedItems, TotalCount, PageIndex, PageSize);
    }
}
