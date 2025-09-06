# Hướng dẫn cài đặt và sử dụng Elasticsearch

## 🚀 Tổng quan

Elasticsearch được tích hợp vào hệ thống Course Manager để cung cấp khả năng tìm kiếm mạnh mẽ, phân tích dữ liệu và real-time analytics.

## 🔧 Cài đặt

### 1. Sử dụng Docker Compose (Khuyến nghị)
```bash
# Chạy Elasticsearch stack
docker-compose -f docker-compose.elasticsearch.yml up -d

# Kiểm tra trạng thái
docker-compose -f docker-compose.elasticsearch.yml ps

# Xem logs
docker-compose -f docker-compose.elasticsearch.yml logs -f elasticsearch
```

### 2. Cài đặt thủ công
```bash
# Windows (sử dụng Chocolatey)
choco install elasticsearch

# Hoặc download từ: https://www.elastic.co/downloads/elasticsearch
```

### 3. Cấu hình appsettings.json
```json
{
  "Elasticsearch": {
    "BaseUrl": "http://localhost:9200",
    "Username": "",
    "Password": "",
    "EnableSsl": false,
    "TimeoutSeconds": 30,
    "MaxRetries": 3,
    "NumberOfShards": 1,
    "NumberOfReplicas": 1,
    "MaxBulkSize": 1000,
    "EnableCompression": true,
    "EnableLogging": true
  }
}
```

## 🏗️ Cấu hình Services

### 1. Program.cs Configuration
```csharp
// Thêm Elasticsearch services
builder.Services.Configure<ElasticsearchConfiguration>(
    builder.Configuration.GetSection("Elasticsearch"));

builder.Services.AddHttpClient<IElasticsearchService, ElasticsearchService>(client =>
{
    var config = builder.Configuration.GetSection("Elasticsearch").Get<ElasticsearchConfiguration>();
    client.BaseAddress = new Uri(config.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
});

builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
builder.Services.AddScoped<IElasticsearchIndexingService, ElasticsearchIndexingService>();
```

## 📊 Cấu trúc Indices

### 1. Courses Index
```json
{
  "mappings": {
    "properties": {
      "id": { "type": "integer" },
      "title": { "type": "text", "analyzer": "standard" },
      "description": { "type": "text", "analyzer": "standard" },
      "content": { "type": "text", "analyzer": "standard" },
      "price": { "type": "double" },
      "instructorName": { "type": "text", "analyzer": "standard" },
      "instructorId": { "type": "integer" },
      "categoryName": { "type": "keyword" },
      "categoryId": { "type": "integer" },
      "tags": { "type": "keyword" },
      "rating": { "type": "double" },
      "reviewCount": { "type": "integer" },
      "studentCount": { "type": "integer" },
      "createdAt": { "type": "date" },
      "updatedAt": { "type": "date" },
      "isActive": { "type": "boolean" },
      "imageUrl": { "type": "keyword" },
      "videoUrl": { "type": "keyword" },
      "duration": { "type": "integer" },
      "level": { "type": "keyword" },
      "language": { "type": "keyword" }
    }
  }
}
```

### 2. Orders Index
```json
{
  "mappings": {
    "properties": {
      "id": { "type": "integer" },
      "userId": { "type": "integer" },
      "userName": { "type": "text", "analyzer": "standard" },
      "userEmail": { "type": "keyword" },
      "totalAmount": { "type": "double" },
      "status": { "type": "keyword" },
      "paymentMethod": { "type": "keyword" },
      "paymentStatus": { "type": "keyword" },
      "orderDate": { "type": "date" },
      "completedDate": { "type": "date" },
      "shippingAddress": { "type": "text" },
      "notes": { "type": "text" }
    }
  }
}
```

### 3. Users Index
```json
{
  "mappings": {
    "properties": {
      "id": { "type": "integer" },
      "firstName": { "type": "text", "analyzer": "standard" },
      "lastName": { "type": "text", "analyzer": "standard" },
      "email": { "type": "keyword" },
      "phone": { "type": "keyword" },
      "roles": { "type": "keyword" },
      "createdAt": { "type": "date" },
      "lastLoginAt": { "type": "date" },
      "isActive": { "type": "boolean" },
      "avatar": { "type": "keyword" },
      "bio": { "type": "text" },
      "location": { "type": "keyword" },
      "skills": { "type": "keyword" },
      "courseCount": { "type": "integer" },
      "orderCount": { "type": "integer" }
    }
  }
}
```

