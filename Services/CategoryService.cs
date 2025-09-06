using Microsoft.EntityFrameworkCore;
using CourseManager.API.Data;
using CourseManager.API.Models;

namespace CourseManager.API.Services
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        public CategoryService(CourseManagerDbContext context, ILogger<CategoryService> logger) 
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            return await CreateAsync(category);
        }

        public async Task<Category> UpdateCategoryAsync(int id, Category category)
        {
            return await UpdateAsync(id, category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await SoftDeleteAsync(id);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active categories");
                throw;
            }
        }
    }
}
