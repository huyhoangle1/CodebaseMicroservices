using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;
using CourseManager.Shared.Repositories;
using CourseManager.Shared.Controllers;

namespace CourseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper, ILogger<CategoriesController> logger) 
            : base(mapper, logger)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Lấy tất cả danh mục
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            return await GetAllAsync<Category, CategoryDto>(_categoryRepository);
        }

        /// <summary>
        /// Lấy danh mục theo ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
        {
            return await GetByIdAsync<Category, CategoryDto>(_categoryRepository, id);
        }

        /// <summary>
        /// Tạo danh mục mới (chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            return await CreateAsync<Category, CategoryDto, CreateCategoryRequest>(_categoryRepository, request, "categories.create");
        }

        /// <summary>
        /// Cập nhật danh mục (chỉ Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            return await UpdateAsync<Category, CategoryDto, UpdateCategoryRequest>(_categoryRepository, id, request);
        }

        /// <summary>
        /// Xóa danh mục (chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            return await SoftDeleteAsync<Category>(_categoryRepository, id);
        }

        /// <summary>
        /// Lấy danh mục hoạt động
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetActiveCategoriesAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
                return HandleResult(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active categories");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy danh mục với phân trang
        /// </summary>
        [HttpGet("paged")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CategoryDto>>> GetPagedCategories(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                Expression<Func<Category, bool>>? predicate = null;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    predicate = c => c.Name.Contains(searchTerm) || 
                                   (c.Description != null && c.Description.Contains(searchTerm));
                }

                if (predicate != null)
                {
                    return await GetPagedAsync<Category, CategoryDto>(_categoryRepository, pageNumber, pageSize, predicate);
                }
                else
                {
                    return await GetPagedAsync<Category, CategoryDto>(_categoryRepository, pageNumber, pageSize);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged categories");
                return HandleInternalServerErrorResult();
            }
        }
    }
}