## 🎯 Sử dụng

### 1. Tìm kiếm khóa học
```csharp
// Tìm kiếm cơ bản
var request = new CourseSearchRequest
{
    Query = "React JavaScript",
    From = 0,
    Size = 10
};
var results = await _elasticsearchService.SearchCoursesAsync(request);

// Tìm kiếm nâng cao
var advancedRequest = new CourseSearchRequest
{
    Query = "web development",
    CategoryIds = new List<int> { 1, 2 },
    MinPrice = 100,
    MaxPrice = 500,
    MinRating = 4.0,
    Levels = new List<string> { "Beginner", "Intermediate" },
    Languages = new List<string> { "English", "Vietnamese" },
    Tags = new List<string> { "programming", "web" },
    SortFields = new List<SortField>
    {
        new() { Field = "rating", Order = "desc" },
        new() { Field = "price", Order = "asc" }
    }
};
var advancedResults = await _elasticsearchService.SearchCoursesAsync(advancedRequest);
```

### 2. Tìm kiếm đơn hàng
```csharp
var orderRequest = new OrderSearchRequest
{
    Query = "John Doe",
    UserIds = new List<int> { 1, 2, 3 },
    Statuses = new List<string> { "Completed", "Processing" },
    OrderDateFrom = DateTime.Now.AddMonths(-1),
    OrderDateTo = DateTime.Now,
    MinAmount = 100,
    MaxAmount = 1000
};
var orderResults = await _elasticsearchService.SearchOrdersAsync(orderRequest);
```

### 3. Tìm kiếm người dùng
```csharp
var userRequest = new UserSearchRequest
{
    Query = "developer",
    Roles = new List<string> { "Instructor", "Admin" },
    IsActive = true,
    Skills = new List<string> { "JavaScript", "React", "Node.js" },
    Location = "Ho Chi Minh City"
};
var userResults = await _elasticsearchService.SearchUsersAsync(userRequest);
```

### 4. Analytics và Aggregations
```csharp
// Thống kê theo danh mục
var categoryStats = await _elasticsearchService.GetAggregationsAsync("courses", "categoryName");

// Thống kê theo thời gian
var timeStats = await _elasticsearchService.GetDateHistogramAsync("orders", "orderDate", "1d");

// Thống kê theo khoảng giá
var priceRanges = new List<RangeAggregation>
{
    new() { Key = "Under 100", To = 100 },
    new() { Key = "100-500", From = 100, To = 500 },
    new() { Key = "500-1000", From = 500, To = 1000 },
    new() { Key = "Over 1000", From = 1000 }
};
var priceStats = await _elasticsearchService.GetRangeAggregationAsync("courses", "price", priceRanges);
```

## 🔍 API Endpoints

### 1. Tìm kiếm toàn cục
```http
GET /api/search/global?query=react&from=0&size=10
```

### 2. Tìm kiếm khóa học
```http
POST /api/search/courses
Content-Type: application/json

{
  "query": "JavaScript",
  "categoryIds": [1, 2],
  "minPrice": 100,
  "maxPrice": 500,
  "minRating": 4.0,
  "levels": ["Beginner", "Intermediate"],
  "from": 0,
  "size": 10
}
```

### 3. Tìm kiếm đơn hàng
```http
POST /api/search/orders
Content-Type: application/json

{
  "query": "John",
  "userIds": [1, 2],
  "statuses": ["Completed"],
  "orderDateFrom": "2024-01-01",
  "orderDateTo": "2024-12-31",
  "from": 0,
  "size": 10
}
```

### 4. Analytics
```http
POST /api/search/analytics/courses
Content-Type: application/json

{
  "fromDate": "2024-01-01",
  "toDate": "2024-12-31",
  "interval": "1d",
  "metrics": ["count", "sum", "avg"]
}
```

## 📈 Performance Optimization

### 1. Indexing Strategy
```csharp
// Bulk indexing cho hiệu suất cao
var courses = await _courseService.GetAllAsync();
await _elasticsearchIndexingService.IndexCoursesAsync(courses);

// Async indexing để không block main thread
_ = Task.Run(async () =>
{
    await _elasticsearchIndexingService.IndexCourseAsync(course);
});
```

