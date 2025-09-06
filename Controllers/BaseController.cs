using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.API.Repositories;
using System.Linq.Expressions;

namespace CourseManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Mặc định yêu cầu authentication
    public abstract class BaseController : ControllerBase
    {
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected BaseController(IMapper mapper, ILogger logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        protected ActionResult<T> HandleResult<T>(T result)
        {
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        protected ActionResult<IEnumerable<T>> HandleResult<T>(IEnumerable<T> result)
        {
            if (result == null || !result.Any())
            {
                return NotFound();
            }

            return Ok(result);
        }

        protected ActionResult<T> HandleCreatedResult<T>(T result, string actionName, object routeValues)
        {
            return CreatedAtAction(actionName, routeValues, result);
        }

        protected ActionResult HandleNoContentResult()
        {
            return NoContent();
        }

        protected ActionResult HandleBadRequestResult(string message)
        {
            return BadRequest(new { message });
        }

        protected ActionResult HandleUnauthorizedResult(string message = "Unauthorized")
        {
            return Unauthorized(new { message });
        }

        protected ActionResult HandleForbiddenResult(string message = "Forbidden")
        {
            return Forbid();
        }

        protected ActionResult HandleInternalServerErrorResult(string message = "An error occurred while processing your request")
        {
            return StatusCode(500, new { message });
        }

        #region CRUD Helper Methods

        /// <summary>
        /// Lấy tất cả entities
        /// </summary>
        protected async Task<ActionResult<IEnumerable<TDto>>> GetAllAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository) 
            where TEntity : class
        {
            try
            {
                var entities = await repository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<TDto>>(entities);
                return HandleResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy tất cả entities với quyền truy cập
        /// </summary>
        protected async Task<ActionResult<IEnumerable<TDto>>> GetAllAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, string requiredPermission) 
            where TEntity : class
        {
            try
            {
                if (!HasPermission(requiredPermission))
                {
                    return HandleForbiddenResult($"You don't have permission to access {typeof(TEntity).Name}");
                }

                var entities = await repository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<TDto>>(entities);
                return HandleResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entity theo ID
        /// </summary>
        protected async Task<ActionResult<TDto>> GetByIdAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, int id) 
            where TEntity : class
        {
            try
            {
                var entity = await repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return HandleBadRequestResult($"{typeof(TEntity).Name} with ID {id} not found");
                }

                var dto = _mapper.Map<TDto>(entity);
                return HandleResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entity theo ID với includes
        /// </summary>
        protected async Task<ActionResult<TDto>> GetByIdAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, int id, params string[] includes) 
            where TEntity : class
        {
            try
            {
                var entity = await repository.GetByIdAsync(id, includes);
                if (entity == null)
                {
                    return HandleBadRequestResult($"{typeof(TEntity).Name} with ID {id} not found");
                }

                var dto = _mapper.Map<TDto>(entity);
                return HandleResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo entity mới
        /// </summary>
        protected async Task<ActionResult<TDto>> CreateAsync<TEntity, TDto, TCreateRequest>(
            IBaseRepository<TEntity> repository, TCreateRequest request) 
            where TEntity : class
        {
            try
            {
                var entity = _mapper.Map<TEntity>(request);
                var createdEntity = await repository.CreateAsync(entity);
                var dto = _mapper.Map<TDto>(createdEntity);

                return HandleCreatedResult(dto, nameof(GetByIdAsync), new { id = GetEntityId(createdEntity) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo entity mới với quyền truy cập
        /// </summary>
        protected async Task<ActionResult<TDto>> CreateAsync<TEntity, TDto, TCreateRequest>(
            IBaseRepository<TEntity> repository, TCreateRequest request, string requiredPermission) 
            where TEntity : class
        {
            try
            {
                if (!HasPermission(requiredPermission))
                {
                    return HandleForbiddenResult($"You don't have permission to create {typeof(TEntity).Name}");
                }

                var entity = _mapper.Map<TEntity>(request);
                var createdEntity = await repository.CreateAsync(entity);
                var dto = _mapper.Map<TDto>(createdEntity);

                return HandleCreatedResult(dto, nameof(GetByIdAsync), new { id = GetEntityId(createdEntity) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo nhiều entities
        /// </summary>
        protected async Task<ActionResult<IEnumerable<TDto>>> CreateRangeAsync<TEntity, TDto, TCreateRequest>(
            IBaseRepository<TEntity> repository, IEnumerable<TCreateRequest> requests) 
            where TEntity : class
        {
            try
            {
                var entities = _mapper.Map<IEnumerable<TEntity>>(requests);
                var createdEntities = await repository.CreateRangeAsync(entities);
                var dtos = _mapper.Map<IEnumerable<TDto>>(createdEntities);

                return HandleResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating range of {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo nhiều entities với quyền truy cập
        /// </summary>
        protected async Task<ActionResult<IEnumerable<TDto>>> CreateRangeAsync<TEntity, TDto, TCreateRequest>(
            IBaseRepository<TEntity> repository, IEnumerable<TCreateRequest> requests, string requiredPermission) 
            where TEntity : class
        {
            try
            {
                if (!HasPermission(requiredPermission))
                {
                    return HandleForbiddenResult($"You don't have permission to create {typeof(TEntity).Name}");
                }

                var entities = _mapper.Map<IEnumerable<TEntity>>(requests);
                var createdEntities = await repository.CreateRangeAsync(entities);
                var dtos = _mapper.Map<IEnumerable<TDto>>(createdEntities);

                return HandleResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating range of {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật entity
        /// </summary>
        protected async Task<ActionResult<TDto>> UpdateAsync<TEntity, TDto, TUpdateRequest>(
            IBaseRepository<TEntity> repository, int id, TUpdateRequest request) 
            where TEntity : class
        {
            try
            {
                var entity = _mapper.Map<TEntity>(request);
                var updatedEntity = await repository.UpdateAsync(id, entity);
                var dto = _mapper.Map<TDto>(updatedEntity);

                return HandleResult(dto);
            }
            catch (KeyNotFoundException)
            {
                return HandleBadRequestResult($"{typeof(TEntity).Name} with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xóa entity (hard delete)
        /// </summary>
        protected async Task<ActionResult> DeleteAsync<TEntity>(
            IBaseRepository<TEntity> repository, int id) 
            where TEntity : class
        {
            try
            {
                var result = await repository.DeleteAsync(id);
                if (!result)
                {
                    return HandleBadRequestResult($"{typeof(TEntity).Name} with ID {id} not found");
                }

                return HandleNoContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xóa entity (soft delete)
        /// </summary>
        protected async Task<ActionResult> SoftDeleteAsync<TEntity>(
            IBaseRepository<TEntity> repository, int id) 
            where TEntity : class
        {
            try
            {
                var result = await repository.SoftDeleteAsync(id);
                if (!result)
                {
                    return HandleBadRequestResult($"{typeof(TEntity).Name} with ID {id} not found");
                }

                return HandleNoContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while soft deleting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                return HandleInternalServerErrorResult();
            }
        }

        #endregion

        #region Query Helper Methods

        /// <summary>
        /// Tìm kiếm entities theo predicate
        /// </summary>
        protected async Task<ActionResult<IEnumerable<TDto>>> FindAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, Expression<Func<TEntity, bool>> predicate) 
            where TEntity : class
        {
            try
            {
                var entities = await repository.FindAsync(predicate);
                var dtos = _mapper.Map<IEnumerable<TDto>>(entities);
                return HandleResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tìm kiếm entity đầu tiên theo predicate
        /// </summary>
        protected async Task<ActionResult<TDto>> FirstOrDefaultAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, Expression<Func<TEntity, bool>> predicate) 
            where TEntity : class
        {
            try
            {
                var entity = await repository.FirstOrDefaultAsync(predicate);
                if (entity == null)
                {
                    return HandleBadRequestResult($"{typeof(TEntity).Name} not found");
                }

                var dto = _mapper.Map<TDto>(entity);
                return HandleResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding first {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Kiểm tra entity có tồn tại không
        /// </summary>
        protected async Task<ActionResult<bool>> ExistsAsync<TEntity>(
            IBaseRepository<TEntity> repository, Expression<Func<TEntity, bool>> predicate) 
            where TEntity : class
        {
            try
            {
                var exists = await repository.ExistsAsync(predicate);
                return HandleResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Đếm số lượng entities
        /// </summary>
        protected async Task<ActionResult<int>> CountAsync<TEntity>(
            IBaseRepository<TEntity> repository) 
            where TEntity : class
        {
            try
            {
                var count = await repository.CountAsync();
                return HandleResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Đếm số lượng entities theo predicate
        /// </summary>
        protected async Task<ActionResult<int>> CountAsync<TEntity>(
            IBaseRepository<TEntity> repository, Expression<Func<TEntity, bool>> predicate) 
            where TEntity : class
        {
            try
            {
                var count = await repository.CountAsync(predicate);
                return HandleResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while counting {EntityType} with predicate", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        #endregion

        #region Paging Helper Methods

        /// <summary>
        /// Lấy entities với phân trang
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize) 
            where TEntity : class
        {
            try
            {
                var (items, totalCount) = await repository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang và predicate
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, Expression<Func<TEntity, bool>> predicate) 
            where TEntity : class
        {
            try
            {
                var (items, totalCount) = await repository.GetPagedAsync(pageNumber, pageSize, predicate);
                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with predicate", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang và sắp xếp
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedAsync<TEntity, TDto, TKey>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, 
            Expression<Func<TEntity, TKey>> orderBy, bool ascending = true) 
            where TEntity : class
        {
            try
            {
                var (items, totalCount) = await repository.GetPagedAsync(pageNumber, pageSize, orderBy, ascending);
                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with ordering", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang, predicate và sắp xếp
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedAsync<TEntity, TDto, TKey>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, 
            Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> orderBy, bool ascending = true) 
            where TEntity : class
        {
            try
            {
                var (items, totalCount) = await repository.GetPagedAsync(pageNumber, pageSize, predicate, orderBy, ascending);
                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with predicate and ordering", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang và includes
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedWithIncludesAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, params string[] includes) 
            where TEntity : class
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

                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with includes", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang, predicate và includes
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedWithIncludesAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, 
            Expression<Func<TEntity, bool>> predicate, params string[] includes) 
            where TEntity : class
        {
            try
            {
                var query = _dbSet.Where(predicate);
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with predicate and includes", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang, sắp xếp và includes
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedWithIncludesAsync<TEntity, TDto, TKey>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, 
            Expression<Func<TEntity, TKey>> orderBy, bool ascending = true, params string[] includes) 
            where TEntity : class
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

                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with ordering and includes", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang đầy đủ (predicate, sắp xếp, includes)
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> GetPagedWithIncludesAsync<TEntity, TDto, TKey>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, 
            Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> orderBy, 
            bool ascending = true, params string[] includes) 
            where TEntity : class
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

                var dtos = _mapper.Map<IEnumerable<TDto>>(items);
                
                var result = new PagedResult<TDto>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged {EntityType} with full options", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang và tìm kiếm
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> SearchPagedAsync<TEntity, TDto>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, string searchTerm, 
            params Expression<Func<TEntity, bool>>[] searchFields) 
            where TEntity : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetPagedAsync<TEntity, TDto>(repository, pageNumber, pageSize);
                }

                var predicate = BuildSearchPredicate(searchTerm, searchFields);
                return await GetPagedAsync<TEntity, TDto>(repository, pageNumber, pageSize, predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching paged {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy entities với phân trang và tìm kiếm có sắp xếp
        /// </summary>
        protected async Task<ActionResult<PagedResult<TDto>>> SearchPagedAsync<TEntity, TDto, TKey>(
            IBaseRepository<TEntity> repository, int pageNumber, int pageSize, string searchTerm, 
            Expression<Func<TEntity, TKey>> orderBy, bool ascending = true, 
            params Expression<Func<TEntity, bool>>[] searchFields) 
            where TEntity : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetPagedAsync<TEntity, TDto, TKey>(repository, pageNumber, pageSize, orderBy, ascending);
                }

                var predicate = BuildSearchPredicate(searchTerm, searchFields);
                return await GetPagedAsync<TEntity, TDto, TKey>(repository, pageNumber, pageSize, predicate, orderBy, ascending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching paged {EntityType} with ordering", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        #endregion

        #region Bulk Operations Helper Methods

        /// <summary>
        /// Tạo nhiều entities
        /// </summary>
        protected async Task<ActionResult<IEnumerable<TDto>>> CreateRangeAsync<TEntity, TDto, TCreateRequest>(
            IBaseRepository<TEntity> repository, IEnumerable<TCreateRequest> requests) 
            where TEntity : class
        {
            try
            {
                var entities = _mapper.Map<IEnumerable<TEntity>>(requests);
                var createdEntities = await repository.CreateRangeAsync(entities);
                var dtos = _mapper.Map<IEnumerable<TDto>>(createdEntities);

                return HandleResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating range of {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật nhiều entities
        /// </summary>
        protected async Task<ActionResult<bool>> UpdateRangeAsync<TEntity, TDto, TUpdateRequest>(
            IBaseRepository<TEntity> repository, IEnumerable<TUpdateRequest> requests) 
            where TEntity : class
        {
            try
            {
                var entities = _mapper.Map<IEnumerable<TEntity>>(requests);
                var result = await repository.UpdateRangeAsync(entities);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating range of {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xóa nhiều entities
        /// </summary>
        protected async Task<ActionResult<bool>> DeleteRangeAsync<TEntity>(
            IBaseRepository<TEntity> repository, IEnumerable<int> ids) 
            where TEntity : class
        {
            try
            {
                var result = await repository.DeleteRangeAsync(ids);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting range of {EntityType}", typeof(TEntity).Name);
                return HandleInternalServerErrorResult();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Lấy ID của entity
        /// </summary>
        protected int GetEntityId<TEntity>(TEntity entity)
        {
            var idProperty = typeof(TEntity).GetProperty("Id");
            if (idProperty != null)
            {
                var value = idProperty.GetValue(entity);
                return value != null ? (int)value : 0;
            }
            return 0;
        }

        /// <summary>
        /// Kiểm tra quyền admin
        /// </summary>
        protected bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        /// <summary>
        /// Lấy User ID từ token
        /// </summary>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return 0;
        }

        /// <summary>
        /// Kiểm tra quyền truy cập
        /// </summary>
        protected bool HasPermission(string permission)
        {
            return User.HasClaim("permission", permission);
        }

        /// <summary>
        /// Xây dựng predicate cho tìm kiếm
        /// </summary>
        protected Expression<Func<TEntity, bool>> BuildSearchPredicate<TEntity>(
            string searchTerm, params Expression<Func<TEntity, bool>>[] searchFields) 
            where TEntity : class
        {
            if (searchFields == null || !searchFields.Any())
            {
                return x => true; // Trả về predicate luôn true nếu không có search fields
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            Expression? combinedExpression = null;

            foreach (var searchField in searchFields)
            {
                var body = searchField.Body;
                var fieldExpression = Expression.Invoke(searchField, parameter);
                
                if (combinedExpression == null)
                {
                    combinedExpression = fieldExpression;
                }
                else
                {
                    combinedExpression = Expression.OrElse(combinedExpression, fieldExpression);
                }
            }

            return Expression.Lambda<Func<TEntity, bool>>(combinedExpression ?? Expression.Constant(true), parameter);
        }

        #endregion
    }

    /// <summary>
    /// Kết quả phân trang
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
