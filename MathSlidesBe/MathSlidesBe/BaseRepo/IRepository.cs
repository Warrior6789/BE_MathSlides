using System.Linq.Expressions;

namespace MathSlidesBe.BaseRepo
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(Guid id);
        Task<TEntity> AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(Guid id);
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate);
        Task<PagedResult<TEntity>> GetPagedAsync(int pageIndex, int pageSize);
    }
}
