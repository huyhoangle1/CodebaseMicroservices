using CourseManager.Shared.DTOs.Notification;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Service Elasticsearch cho tìm kiếm và phân tích dữ liệu
    /// </summary>
    public interface IElasticsearchService
    {
        // Index Management
        Task<bool> CreateIndexAsync<T>(string indexName) where T : class;
        Task<bool> DeleteIndexAsync(string indexName);
        Task<bool> IndexExistsAsync(string indexName);
        Task<bool> RefreshIndexAsync(string indexName);
        Task<bool> ReindexAsync<T>(string sourceIndex, string targetIndex) where T : class;

        // Document Operations
        Task<string> IndexDocumentAsync<T>(string indexName, T document, string? id = null) where T : class;
        Task<bool> IndexDocumentAsync<T>(string indexName, T document, string id) where T : class;
        Task<bool> UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class;
        Task<bool> DeleteDocumentAsync(string indexName, string id);
        Task<T?> GetDocumentAsync<T>(string indexName, string id) where T : class;
        Task<bool> DocumentExistsAsync(string indexName, string id);

        // Bulk Operations
        Task<BulkResponse> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents) where T : class;
        Task<BulkResponse> BulkUpdateAsync<T>(string indexName, IEnumerable<BulkUpdateItem<T>> updates) where T : class;
        Task<BulkResponse> BulkDeleteAsync(string indexName, IEnumerable<string> ids);

        // Search Operations
        Task<SearchResponse<T>> SearchAsync<T>(string indexName, SearchRequest request) where T : class;
        Task<SearchResponse<T>> SearchAsync<T>(string indexName, string query, int from = 0, int size = 10) where T : class;
        Task<SearchResponse<T>> MultiSearchAsync<T>(string indexName, IEnumerable<SearchRequest> requests) where T : class;
        Task<SearchResponse<T>> SearchWithFiltersAsync<T>(string indexName, SearchRequest request, Dictionary<string, object> filters) where T : class;

        // Aggregations
        Task<AggregationResponse> GetAggregationsAsync(string indexName, string field, string aggregationType = "terms");
        Task<AggregationResponse> GetDateHistogramAsync(string indexName, string field, string interval = "1d");
        Task<AggregationResponse> GetRangeAggregationAsync(string indexName, string field, List<RangeAggregation> ranges);

        // Course Search
        Task<SearchResponse<CourseSearchDocument>> SearchCoursesAsync(CourseSearchRequest request);
        Task<SearchResponse<CourseSearchDocument>> SearchCoursesByCategoryAsync(int categoryId, int from = 0, int size = 10);
        Task<SearchResponse<CourseSearchDocument>> SearchCoursesByInstructorAsync(int instructorId, int from = 0, int size = 10);
        Task<SearchResponse<CourseSearchDocument>> SearchCoursesByPriceRangeAsync(decimal minPrice, decimal maxPrice, int from = 0, int size = 10);

        // Order Search
        Task<SearchResponse<OrderSearchDocument>> SearchOrdersAsync(OrderSearchRequest request);
        Task<SearchResponse<OrderSearchDocument>> SearchOrdersByUserAsync(int userId, int from = 0, int size = 10);
        Task<SearchResponse<OrderSearchDocument>> SearchOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int from = 0, int size = 10);

        // User Search
        Task<SearchResponse<UserSearchDocument>> SearchUsersAsync(UserSearchRequest request);
        Task<SearchResponse<UserSearchDocument>> SearchUsersByRoleAsync(string role, int from = 0, int size = 10);

        // Analytics
        Task<AnalyticsResponse> GetCourseAnalyticsAsync(AnalyticsRequest request);
        Task<AnalyticsResponse> GetOrderAnalyticsAsync(AnalyticsRequest request);
        Task<AnalyticsResponse> GetUserAnalyticsAsync(AnalyticsRequest request);

        // Health & Monitoring
        Task<ElasticsearchHealth> GetHealthAsync();
        Task<ClusterInfo> GetClusterInfoAsync();
        Task<IndexStats> GetIndexStatsAsync(string indexName);
        Task<bool> IsHealthyAsync();

        // Configuration
        Task<bool> UpdateMappingAsync<T>(string indexName) where T : class;
        Task<bool> UpdateSettingsAsync(string indexName, Dictionary<string, object> settings);
    }

    /// <summary>
    /// Search Request
    /// </summary>
    public class SearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
        public List<string> Fields { get; set; } = new();
        public List<SortField> SortFields { get; set; } = new();
        public Dictionary<string, object> Filters { get; set; } = new();
        public List<string> HighlightFields { get; set; } = new();
        public bool Explain { get; set; } = false;
        public string[] Indices { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Sort Field
    /// </summary>
    public class SortField
    {
        public string Field { get; set; } = string.Empty;
        public string Order { get; set; } = "asc"; // asc, desc
    }

    /// <summary>
    /// Search Response
    /// </summary>
    public class SearchResponse<T>
    {
        public List<T> Documents { get; set; } = new();
        public long Total { get; set; }
        public long Took { get; set; }
        public bool TimedOut { get; set; }
        public Dictionary<string, object> Aggregations { get; set; } = new();
        public List<Highlight> Highlights { get; set; } = new();
        public double MaxScore { get; set; }
    }

    /// <summary>
    /// Highlight
    /// </summary>
    public class Highlight
    {
        public string Field { get; set; } = string.Empty;
        public List<string> Fragments { get; set; } = new();
    }

    /// <summary>
    /// Bulk Response
    /// </summary>
    public class BulkResponse
    {
        public bool IsValid { get; set; }
        public int ItemsProcessed { get; set; }
        public int ItemsFailed { get; set; }
        public List<string> Errors { get; set; } = new();
        public long Took { get; set; }
    }

    /// <summary>
    /// Bulk Update Item
    /// </summary>
    public class BulkUpdateItem<T>
    {
        public string Id { get; set; } = string.Empty;
        public T Document { get; set; } = default!;
    }

    /// <summary>
    /// Aggregation Response
    /// </summary>
    public class AggregationResponse
    {
        public Dictionary<string, object> Aggregations { get; set; } = new();
        public long Took { get; set; }
        public bool TimedOut { get; set; }
    }

    /// <summary>
    /// Range Aggregation
    /// </summary>
    public class RangeAggregation
    {
        public string Key { get; set; } = string.Empty;
        public object? From { get; set; }
        public object? To { get; set; }
    }

    /// <summary>
    /// Course Search Document
    /// </summary>
    public class CourseSearchDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int InstructorId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public List<string> Tags { get; set; } = new();
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int Duration { get; set; } // in minutes
        public string Level { get; set; } = string.Empty; // Beginner, Intermediate, Advanced
        public string Language { get; set; } = string.Empty;
    }

    /// <summary>
    /// Course Search Request
    /// </summary>
    public class CourseSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
        public List<int> CategoryIds { get; set; } = new();
        public List<int> InstructorIds { get; set; } = new();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRating { get; set; }
        public List<string> Levels { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public bool? IsActive { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public List<SortField> SortFields { get; set; } = new();
    }

    /// <summary>
    /// Order Search Document
    /// </summary>
    public class OrderSearchDocument
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<OrderItemSearchDocument> Items { get; set; } = new();
        public string ShippingAddress { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Order Item Search Document
    /// </summary>
    public class OrderItemSearchDocument
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Order Search Request
    /// </summary>
    public class OrderSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
        public List<int> UserIds { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
        public List<string> PaymentMethods { get; set; } = new();
        public List<string> PaymentStatuses { get; set; } = new();
        public DateTime? OrderDateFrom { get; set; }
        public DateTime? OrderDateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public List<SortField> SortFields { get; set; } = new();
    }

    /// <summary>
    /// User Search Document
    /// </summary>
    public class UserSearchDocument
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public int CourseCount { get; set; }
        public int OrderCount { get; set; }
    }

    /// <summary>
    /// User Search Request
    /// </summary>
    public class UserSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
        public List<string> Roles { get; set; } = new();
        public bool? IsActive { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? LastLoginAfter { get; set; }
        public DateTime? LastLoginBefore { get; set; }
        public List<string> Skills { get; set; } = new();
        public string? Location { get; set; }
        public List<SortField> SortFields { get; set; } = new();
    }

    /// <summary>
    /// Analytics Request
    /// </summary>
    public class AnalyticsRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Interval { get; set; } = "1d"; // 1h, 1d, 1w, 1M
        public List<string> Metrics { get; set; } = new();
        public Dictionary<string, object> Filters { get; set; } = new();
    }

    /// <summary>
    /// Analytics Response
    /// </summary>
    public class AnalyticsResponse
    {
        public Dictionary<string, object> Data { get; set; } = new();
        public List<TimeSeriesData> TimeSeries { get; set; } = new();
        public Dictionary<string, object> Aggregations { get; set; } = new();
        public long Took { get; set; }
    }

    /// <summary>
    /// Time Series Data
    /// </summary>
    public class TimeSeriesData
    {
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Values { get; set; } = new();
    }

    /// <summary>
    /// Elasticsearch Health
    /// </summary>
    public class ElasticsearchHealth
    {
        public string Status { get; set; } = string.Empty;
        public int NumberOfNodes { get; set; }
        public int NumberOfDataNodes { get; set; }
        public int ActivePrimaryShards { get; set; }
        public int ActiveShards { get; set; }
        public int RelocatingShards { get; set; }
        public int InitializingShards { get; set; }
        public int UnassignedShards { get; set; }
    }

    /// <summary>
    /// Cluster Info
    /// </summary>
    public class ClusterInfo
    {
        public string ClusterName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int NumberOfNodes { get; set; }
        public int NumberOfDataNodes { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    /// <summary>
    /// Index Stats
    /// </summary>
    public class IndexStats
    {
        public string IndexName { get; set; } = string.Empty;
        public long DocumentCount { get; set; }
        public long StoreSize { get; set; }
        public long IndexSize { get; set; }
        public int ShardCount { get; set; }
        public Dictionary<string, object> Stats { get; set; } = new();
    }
}
