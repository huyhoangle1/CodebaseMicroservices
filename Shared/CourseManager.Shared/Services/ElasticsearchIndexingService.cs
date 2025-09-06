using CourseManager.Shared.DTOs.Notification;
using CourseManager.Shared.Models;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Service để index dữ liệu vào Elasticsearch
    /// </summary>
    public interface IElasticsearchIndexingService
    {
        // Course indexing
        Task<bool> IndexCourseAsync(Course course);
        Task<bool> IndexCoursesAsync(IEnumerable<Course> courses);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int courseId);

        // Order indexing
        Task<bool> IndexOrderAsync(Order order);
        Task<bool> IndexOrdersAsync(IEnumerable<Order> orders);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int orderId);

        // User indexing
        Task<bool> IndexUserAsync(User user);
        Task<bool> IndexUsersAsync(IEnumerable<User> users);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);

        // Category indexing
        Task<bool> IndexCategoryAsync(Category category);
        Task<bool> IndexCategoriesAsync(IEnumerable<Category> categories);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int categoryId);

        // Bulk operations
        Task<bool> BulkIndexAllDataAsync();
        Task<bool> ReindexAllDataAsync();
        Task<bool> SyncDataAsync();

        // Index management
        Task<bool> CreateAllIndicesAsync();
        Task<bool> DeleteAllIndicesAsync();
        Task<bool> RefreshAllIndicesAsync();
    }

    /// <summary>
    /// Implementation của Elasticsearch Indexing Service
    /// </summary>
    public class ElasticsearchIndexingService : IElasticsearchIndexingService
    {
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<ElasticsearchIndexingService> _logger;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;

        public ElasticsearchIndexingService(
            IElasticsearchService elasticsearchService,
            ILogger<ElasticsearchIndexingService> logger,
            IUserService userService,
            ICourseService courseService,
            IOrderService orderService,
            ICategoryService categoryService)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
            _userService = userService;
            _courseService = courseService;
            _orderService = orderService;
            _categoryService = categoryService;
        }

        #region Course Indexing

        public async Task<bool> IndexCourseAsync(Course course)
        {
            try
            {
                var searchDocument = await ConvertToCourseSearchDocumentAsync(course);
                var id = await _elasticsearchService.IndexDocumentAsync("courses", searchDocument, course.Id.ToString());
                
                if (!string.IsNullOrEmpty(id))
                {
                    _logger.LogDebug("Indexed course {CourseId} to Elasticsearch", course.Id);
                    return true;
                }

                _logger.LogWarning("Failed to index course {CourseId}", course.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing course {CourseId}", course.Id);
                return false;
            }
        }

        public async Task<bool> IndexCoursesAsync(IEnumerable<Course> courses)
        {
            try
            {
                var searchDocuments = new List<CourseSearchDocument>();
                
                foreach (var course in courses)
                {
                    var searchDocument = await ConvertToCourseSearchDocumentAsync(course);
                    searchDocuments.Add(searchDocument);
                }

                var result = await _elasticsearchService.BulkIndexAsync("courses", searchDocuments);
                
                if (result.IsValid)
                {
                    _logger.LogInformation("Bulk indexed {Count} courses to Elasticsearch", searchDocuments.Count);
                    return true;
                }

                _logger.LogWarning("Failed to bulk index courses: {Errors}", string.Join(", ", result.Errors));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing courses");
                return false;
            }
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            try
            {
                var searchDocument = await ConvertToCourseSearchDocumentAsync(course);
                var success = await _elasticsearchService.UpdateDocumentAsync("courses", course.Id.ToString(), searchDocument);
                
                if (success)
                {
                    _logger.LogDebug("Updated course {CourseId} in Elasticsearch", course.Id);
                    return true;
                }

                _logger.LogWarning("Failed to update course {CourseId} in Elasticsearch", course.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course {CourseId} in Elasticsearch", course.Id);
                return false;
            }
        }

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            try
            {
                var success = await _elasticsearchService.DeleteDocumentAsync("courses", courseId.ToString());
                
                if (success)
                {
                    _logger.LogDebug("Deleted course {CourseId} from Elasticsearch", courseId);
                    return true;
                }

                _logger.LogWarning("Failed to delete course {CourseId} from Elasticsearch", courseId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId} from Elasticsearch", courseId);
                return false;
            }
        }

        #endregion

        #region Order Indexing

        public async Task<bool> IndexOrderAsync(Order order)
        {
            try
            {
                var searchDocument = await ConvertToOrderSearchDocumentAsync(order);
                var id = await _elasticsearchService.IndexDocumentAsync("orders", searchDocument, order.Id.ToString());
                
                if (!string.IsNullOrEmpty(id))
                {
                    _logger.LogDebug("Indexed order {OrderId} to Elasticsearch", order.Id);
                    return true;
                }

                _logger.LogWarning("Failed to index order {OrderId}", order.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing order {OrderId}", order.Id);
                return false;
            }
        }

        public async Task<bool> IndexOrdersAsync(IEnumerable<Order> orders)
        {
            try
            {
                var searchDocuments = new List<OrderSearchDocument>();
                
                foreach (var order in orders)
                {
                    var searchDocument = await ConvertToOrderSearchDocumentAsync(order);
                    searchDocuments.Add(searchDocument);
                }

                var result = await _elasticsearchService.BulkIndexAsync("orders", searchDocuments);
                
                if (result.IsValid)
                {
                    _logger.LogInformation("Bulk indexed {Count} orders to Elasticsearch", searchDocuments.Count);
                    return true;
                }

                _logger.LogWarning("Failed to bulk index orders: {Errors}", string.Join(", ", result.Errors));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing orders");
                return false;
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                var searchDocument = await ConvertToOrderSearchDocumentAsync(order);
                var success = await _elasticsearchService.UpdateDocumentAsync("orders", order.Id.ToString(), searchDocument);
                
                if (success)
                {
                    _logger.LogDebug("Updated order {OrderId} in Elasticsearch", order.Id);
                    return true;
                }

                _logger.LogWarning("Failed to update order {OrderId} in Elasticsearch", order.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} in Elasticsearch", order.Id);
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                var success = await _elasticsearchService.DeleteDocumentAsync("orders", orderId.ToString());
                
                if (success)
                {
                    _logger.LogDebug("Deleted order {OrderId} from Elasticsearch", orderId);
                    return true;
                }

                _logger.LogWarning("Failed to delete order {OrderId} from Elasticsearch", orderId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId} from Elasticsearch", orderId);
                return false;
            }
        }

        #endregion

        #region User Indexing

        public async Task<bool> IndexUserAsync(User user)
        {
            try
            {
                var searchDocument = await ConvertToUserSearchDocumentAsync(user);
                var id = await _elasticsearchService.IndexDocumentAsync("users", searchDocument, user.Id.ToString());
                
                if (!string.IsNullOrEmpty(id))
                {
                    _logger.LogDebug("Indexed user {UserId} to Elasticsearch", user.Id);
                    return true;
                }

                _logger.LogWarning("Failed to index user {UserId}", user.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> IndexUsersAsync(IEnumerable<User> users)
        {
            try
            {
                var searchDocuments = new List<UserSearchDocument>();
                
                foreach (var user in users)
                {
                    var searchDocument = await ConvertToUserSearchDocumentAsync(user);
                    searchDocuments.Add(searchDocument);
                }

                var result = await _elasticsearchService.BulkIndexAsync("users", searchDocuments);
                
                if (result.IsValid)
                {
                    _logger.LogInformation("Bulk indexed {Count} users to Elasticsearch", searchDocuments.Count);
                    return true;
                }

                _logger.LogWarning("Failed to bulk index users: {Errors}", string.Join(", ", result.Errors));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing users");
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var searchDocument = await ConvertToUserSearchDocumentAsync(user);
                var success = await _elasticsearchService.UpdateDocumentAsync("users", user.Id.ToString(), searchDocument);
                
                if (success)
                {
                    _logger.LogDebug("Updated user {UserId} in Elasticsearch", user.Id);
                    return true;
                }

                _logger.LogWarning("Failed to update user {UserId} in Elasticsearch", user.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId} in Elasticsearch", user.Id);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var success = await _elasticsearchService.DeleteDocumentAsync("users", userId.ToString());
                
                if (success)
                {
                    _logger.LogDebug("Deleted user {UserId} from Elasticsearch", userId);
                    return true;
                }

                _logger.LogWarning("Failed to delete user {UserId} from Elasticsearch", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId} from Elasticsearch", userId);
                return false;
            }
        }

        #endregion

        #region Category Indexing

        public async Task<bool> IndexCategoryAsync(Category category)
        {
            try
            {
                var searchDocument = ConvertToCategorySearchDocument(category);
                var id = await _elasticsearchService.IndexDocumentAsync("categories", searchDocument, category.Id.ToString());
                
                if (!string.IsNullOrEmpty(id))
                {
                    _logger.LogDebug("Indexed category {CategoryId} to Elasticsearch", category.Id);
                    return true;
                }

                _logger.LogWarning("Failed to index category {CategoryId}", category.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing category {CategoryId}", category.Id);
                return false;
            }
        }

        public async Task<bool> IndexCategoriesAsync(IEnumerable<Category> categories)
        {
            try
            {
                var searchDocuments = categories.Select(ConvertToCategorySearchDocument).ToList();
                var result = await _elasticsearchService.BulkIndexAsync("categories", searchDocuments);
                
                if (result.IsValid)
                {
                    _logger.LogInformation("Bulk indexed {Count} categories to Elasticsearch", searchDocuments.Count);
                    return true;
                }

                _logger.LogWarning("Failed to bulk index categories: {Errors}", string.Join(", ", result.Errors));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing categories");
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                var searchDocument = ConvertToCategorySearchDocument(category);
                var success = await _elasticsearchService.UpdateDocumentAsync("categories", category.Id.ToString(), searchDocument);
                
                if (success)
                {
                    _logger.LogDebug("Updated category {CategoryId} in Elasticsearch", category.Id);
                    return true;
                }

                _logger.LogWarning("Failed to update category {CategoryId} in Elasticsearch", category.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId} in Elasticsearch", category.Id);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                var success = await _elasticsearchService.DeleteDocumentAsync("categories", categoryId.ToString());
                
                if (success)
                {
                    _logger.LogDebug("Deleted category {CategoryId} from Elasticsearch", categoryId);
                    return true;
                }

                _logger.LogWarning("Failed to delete category {CategoryId} from Elasticsearch", categoryId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId} from Elasticsearch", categoryId);
                return false;
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> BulkIndexAllDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting bulk index of all data to Elasticsearch");

                // Index all data types
                var tasks = new List<Task<bool>>
                {
                    IndexAllCoursesAsync(),
                    IndexAllOrdersAsync(),
                    IndexAllUsersAsync(),
                    IndexAllCategoriesAsync()
                };

                var results = await Task.WhenAll(tasks);
                var success = results.All(r => r);

                if (success)
                {
                    _logger.LogInformation("Successfully bulk indexed all data to Elasticsearch");
                }
                else
                {
                    _logger.LogWarning("Some data failed to index during bulk operation");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk index of all data");
                return false;
            }
        }

        public async Task<bool> ReindexAllDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting reindex of all data to Elasticsearch");

                // Delete all indices first
                await DeleteAllIndicesAsync();

                // Create all indices
                await CreateAllIndicesAsync();

                // Index all data
                var success = await BulkIndexAllDataAsync();

                if (success)
                {
                    _logger.LogInformation("Successfully reindexed all data to Elasticsearch");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reindex of all data");
                return false;
            }
        }

        public async Task<bool> SyncDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data sync to Elasticsearch");

                // This would typically compare database data with Elasticsearch data
                // and sync any differences
                var success = await BulkIndexAllDataAsync();

                if (success)
                {
                    _logger.LogInformation("Successfully synced data to Elasticsearch");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data sync");
                return false;
            }
        }

        #endregion

        #region Index Management

        public async Task<bool> CreateAllIndicesAsync()
        {
            try
            {
                _logger.LogInformation("Creating all Elasticsearch indices");

                var tasks = new List<Task<bool>>
                {
                    _elasticsearchService.CreateIndexAsync<CourseSearchDocument>("courses"),
                    _elasticsearchService.CreateIndexAsync<OrderSearchDocument>("orders"),
                    _elasticsearchService.CreateIndexAsync<UserSearchDocument>("users"),
                    _elasticsearchService.CreateIndexAsync<CategorySearchDocument>("categories")
                };

                var results = await Task.WhenAll(tasks);
                var success = results.All(r => r);

                if (success)
                {
                    _logger.LogInformation("Successfully created all Elasticsearch indices");
                }
                else
                {
                    _logger.LogWarning("Some indices failed to create");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating all indices");
                return false;
            }
        }

        public async Task<bool> DeleteAllIndicesAsync()
        {
            try
            {
                _logger.LogInformation("Deleting all Elasticsearch indices");

                var indices = new[] { "courses", "orders", "users", "categories" };
                var tasks = indices.Select(index => _elasticsearchService.DeleteIndexAsync(index));
                var results = await Task.WhenAll(tasks);

                var success = results.All(r => r);

                if (success)
                {
                    _logger.LogInformation("Successfully deleted all Elasticsearch indices");
                }
                else
                {
                    _logger.LogWarning("Some indices failed to delete");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all indices");
                return false;
            }
        }

        public async Task<bool> RefreshAllIndicesAsync()
        {
            try
            {
                _logger.LogInformation("Refreshing all Elasticsearch indices");

                var indices = new[] { "courses", "orders", "users", "categories" };
                var tasks = indices.Select(index => _elasticsearchService.RefreshIndexAsync(index));
                var results = await Task.WhenAll(tasks);

                var success = results.All(r => r);

                if (success)
                {
                    _logger.LogInformation("Successfully refreshed all Elasticsearch indices");
                }
                else
                {
                    _logger.LogWarning("Some indices failed to refresh");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing all indices");
                return false;
            }
        }

        #endregion

        #region Private Methods

        private async Task<CourseSearchDocument> ConvertToCourseSearchDocumentAsync(Course course)
        {
            // Get additional data needed for search document
            var category = await _categoryService.GetByIdAsync(course.CategoryId);
            var instructor = await _userService.GetByIdAsync(course.InstructorId);

            return new CourseSearchDocument
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Content = course.Content,
                Price = course.Price,
                InstructorName = instructor?.FirstName + " " + instructor?.LastName ?? "Unknown",
                InstructorId = course.InstructorId,
                CategoryName = category?.Name ?? "Unknown",
                CategoryId = course.CategoryId,
                Tags = course.Tags?.Split(',').Select(t => t.Trim()).ToList() ?? new List<string>(),
                Rating = course.Rating,
                ReviewCount = course.ReviewCount,
                StudentCount = course.StudentCount,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                IsActive = course.IsActive,
                ImageUrl = course.ImageUrl,
                VideoUrl = course.VideoUrl,
                Duration = course.Duration,
                Level = course.Level,
                Language = course.Language
            };
        }

        private async Task<OrderSearchDocument> ConvertToOrderSearchDocumentAsync(Order order)
        {
            // Get additional data needed for search document
            var user = await _userService.GetByIdAsync(order.UserId);

            return new OrderSearchDocument
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = user?.FirstName + " " + user?.LastName ?? "Unknown",
                UserEmail = user?.Email ?? "Unknown",
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderDate = order.OrderDate,
                CompletedDate = order.CompletedDate,
                Items = order.OrderItems?.Select(oi => new OrderItemSearchDocument
                {
                    CourseId = oi.CourseId,
                    CourseTitle = oi.Course?.Title ?? "Unknown",
                    Price = oi.Price,
                    Quantity = oi.Quantity
                }).ToList() ?? new List<OrderItemSearchDocument>(),
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes
            };
        }

        private async Task<UserSearchDocument> ConvertToUserSearchDocumentAsync(User user)
        {
            // Get additional data needed for search document
            var roles = await _userService.GetUserRolesAsync(user.Id);
            var courseCount = await _courseService.GetCountByInstructorAsync(user.Id);
            var orderCount = await _orderService.GetCountByUserAsync(user.Id);

            return new UserSearchDocument
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive,
                Avatar = user.Avatar,
                Bio = user.Bio,
                Location = user.Location,
                Skills = user.Skills?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string>(),
                CourseCount = courseCount,
                OrderCount = orderCount
            };
        }

        private CategorySearchDocument ConvertToCategorySearchDocument(Category category)
        {
            return new CategorySearchDocument
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        private async Task<bool> IndexAllCoursesAsync()
        {
            try
            {
                var courses = await _courseService.GetAllAsync();
                return await IndexCoursesAsync(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing all courses");
                return false;
            }
        }

        private async Task<bool> IndexAllOrdersAsync()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();
                return await IndexOrdersAsync(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing all orders");
                return false;
            }
        }

        private async Task<bool> IndexAllUsersAsync()
        {
            try
            {
                var users = await _userService.GetAllAsync();
                return await IndexUsersAsync(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing all users");
                return false;
            }
        }

        private async Task<bool> IndexAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return await IndexCategoriesAsync(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing all categories");
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Category Search Document
    /// </summary>
    public class CategorySearchDocument
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
