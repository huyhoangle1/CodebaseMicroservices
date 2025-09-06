using CourseManager.API.Models;
using System.Linq.Expressions;

namespace CourseManager.API.Repositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<IEnumerable<Course>> GetActiveCoursesAsync();
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category);
        Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm);
        Task<IEnumerable<Course>> GetCoursesByInstructorAsync(string instructor);
        Task<IEnumerable<Course>> GetCoursesByLevelAsync(string level);
        Task<IEnumerable<Course>> GetCoursesByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Course>> GetFeaturedCoursesAsync(int count = 10);
        Task<IEnumerable<Course>> GetNewestCoursesAsync(int count = 10);
        Task<IEnumerable<Course>> GetPopularCoursesAsync(int count = 10);
    }
}
