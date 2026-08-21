using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Models;

namespace WolverineApp.Application.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Tự động đếm tổng số bản ghi (CountAsync) và phân trang (Skip/Take) chỉ trong 1 dòng code
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var page = pageIndex < 1 ? 1 : pageIndex;
        var size = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, page, size);
    }

    /// <summary>
    /// Tự động đếm, cắt trang và ánh xạ Entity sang DTO
    /// </summary>
    public static async Task<PagedResult<TDestination>> ToPagedResultAsync<TSource, TDestination>(
        this IQueryable<TSource> source,
        int pageIndex,
        int pageSize,
        Func<TSource, TDestination> map,
        CancellationToken cancellationToken = default)
    {
        var paged = await source.ToPagedResultAsync(pageIndex, pageSize, cancellationToken);
        return paged.Map(map);
    }
}
