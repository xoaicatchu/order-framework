using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WolverineApp.Application.Common.Interfaces;

namespace WolverineApp.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> Query(bool tracking = false, bool ignoreFilters = false)
    {
        IQueryable<T> query = _dbSet;

        if (ignoreFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        return tracking ? query.AsTracking() : query.AsNoTracking();
    }

    public IQueryable<T> AsQueryable() => Query();

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool tracking = false,
        CancellationToken cancellationToken = default)
    {
        return await Query(tracking).FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await Query().AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await Query().CountAsync(cancellationToken)
            : await Query().CountAsync(predicate, cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}