### 2. Query Optimization
```csharp
// Sử dụng filters thay vì queries khi có thể
var request = new SearchRequest
{
    Query = "", // Empty query
    Filters = new Dictionary<string, object>
    {
        ["categoryId"] = 1,
        ["isActive"] = true,
        ["price"] = new { gte = 100, lte = 500 }
    }
};

// Sử dụng pagination
var request = new SearchRequest
{
    Query = "JavaScript",
    From = 0,
    Size = 20 // Reasonable page size
};
```

### 3. Caching Strategy
```csharp
// Cache kết quả tìm kiếm phổ biến
var cacheKey = $"search:courses:{query}:{page}";
var cachedResults = await _cache.GetAsync<SearchResponse<CourseSearchDocument>>(cacheKey);

if (cachedResults == null)
{
    var results = await _elasticsearchService.SearchCoursesAsync(request);
    await _cache.SetAsync(cacheKey, results, TimeSpan.FromMinutes(15));
    return results;
}

return cachedResults;
```

## 🔧 Monitoring và Maintenance

### 1. Health Checks
```csharp
// Kiểm tra tình trạng Elasticsearch
var health = await _elasticsearchService.GetHealthAsync();
if (health.Status != "green")
{
    _logger.LogWarning("Elasticsearch health status: {Status}", health.Status);
}

// Kiểm tra cluster info
var clusterInfo = await _elasticsearchService.GetClusterInfoAsync();
_logger.LogInformation("Cluster: {ClusterName}, Version: {Version}", 
    clusterInfo.ClusterName, clusterInfo.Version);
```

### 2. Index Management
```csharp
// Tạo indices
await _elasticsearchService.CreateIndexAsync<CourseSearchDocument>("courses");

// Refresh index sau khi bulk operations
await _elasticsearchService.RefreshIndexAsync("courses");

// Xóa index cũ
await _elasticsearchService.DeleteIndexAsync("courses_old");
```

### 3. Logging và Debugging
```csharp
// Enable detailed logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Log search queries
_logger.LogDebug("Executing search query: {Query}", JsonSerializer.Serialize(searchRequest));
```

## 🚨 Troubleshooting

### 1. Connection Issues
```bash
# Kiểm tra Elasticsearch status
curl http://localhost:9200/_cluster/health

# Kiểm tra indices
curl http://localhost:9200/_cat/indices?v

# Kiểm tra cluster info
curl http://localhost:9200/
```

### 2. Performance Issues
```bash
# Kiểm tra cluster stats
curl http://localhost:9200/_cluster/stats

# Kiểm tra node stats
curl http://localhost:9200/_nodes/stats

# Kiểm tra index stats
curl http://localhost:9200/courses/_stats
```

### 3. Memory Issues
```bash
# Kiểm tra memory usage
curl http://localhost:9200/_cat/nodes?v&h=name,heap.percent,ram.percent

# Clear cache
curl -X POST http://localhost:9200/_cache/clear
```

## 📊 Kibana Dashboard

### 1. Truy cập Kibana
```
URL: http://localhost:5601
```

### 2. Tạo Index Patterns
```
Management > Stack Management > Index Patterns > Create index pattern
Pattern: courses*, orders*, users*
Time field: createdAt, orderDate, createdAt
```

### 3. Tạo Visualizations
```
Visualize > Create visualization
- Bar chart: Courses by category
- Line chart: Orders over time
- Pie chart: Users by role
- Data table: Top courses by rating
```

### 4. Tạo Dashboard
```
Dashboard > Create dashboard
- Add visualizations
- Set auto-refresh
- Add filters
- Save dashboard
```

## 🎉 Kết luận

Elasticsearch cung cấp:
- ✅ **Tìm kiếm mạnh mẽ** - Full-text search với fuzzy matching
- ✅ **Phân tích dữ liệu** - Aggregations và analytics
- ✅ **Real-time** - Cập nhật dữ liệu real-time
- ✅ **Scalable** - Hỗ trợ hàng triệu documents
- ✅ **Flexible** - Query DSL linh hoạt
- ✅ **Fast** - Sub-second search response
- ✅ **Monitoring** - Kibana dashboard tích hợp

Hệ thống này đảm bảo:
- 🚀 **Fast search** (< 100ms)
- 📊 **Rich analytics**
- 🔍 **Advanced filtering**
- 📈 **Real-time insights**
- 🛠️ **Easy maintenance**
