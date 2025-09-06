using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.Attributes;

namespace CourseManager.Shared.Controllers
{
    /// <summary>
    /// Controller cho tìm kiếm với Elasticsearch
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IElasticsearchService _elasticsearchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            IElasticsearchService elasticsearchService,
            ILogger<SearchController> logger)
        {
            _elasticsearchService = elasticsearchService;
            _logger = logger;
        }

        #region Global Search

        /// <summary>
        /// Tìm kiếm toàn cục
        /// </summary>
        [HttpGet("global")]
        [RequirePermission("search", "read")]
        public async Task<IActionResult> GlobalSearch([FromQuery] string query, [FromQuery] int from = 0, [FromQuery] int size = 10)
        {
            try
            {
                var searchRequest = new SearchRequest
                {
                    Query = query,
                    From = from,
                    Size = size
                };

                // Search across multiple indices
                var courseResults = await _elasticsearchService.SearchAsync<CourseSearchDocument>("courses", searchRequest);
                var orderResults = await _elasticsearchService.SearchAsync<OrderSearchDocument>("orders", searchRequest);
                var userResults = await _elasticsearchService.SearchAsync<UserSearchDocument>("users", searchRequest);

                var response = new
                {
                    Query = query,
                    TotalResults = courseResults.Total + orderResults.Total + userResults.Total,
                    Results = new
                    {
                        Courses = new
                        {
                            Count = courseResults.Total,
                            Items = courseResults.Documents.Take(5) // Limit for global search
                        },
                        Orders = new
                        {
                            Count = orderResults.Total,
                            Items = orderResults.Documents.Take(5)
                        },
                        Users = new
                        {
                            Count = userResults.Total,
                            Items = userResults.Documents.Take(5)
                        }
                    },
                    Took = Math.Max(courseResults.Took, Math.Max(orderResults.Took, userResults.Took))
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing global search for query: {Query}", query);
                return StatusCode(500, "Internal server error during search");
            }
        }

        #endregion

        #region Course Search

        /// <summary>
        /// Tìm kiếm khóa học
        /// </summary>
        [HttpPost("courses")]
        [RequirePermission("courses", "read")]
        public async Task<IActionResult> SearchCourses([FromBody] CourseSearchRequest request)
        {
            try
            {
                var results = await _elasticsearchService.SearchCoursesAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching courses");
                return StatusCode(500, "Internal server error during course search");
            }
        }

        /// <summary>
        /// Tìm kiếm khóa học theo danh mục
        /// </summary>
        [HttpGet("courses/category/{categoryId}")]
        [RequirePermission("courses", "read")]
        public async Task<IActionResult> SearchCoursesByCategory(int categoryId, [FromQuery] int from = 0, [FromQuery] int size = 10)
        {
            try
            {
                var results = await _elasticsearchService.SearchCoursesByCategoryAsync(categoryId, from, size);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching courses by category {CategoryId}", categoryId);
                return StatusCode(500, "Internal server error during course search");
            }
        }

        /// <summary>
        /// Tìm kiếm khóa học theo giảng viên
        /// </summary>
        [HttpGet("courses/instructor/{instructorId}")]
        [RequirePermission("courses", "read")]
        public async Task<IActionResult> SearchCoursesByInstructor(int instructorId, [FromQuery] int from = 0, [FromQuery] int size = 10)
        {
            try
            {
                var results = await _elasticsearchService.SearchCoursesByInstructorAsync(instructorId, from, size);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching courses by instructor {InstructorId}", instructorId);
                return StatusCode(500, "Internal server error during course search");
            }
        }

        /// <summary>
        /// Tìm kiếm khóa học theo khoảng giá
        /// </summary>
        [HttpGet("courses/price-range")]
        [RequirePermission("courses", "read")]
        public async Task<IActionResult> SearchCoursesByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice, [FromQuery] int from = 0, [FromQuery] int size = 10)
        {
            try
            {
                var results = await _elasticsearchService.SearchCoursesByPriceRangeAsync(minPrice, maxPrice, from, size);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching courses by price range {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return StatusCode(500, "Internal server error during course search");
            }
        }

        /// <summary>
        /// Tìm kiếm khóa học nâng cao
        /// </summary>
        [HttpPost("courses/advanced")]
        [RequirePermission("courses", "read")]
        public async Task<IActionResult> AdvancedCourseSearch([FromBody] CourseSearchRequest request)
        {
            try
            {
                var results = await _elasticsearchService.SearchCoursesAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing advanced course search");
                return StatusCode(500, "Internal server error during advanced course search");
            }
        }

        #endregion

        #region Order Search

        /// <summary>
        /// Tìm kiếm đơn hàng
        /// </summary>
        [HttpPost("orders")]
        [RequirePermission("orders", "read")]
        public async Task<IActionResult> SearchOrders([FromBody] OrderSearchRequest request)
        {
            try
            {
                var results = await _elasticsearchService.SearchOrdersAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders");
                return StatusCode(500, "Internal server error during order search");
            }
        }

        /// <summary>
        /// Tìm kiếm đơn hàng theo người dùng
        /// </summary>
        [HttpGet("orders/user/{userId}")]
        [RequirePermission("orders", "read")]
        public async Task<IActionResult> SearchOrdersByUser(int userId, [FromQuery] int from = 0, [FromQuery] int size = 10)
        {
            try
            {
                var results = await _elasticsearchService.SearchOrdersByUserAsync(userId, from, size);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders by user {UserId}", userId);
                return StatusCode(500, "Internal server error during order search");
            }
        }

        /// <summary>
        /// Tìm kiếm đơn hàng theo khoảng thời gian
        /// </summary>
        [HttpGet("orders/date-range")]
        [RequirePermission("orders", "read")]
        public async Task<IActionResult> SearchOrdersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int from = 0, [FromQuery] int size = 10)
        {
            try
            {
                var results = await _elasticsearchService.SearchOrdersByDateRangeAsync(startDate, endDate, from, size);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders by date range {StartDate}-{EndDate}", startDate, endDate);
                return StatusCode(500, "Internal server error during order search");
            }
        }

        #endregion

        #region User Search

        /// <summary>
        /// Tìm kiếm người dùng
        /// </summary>
        [HttpPost("users")]
        [RequirePermission("users", "read")]
        public async Task<IActionResult> SearchUsers([FromBody] UserSearchRequest request)
        {
            try
            {
                var results = await _elasticsearchService.SearchUsersAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, "Internal server error during user search");
            }
        }

        /// <summary>
        /// Tìm kiếm người dùng theo vai trò
        /// </summary>
        [HttpGet("users/role/{role}")]
        [RequirePermission("users", "read")]
        public async Task<IActionResult> SearchUsersByRole(string role, [FromQuery] int from = 0, [FromQuery] int size = 10)
        {
            try
            {
                var results = await _elasticsearchService.SearchUsersByRoleAsync(role, from, size);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users by role {Role}", role);
                return StatusCode(500, "Internal server error during user search");
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Phân tích khóa học
        /// </summary>
        [HttpPost("analytics/courses")]
        [RequirePermission("analytics", "read")]
        public async Task<IActionResult> GetCourseAnalytics([FromBody] AnalyticsRequest request)
        {
            try
            {
                var results = await _elasticsearchService.GetCourseAnalyticsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course analytics");
                return StatusCode(500, "Internal server error during analytics");
            }
        }

        /// <summary>
        /// Phân tích đơn hàng
        /// </summary>
        [HttpPost("analytics/orders")]
        [RequirePermission("analytics", "read")]
        public async Task<IActionResult> GetOrderAnalytics([FromBody] AnalyticsRequest request)
        {
            try
            {
                var results = await _elasticsearchService.GetOrderAnalyticsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order analytics");
                return StatusCode(500, "Internal server error during analytics");
            }
        }

        /// <summary>
        /// Phân tích người dùng
        /// </summary>
        [HttpPost("analytics/users")]
        [RequirePermission("analytics", "read")]
        public async Task<IActionResult> GetUserAnalytics([FromBody] AnalyticsRequest request)
        {
            try
            {
                var results = await _elasticsearchService.GetUserAnalyticsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user analytics");
                return StatusCode(500, "Internal server error during analytics");
            }
        }

        #endregion

        #region Aggregations

        /// <summary>
        /// Lấy thống kê theo trường
        /// </summary>
        [HttpGet("aggregations/{indexName}/{field}")]
        [RequirePermission("search", "read")]
        public async Task<IActionResult> GetAggregations(string indexName, string field, [FromQuery] string aggregationType = "terms")
        {
            try
            {
                var results = await _elasticsearchService.GetAggregationsAsync(indexName, field, aggregationType);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregations for field {Field} in index {IndexName}", field, indexName);
                return StatusCode(500, "Internal server error during aggregation");
            }
        }

        /// <summary>
        /// Lấy thống kê theo thời gian
        /// </summary>
        [HttpGet("aggregations/{indexName}/date-histogram/{field}")]
        [RequirePermission("search", "read")]
        public async Task<IActionResult> GetDateHistogram(string indexName, string field, [FromQuery] string interval = "1d")
        {
            try
            {
                var results = await _elasticsearchService.GetDateHistogramAsync(indexName, field, interval);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting date histogram for field {Field} in index {IndexName}", field, indexName);
                return StatusCode(500, "Internal server error during aggregation");
            }
        }

        /// <summary>
        /// Lấy thống kê theo khoảng
        /// </summary>
        [HttpPost("aggregations/{indexName}/range/{field}")]
        [RequirePermission("search", "read")]
        public async Task<IActionResult> GetRangeAggregation(string indexName, string field, [FromBody] List<RangeAggregation> ranges)
        {
            try
            {
                var results = await _elasticsearchService.GetRangeAggregationAsync(indexName, field, ranges);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting range aggregation for field {Field} in index {IndexName}", field, indexName);
                return StatusCode(500, "Internal server error during aggregation");
            }
        }

        #endregion

        #region Health & Monitoring

        /// <summary>
        /// Kiểm tra tình trạng Elasticsearch
        /// </summary>
        [HttpGet("health")]
        [RequirePermission("system", "read")]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                var health = await _elasticsearchService.GetHealthAsync();
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Elasticsearch health");
                return StatusCode(500, "Internal server error during health check");
            }
        }

        /// <summary>
        /// Kiểm tra tình trạng cluster
        /// </summary>
        [HttpGet("cluster/info")]
        [RequirePermission("system", "read")]
        public async Task<IActionResult> GetClusterInfo()
        {
            try
            {
                var info = await _elasticsearchService.GetClusterInfoAsync();
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cluster info");
                return StatusCode(500, "Internal server error during cluster info check");
            }
        }

        /// <summary>
        /// Lấy thống kê index
        /// </summary>
        [HttpGet("indices/{indexName}/stats")]
        [RequirePermission("system", "read")]
        public async Task<IActionResult> GetIndexStats(string indexName)
        {
            try
            {
                var stats = await _elasticsearchService.GetIndexStatsAsync(indexName);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index stats for {IndexName}", indexName);
                return StatusCode(500, "Internal server error during index stats check");
            }
        }

        #endregion

        #region Index Management

        /// <summary>
        /// Tạo index
        /// </summary>
        [HttpPost("indices/{indexName}")]
        [RequirePermission("system", "write")]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            try
            {
                var success = await _elasticsearchService.CreateIndexAsync<object>(indexName);
                if (success)
                {
                    return Ok(new { Message = $"Index {indexName} created successfully" });
                }
                return BadRequest(new { Message = $"Failed to create index {indexName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating index {IndexName}", indexName);
                return StatusCode(500, "Internal server error during index creation");
            }
        }

        /// <summary>
        /// Xóa index
        /// </summary>
        [HttpDelete("indices/{indexName}")]
        [RequirePermission("system", "write")]
        public async Task<IActionResult> DeleteIndex(string indexName)
        {
            try
            {
                var success = await _elasticsearchService.DeleteIndexAsync(indexName);
                if (success)
                {
                    return Ok(new { Message = $"Index {indexName} deleted successfully" });
                }
                return BadRequest(new { Message = $"Failed to delete index {indexName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting index {IndexName}", indexName);
                return StatusCode(500, "Internal server error during index deletion");
            }
        }

        /// <summary>
        /// Kiểm tra index tồn tại
        /// </summary>
        [HttpGet("indices/{indexName}/exists")]
        [RequirePermission("system", "read")]
        public async Task<IActionResult> IndexExists(string indexName)
        {
            try
            {
                var exists = await _elasticsearchService.IndexExistsAsync(indexName);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if index {IndexName} exists", indexName);
                return StatusCode(500, "Internal server error during index existence check");
            }
        }

        /// <summary>
        /// Refresh index
        /// </summary>
        [HttpPost("indices/{indexName}/refresh")]
        [RequirePermission("system", "write")]
        public async Task<IActionResult> RefreshIndex(string indexName)
        {
            try
            {
                var success = await _elasticsearchService.RefreshIndexAsync(indexName);
                if (success)
                {
                    return Ok(new { Message = $"Index {indexName} refreshed successfully" });
                }
                return BadRequest(new { Message = $"Failed to refresh index {indexName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing index {IndexName}", indexName);
                return StatusCode(500, "Internal server error during index refresh");
            }
        }

        #endregion
    }
}
