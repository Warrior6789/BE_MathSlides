using MathSlidesBe.Entity;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Transactions;

namespace MathSlidesBe.BaseRepo
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly MathSlidesDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(MathSlidesDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
           => await _dbSet.Where(x => !x.IsDeleted).ToListAsync();

        public async Task<TEntity?> GetByIdAsync(Guid id)
            => await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.Now;
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate) => _dbSet.Where(predicate);

        public async Task<PagedResult<TEntity>> GetPagedAsync(int pageIndex, int pageSize)
    => await _dbSet.AsQueryable().Where(x => !x.IsDeleted).ToPagedResultAsync(pageIndex, pageSize);

        public async Task<TEntity?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet.Where(x => x.Id == id && !x.IsDeleted);
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<TEntity> FindAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach(var entity in entities)
            {
                entity.UpdatedAt = DateTime.Now;
            }
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task RemoveRangeAsync(IEnumerable<TEntity> entites)
        {
            _dbSet.RemoveRange(entites);
            await Task.CompletedTask;
        }

        public async Task<T> ExcuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await action();
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
    public static class QueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageIndex,
            int pageSize) where T : class
        {
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalItems = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
    }
}
