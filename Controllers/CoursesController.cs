using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.API.Services;
using CourseManager.API.DTOs;
using CourseManager.API.Models;

namespace CourseManager.API.Controllers
{
    [Authorize]
    public class CoursesController : BaseController
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService, IMapper mapper, ILogger<CoursesController> logger) 
            : base(mapper, logger)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Lấy tất cả khóa học
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return HandleResult(courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all courses");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy khóa học theo ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CourseDto>> GetCourseById(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    return HandleBadRequestResult($"Course with ID {id} not found");
                }

                var courseDto = _mapper.Map<CourseDto>(course);
                return HandleResult(courseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course with ID {CourseId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo khóa học mới
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CreateCourseRequest request)
        {
            try
            {
                var course = _mapper.Map<Course>(request);
                var createdCourse = await _courseService.CreateCourseAsync(course);
                var courseDto = _mapper.Map<CourseDto>(createdCourse);

                return HandleCreatedResult(courseDto, nameof(GetCourseById), new { id = createdCourse.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating course");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật khóa học
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
        {
            try
            {
                var course = _mapper.Map<Course>(request);
                var updatedCourse = await _courseService.UpdateCourseAsync(id, course);
                var courseDto = _mapper.Map<CourseDto>(updatedCourse);

                return HandleResult(courseDto);
            }
            catch (KeyNotFoundException)
            {
                return HandleBadRequestResult($"Course with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating course with ID {CourseId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xóa khóa học
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            try
            {
                var result = await _courseService.DeleteCourseAsync(id);
                if (!result)
                {
                    return HandleBadRequestResult($"Course with ID {id} not found");
                }

                return HandleNoContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting course with ID {CourseId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tìm kiếm khóa học theo danh mục
        /// </summary>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesByCategory(string category)
        {
            try
            {
                var courses = await _courseService.GetCoursesByCategoryAsync(category);
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

                var courses = await _courseService.SearchCoursesAsync(q);
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return HandleResult(courseDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching courses with query {Query}", q);
                return HandleInternalServerErrorResult();
            }
        }
    }
}
