using Microsoft.EntityFrameworkCore;
using CourseManager.API.Data;
using CourseManager.API.Models;
using System.Linq.Expressions;

namespace CourseManager.API.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        public CourseRepository(CourseManagerDbContext context, ILogger<CourseRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
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
                _logger.LogError(ex, "Error occurred while getting active courses");
                throw;
            }
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

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructor)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.Instructor.ToLower().Contains(instructor.ToLower()) && c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by instructor {Instructor}", instructor);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetCoursesByLevelAsync(string level)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.Level.ToLower() == level.ToLower() && c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by level {Level}", level);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetCoursesByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.IsActive && c.Price >= minPrice && c.Price <= maxPrice)
                    .OrderBy(c => c.Price)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting courses by price range {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetFeaturedCoursesAsync(int count = 10)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.IsActive && c.OriginalPrice.HasValue && c.OriginalPrice > c.Price)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting featured courses");
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetNewestCoursesAsync(int count = 10)
        {
            try
            {
                return await _dbSet
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting newest courses");
                throw;
            }
        }

        public async Task<IEnumerable<Course>> GetPopularCoursesAsync(int count = 10)
        {
            try
            {
                // Giả sử popular courses là những courses có nhiều order items nhất
                return await _dbSet
                    .Where(c => c.IsActive)
                    .Include(c => c.OrderItems)
                    .OrderByDescending(c => c.OrderItems.Count)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting popular courses");
                throw;
            }
        }
    }
}
