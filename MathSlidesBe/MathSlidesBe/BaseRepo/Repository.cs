using MathSlidesBe.Entity;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
