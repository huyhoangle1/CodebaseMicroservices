using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;
using CourseManager.Shared.Repositories;
using CourseManager.Shared.Controllers;
using System.Linq.Expressions;

namespace CourseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : BaseController
    {
        private readonly ICourseRepository _courseRepository;

        public CoursesController(ICourseRepository courseRepository, IMapper mapper, ILogger<CoursesController> logger) 
            : base(mapper, logger)
        {
            _courseRepository = courseRepository;
        }

        /// <summary>
        /// Lấy tất cả khóa học
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
        {
            return await GetAllAsync<Course, CourseDto>(_courseRepository);
        }

        /// <summary>
        /// Lấy khóa học theo ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CourseDto>> GetCourseById(int id)
        {
            return await GetByIdAsync<Course, CourseDto>(_courseRepository, id);
        }

        /// <summary>
        /// Tạo khóa học mới (chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CreateCourseRequest request)
        {
            return await CreateAsync<Course, CourseDto, CreateCourseRequest>(_courseRepository, request, "courses.create");
        }

        /// <summary>
        /// Cập nhật khóa học (chỉ Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
        {
            return await UpdateAsync<Course, CourseDto, UpdateCourseRequest>(_courseRepository, id, request);
        }

        /// <summary>
        /// Xóa khóa học (chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            return await SoftDeleteAsync<Course>(_courseRepository, id);
        }

        /// <summary>
        /// Lấy khóa học theo danh mục
        /// </summary>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesByCategory(string category)
        {
            try
            {
                var courses = await _courseRepository.GetCoursesByCategoryAsync(category);
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return HandleResult(courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by category {Category}", category);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tìm kiếm khóa học
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> SearchCourses([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return HandleBadRequestResult("Search query cannot be empty");
                }

                var courses = await _courseRepository.SearchCoursesAsync(q);
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return HandleResult(courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching courses with query {Query}", q);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy khóa học nổi bật
        /// </summary>
        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetFeaturedCourses([FromQuery] int count = 10)
        {
            try
            {
                var courses = await _courseRepository.GetFeaturedCoursesAsync(count);
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return HandleResult(courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured courses");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy khóa học mới nhất
        /// </summary>
        [HttpGet("newest")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetNewestCourses([FromQuery] int count = 10)
        {
            try
            {
                var courses = await _courseRepository.GetNewestCoursesAsync(count);
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return HandleResult(courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting newest courses");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy khóa học phổ biến
        /// </summary>
        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetPopularCourses([FromQuery] int count = 10)
        {
            try
            {
                var courses = await _courseRepository.GetPopularCoursesAsync(count);
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return HandleResult(courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting popular courses");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy khóa học với phân trang
        /// </summary>
        [HttpGet("paged")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CourseDto>>> GetPagedCourses(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? category = null,
            [FromQuery] string? level = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool ascending = false)
        {
            try
            {
                Expression<Func<Course, bool>>? predicate = null;

                // Build predicate based on filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    predicate = c => c.Title.Contains(searchTerm) || 
                                   c.Description.Contains(searchTerm) || 
                                   c.Instructor.Contains(searchTerm);
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    var categoryPredicate = c => c.Category.ToLower() == category.ToLower();
                    predicate = predicate == null ? categoryPredicate : 
                               Expression.Lambda<Func<Course, bool>>(
                                   Expression.AndAlso(predicate.Body, categoryPredicate.Body), 
                                   predicate.Parameters);
                }

                if (!string.IsNullOrWhiteSpace(level))
                {
                    var levelPredicate = c => c.Level.ToLower() == level.ToLower();
                    predicate = predicate == null ? levelPredicate : 
                               Expression.Lambda<Func<Course, bool>>(
                                   Expression.AndAlso(predicate.Body, levelPredicate.Body), 
                                   predicate.Parameters);
                }

                // Apply sorting
                Expression<Func<Course, object>> orderBy = sortBy.ToLower() switch
                {
                    "title" => c => c.Title,
                    "price" => c => c.Price,
                    "createdat" => c => c.CreatedAt,
                    "updatedat" => c => c.UpdatedAt,
                    _ => c => c.CreatedAt
                };

                if (predicate != null)
                {
                    return await GetPagedAsync<Course, CourseDto, object>(_courseRepository, pageNumber, pageSize, predicate, orderBy, ascending);
                }
                else
                {
                    return await GetPagedAsync<Course, CourseDto, object>(_courseRepository, pageNumber, pageSize, orderBy, ascending);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged courses");
                return HandleInternalServerErrorResult();
            }
        }
    }
}
