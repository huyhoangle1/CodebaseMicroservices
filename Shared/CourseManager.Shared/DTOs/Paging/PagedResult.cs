namespace CourseManager.Shared.DTOs.Paging
{
    /// <summary>
    /// Kết quả phân trang chung
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu</typeparam>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int FirstItemOnPage => (PageNumber - 1) * PageSize + 1;
        public int LastItemOnPage => Math.Min(PageNumber * PageSize, TotalCount);
    }

    /// <summary>
    /// Request phân trang cơ bản
    /// </summary>
    public class PagingRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool Ascending { get; set; } = true;
    }

    /// <summary>
    /// Request phân trang nâng cao
    /// </summary>
    public class AdvancedPagingRequest : PagingRequest
    {
        public Dictionary<string, object>? Filters { get; set; }
        public List<string>? Includes { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>
    /// Response phân trang với metadata
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu</typeparam>
    public class PagedResponse<T> : PagedResult<T>
    {
        public PagingMetadata Metadata { get; set; } = new PagingMetadata();
    }

    /// <summary>
    /// Metadata phân trang
    /// </summary>
    public class PagingMetadata
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool Ascending { get; set; }
        public Dictionary<string, object>? AppliedFilters { get; set; }
        public int FilteredCount { get; set; }
    }
}
