using System.Linq.Expressions;

namespace CourseManager.API.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        // CRUD Operations
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(int id, params string[] includes);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(int id, T entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        
        // Query Operations
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        
        // Paging
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate);
        
        // Bulk Operations
        Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities);
        Task<bool> UpdateRangeAsync(IEnumerable<T> entities);
        Task<bool> DeleteRangeAsync(IEnumerable<int> ids);
        
        // Custom Queries
        Task<IEnumerable<T>> GetWithIncludesAsync(params string[] includes);
        Task<IEnumerable<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params string[] includes);
        
        // Advanced Paging
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, TKey>> orderBy, bool ascending = true);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, bool ascending = true);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedWithIncludesAsync(int pageNumber, int pageSize, params string[] includes);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedWithIncludesAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, TKey>> orderBy, bool ascending = true, params string[] includes);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedWithIncludesAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, bool ascending = true, params string[] includes);
    }
}
