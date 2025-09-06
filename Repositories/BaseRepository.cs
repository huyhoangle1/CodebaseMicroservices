using Microsoft.EntityFrameworkCore;
using CourseManager.API.Data;
using System.Linq.Expressions;

namespace CourseManager.API.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly CourseManagerDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        public BaseRepository(CourseManagerDbContext context, ILogger logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        #region CRUD Operations

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id, params string[] includes)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                return await query.FirstOrDefaultAsync(GetIdPredicate(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting {EntityType} with ID {Id} and includes", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                SetAuditFields(entity, true);
                _dbSet.Add(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{EntityType} created successfully with ID {Id}", typeof(T).Name, GetEntityId(entity));
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T> UpdateAsync(int id, T entity)
        {
            try
            {
                var existingEntity = await GetByIdAsync(id);
                if (existingEntity == null)
                {
                    throw new KeyNotFoundException($"{typeof(T).Name} with ID {id} not found");
                }

                SetAuditFields(entity, false);
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{EntityType} updated successfully with ID {Id}", typeof(T).Name, id);
                return existingEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    return false;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{EntityType} deleted successfully with ID {Id}", typeof(T).Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    return false;
                }

                // Kiểm tra xem entity có property IsActive không
                var isActiveProperty = typeof(T).GetProperty("IsActive");
                if (isActiveProperty != null && isActiveProperty.CanWrite)
                {
                    isActiveProperty.SetValue(entity, false);
                    
                    // Cập nhật UpdatedAt nếu có
                    var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");
                    if (updatedAtProperty != null && updatedAtProperty.CanWrite)
                    {
                        updatedAtProperty.SetValue(entity, DateTime.UtcNow);
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("{EntityType} soft deleted successfully with ID {Id}", typeof(T).Name, id);
                    return true;
                }
                else
                {
                    // Nếu không có IsActive property, thực hiện hard delete
                    return await DeleteAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while soft deleting {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        #endregion

        #region Query Operations

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding first {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Paging

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var totalCount = await _dbSet.CountAsync();
                var items = await _dbSet
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate)
        {
            try
            {
                var query = _dbSet.Where(predicate);
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with predicate", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Bulk Operations

        public virtual async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    SetAuditFields(entity, true);
                }

                _dbSet.AddRange(entities);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{Count} {EntityType} entities created successfully", entities.Count(), typeof(T).Name);
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating range of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<bool> UpdateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    SetAuditFields(entity, false);
                }

                _dbSet.UpdateRange(entities);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("{Count} {EntityType} entities updated successfully", entities.Count(), typeof(T).Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating range of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<int> ids)
        {
            try
            {
                var entities = new List<T>();
                foreach (var id in ids)
                {
                    var entity = await GetByIdAsync(id);
                    if (entity != null)
                    {
                        entities.Add(entity);
                    }
                }

                if (entities.Any())
                {
                    _dbSet.RemoveRange(entities);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("{Count} {EntityType} entities deleted successfully", entities.Count, typeof(T).Name);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting range of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Custom Queries

        public virtual async Task<IEnumerable<T>> GetWithIncludesAsync(params string[] includes)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting {EntityType} with includes", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> FindWithIncludesAsync(Expression<Func<T, bool>> predicate, params string[] includes)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                return await query.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding {EntityType} with includes", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        protected virtual void SetAuditFields(T entity, bool isNew)
        {
            var createdAtProperty = typeof(T).GetProperty("CreatedAt");
            var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");

            if (isNew && createdAtProperty != null && createdAtProperty.CanWrite)
            {
                createdAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            if (updatedAtProperty != null && updatedAtProperty.CanWrite)
            {
                updatedAtProperty.SetValue(entity, DateTime.UtcNow);
            }
        }

        protected virtual int GetEntityId(T entity)
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                var value = idProperty.GetValue(entity);
                return value != null ? (int)value : 0;
            }
            return 0;
        }

        protected virtual Expression<Func<T, bool>> GetIdPredicate(int id)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);
            var equal = Expression.Equal(property, constant);
            
            return Expression.Lambda<Func<T, bool>>(equal, parameter);
        }

        #endregion

        #region Advanced Paging

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, TKey>> orderBy, bool ascending = true)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                var totalCount = await query.CountAsync();
                
                var orderedQuery = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                var items = await orderedQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with ordering", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, bool ascending = true)
        {
            try
            {
                var query = _dbSet.Where(predicate);
                var totalCount = await query.CountAsync();
                
                var orderedQuery = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                var items = await orderedQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with predicate and ordering", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedWithIncludesAsync(int pageNumber, int pageSize, params string[] includes)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with includes", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedWithIncludesAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, TKey>> orderBy, bool ascending = true, params string[] includes)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                var totalCount = await query.CountAsync();
                var orderedQuery = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                var items = await orderedQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with ordering and includes", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedWithIncludesAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBy, bool ascending = true, params string[] includes)
        {
            try
            {
                var query = _dbSet.Where(predicate);
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                var totalCount = await query.CountAsync();
                var orderedQuery = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
                var items = await orderedQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with full options", typeof(T).Name);
                throw;
            }
        }

        #endregion
    }
}
