using Microsoft.EntityFrameworkCore;
using CourseManager.API.Data;

namespace CourseManager.API.Services
{
    public abstract class BaseService<T> where T : class
    {
        protected readonly CourseManagerDbContext _context;
        protected readonly ILogger _logger;
        protected readonly DbSet<T> _dbSet;

        protected BaseService(CourseManagerDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Lấy tất cả entities
        /// </summary>
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

        /// <summary>
        /// Lấy entity theo ID
        /// </summary>
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

        /// <summary>
        /// Lấy entity theo ID với include related entities
        /// </summary>
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

        /// <summary>
        /// Tạo entity mới
        /// </summary>
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

        /// <summary>
        /// Cập nhật entity
        /// </summary>
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

        /// <summary>
        /// Xóa entity (hard delete)
        /// </summary>
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

        /// <summary>
        /// Xóa entity (soft delete) - chỉ hoạt động với entities có IsActive property
        /// </summary>
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

        /// <summary>
        /// Kiểm tra entity có tồn tại không
        /// </summary>
        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _dbSet.AnyAsync(GetIdPredicate(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// Đếm số lượng entities
        /// </summary>
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

        /// <summary>
        /// Lấy entities với phân trang
        /// </summary>
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

        /// <summary>
        /// Lưu thay đổi vào database
        /// </summary>
        protected virtual async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving changes for {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Thiết lập các trường audit (CreatedAt, UpdatedAt)
        /// </summary>
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

        /// <summary>
        /// Lấy ID của entity
        /// </summary>
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

        /// <summary>
        /// Tạo predicate để tìm entity theo ID
        /// </summary>
        protected virtual System.Linq.Expressions.Expression<Func<T, bool>> GetIdPredicate(int id)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var property = System.Linq.Expressions.Expression.Property(parameter, "Id");
            var constant = System.Linq.Expressions.Expression.Constant(id);
            var equal = System.Linq.Expressions.Expression.Equal(property, constant);
            
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equal, parameter);
        }
    }
}
