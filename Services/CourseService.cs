using Microsoft.EntityFrameworkCore;
using CourseManager.API.Data;
using CourseManager.API.Models;

namespace CourseManager.API.Services
{
    public class CourseService : BaseService<Course>, ICourseService
    {
        public CourseService(CourseManagerDbContext context, ILogger<CourseService> logger) 
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all courses");
                throw;
            }
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting course with ID {CourseId}", id);
                throw;
            }
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            return await CreateAsync(course);
        }

        public async Task<Course> UpdateCourseAsync(int id, Course course)
        {
            return await UpdateAsync(id, course);
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            return await SoftDeleteAsync(id);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.Category.ToLower() == category.ToLower() && c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by category {Category}", category);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.IsActive && 
                               (c.Title.Contains(searchTerm) || 
                                c.Description.Contains(searchTerm) ||
                                c.Instructor.Contains(searchTerm)))
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching courses with term {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
}

