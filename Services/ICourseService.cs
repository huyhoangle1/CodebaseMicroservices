using CourseManager.API.Models;

namespace CourseManager.API.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course> CreateCourseAsync(Course course);
        Task<Course> UpdateCourseAsync(int id, Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category);
        Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm);
    }
}

