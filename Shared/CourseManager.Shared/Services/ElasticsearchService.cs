using CourseManager.Shared.DTOs.Notification;
using System.Text.Json;
using System.Text;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Implementation của Elasticsearch Service
    /// </summary>
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ElasticsearchService> _logger;
        private readonly ElasticsearchConfiguration _config;

        public ElasticsearchService(
            HttpClient httpClient,
            ILogger<ElasticsearchService> logger,
            ElasticsearchConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }

        #region Index Management

        public async Task<bool> CreateIndexAsync<T>(string indexName) where T : class
        {
            try
            {
                var mapping = GenerateMapping<T>();
                var settings = GenerateSettings();
                
                var indexConfig = new
                {
                    settings = settings,
                    mappings = mapping
                };

                var json = JsonSerializer.Serialize(indexConfig, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/{indexName}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Created index {IndexName}", indexName);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create index {IndexName}: {Error}", indexName, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating index {IndexName}", indexName);
                return false;
            }
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/{indexName}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Deleted index {IndexName}", indexName);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete index {IndexName}: {Error}", indexName, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting index {IndexName}", indexName);
                return false;
            }
        }

        public async Task<bool> IndexExistsAsync(string indexName)
        {
            try
            {
                var response = await _httpClient.HeadAsync($"/{indexName}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if index {IndexName} exists", indexName);
                return false;
            }
        }

        public async Task<bool> RefreshIndexAsync(string indexName)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/{indexName}/_refresh", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing index {IndexName}", indexName);
                return false;
            }
        }

        public async Task<bool> ReindexAsync<T>(string sourceIndex, string targetIndex) where T : class
        {
            try
            {
                var reindexRequest = new
                {
                    source = new { index = sourceIndex },
                    dest = new { index = targetIndex }
                };

                var json = JsonSerializer.Serialize(reindexRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/_reindex", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reindexing from {SourceIndex} to {TargetIndex}", sourceIndex, targetIndex);
                return false;
            }
        }

        #endregion

        #region Document Operations

        public async Task<string> IndexDocumentAsync<T>(string indexName, T document, string? id = null) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(document, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = string.IsNullOrEmpty(id) ? $"/{indexName}/_doc" : $"/{indexName}/_doc/{id}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<IndexResponse>(responseContent);
                    return result?.Id ?? string.Empty;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to index document in {IndexName}: {Error}", indexName, errorContent);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing document in {IndexName}", indexName);
                return string.Empty;
            }
        }

        public async Task<bool> IndexDocumentAsync<T>(string indexName, T document, string id) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(document, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/{indexName}/_doc/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Indexed document {Id} in {IndexName}", id, indexName);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to index document {Id} in {IndexName}: {Error}", id, indexName, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing document {Id} in {IndexName}", id, indexName);
                return false;
            }
        }

        public async Task<bool> UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class
        {
            try
            {
                var updateRequest = new
                {
                    doc = document
                };

                var json = JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/{indexName}/_update/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Updated document {Id} in {IndexName}", id, indexName);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update document {Id} in {IndexName}: {Error}", id, indexName, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document {Id} in {IndexName}", id, indexName);
                return false;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string indexName, string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/{indexName}/_doc/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Deleted document {Id} from {IndexName}", id, indexName);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete document {Id} from {IndexName}: {Error}", id, indexName, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {Id} from {IndexName}", id, indexName);
                return false;
            }
        }

        public async Task<T?> GetDocumentAsync<T>(string indexName, string id) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{indexName}/_doc/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GetDocumentResponse<T>>(content);
                    return result?.Source;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {Id} from {IndexName}", id, indexName);
                return null;
            }
        }

        public async Task<bool> DocumentExistsAsync(string indexName, string id)
        {
            try
            {
                var response = await _httpClient.HeadAsync($"/{indexName}/_doc/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if document {Id} exists in {IndexName}", id, indexName);
                return false;
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<BulkResponse> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents) where T : class
        {
            try
            {
                var bulkBody = new StringBuilder();
                
                foreach (var doc in documents)
                {
                    var indexAction = new
                    {
                        index = new { _index = indexName }
                    };
                    
                    bulkBody.AppendLine(JsonSerializer.Serialize(indexAction));
                    bulkBody.AppendLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                }

                var content = new StringContent(bulkBody.ToString(), Encoding.UTF8, "application/x-ndjson");
                var response = await _httpClient.PostAsync("/_bulk", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<BulkResponse>(responseContent);
                    return result ?? new BulkResponse { IsValid = false };
                }

                return new BulkResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing documents in {IndexName}", indexName);
                return new BulkResponse { IsValid = false };
            }
        }

        public async Task<BulkResponse> BulkUpdateAsync<T>(string indexName, IEnumerable<BulkUpdateItem<T>> updates) where T : class
        {
            try
            {
                var bulkBody = new StringBuilder();
                
                foreach (var update in updates)
                {
                    var updateAction = new
                    {
                        update = new { _index = indexName, _id = update.Id }
                    };
                    
                    var updateDoc = new
                    {
                        doc = update.Document
                    };
                    
                    bulkBody.AppendLine(JsonSerializer.Serialize(updateAction));
                    bulkBody.AppendLine(JsonSerializer.Serialize(updateDoc, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                }

                var content = new StringContent(bulkBody.ToString(), Encoding.UTF8, "application/x-ndjson");
                var response = await _httpClient.PostAsync("/_bulk", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<BulkResponse>(responseContent);
                    return result ?? new BulkResponse { IsValid = false };
                }

                return new BulkResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating documents in {IndexName}", indexName);
                return new BulkResponse { IsValid = false };
            }
        }

        public async Task<BulkResponse> BulkDeleteAsync(string indexName, IEnumerable<string> ids)
        {
            try
            {
                var bulkBody = new StringBuilder();
                
                foreach (var id in ids)
                {
                    var deleteAction = new
                    {
                        delete = new { _index = indexName, _id = id }
                    };
                    
                    bulkBody.AppendLine(JsonSerializer.Serialize(deleteAction));
                }

                var content = new StringContent(bulkBody.ToString(), Encoding.UTF8, "application/x-ndjson");
                var response = await _httpClient.PostAsync("/_bulk", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<BulkResponse>(responseContent);
                    return result ?? new BulkResponse { IsValid = false };
                }

                return new BulkResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting documents from {IndexName}", indexName);
                return new BulkResponse { IsValid = false };
            }
        }

        #endregion

        #region Search Operations

        public async Task<SearchResponse<T>> SearchAsync<T>(string indexName, SearchRequest request) where T : class
        {
            try
            {
                var searchQuery = BuildSearchQuery(request);
                var json = JsonSerializer.Serialize(searchQuery, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/{indexName}/_search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return ParseSearchResponse<T>(responseContent);
                }

                return new SearchResponse<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching in {IndexName}", indexName);
                return new SearchResponse<T>();
            }
        }

        public async Task<SearchResponse<T>> SearchAsync<T>(string indexName, string query, int from = 0, int size = 10) where T : class
        {
            var request = new SearchRequest
            {
                Query = query,
                From = from,
                Size = size
            };

            return await SearchAsync<T>(indexName, request);
        }

        public async Task<SearchResponse<T>> MultiSearchAsync<T>(string indexName, IEnumerable<SearchRequest> requests) where T : class
        {
            try
            {
                var multiSearchBody = new StringBuilder();
                
                foreach (var request in requests)
                {
                    var header = new { index = indexName };
                    var searchQuery = BuildSearchQuery(request);
                    
                    multiSearchBody.AppendLine(JsonSerializer.Serialize(header));
                    multiSearchBody.AppendLine(JsonSerializer.Serialize(searchQuery, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                }

                var content = new StringContent(multiSearchBody.ToString(), Encoding.UTF8, "application/x-ndjson");
                var response = await _httpClient.PostAsync("/_msearch", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return ParseMultiSearchResponse<T>(responseContent);
                }

                return new SearchResponse<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error multi searching in {IndexName}", indexName);
                return new SearchResponse<T>();
            }
        }

        public async Task<SearchResponse<T>> SearchWithFiltersAsync<T>(string indexName, SearchRequest request, Dictionary<string, object> filters) where T : class
        {
            // Merge filters into request
            foreach (var filter in filters)
            {
                request.Filters[filter.Key] = filter.Value;
            }

            return await SearchAsync<T>(indexName, request);
        }

        #endregion

        #region Course Search

        public async Task<SearchResponse<CourseSearchDocument>> SearchCoursesAsync(CourseSearchRequest request)
        {
            var searchRequest = new SearchRequest
            {
                Query = request.Query,
                From = request.From,
                Size = request.Size,
                SortFields = request.SortFields
            };

            // Add filters
            if (request.CategoryIds.Any())
                searchRequest.Filters["categoryId"] = request.CategoryIds;
            
            if (request.InstructorIds.Any())
                searchRequest.Filters["instructorId"] = request.InstructorIds;
            
            if (request.MinPrice.HasValue)
                searchRequest.Filters["price"] = new { gte = request.MinPrice.Value };
            
            if (request.MaxPrice.HasValue)
                searchRequest.Filters["price"] = new { lte = request.MaxPrice.Value };
            
            if (request.MinRating.HasValue)
                searchRequest.Filters["rating"] = new { gte = request.MinRating.Value };
            
            if (request.Levels.Any())
                searchRequest.Filters["level"] = request.Levels;
            
            if (request.Languages.Any())
                searchRequest.Filters["language"] = request.Languages;
            
            if (request.Tags.Any())
                searchRequest.Filters["tags"] = request.Tags;
            
            if (request.IsActive.HasValue)
                searchRequest.Filters["isActive"] = request.IsActive.Value;

            return await SearchAsync<CourseSearchDocument>("courses", searchRequest);
        }

        public async Task<SearchResponse<CourseSearchDocument>> SearchCoursesByCategoryAsync(int categoryId, int from = 0, int size = 10)
        {
            var request = new CourseSearchRequest
            {
                CategoryIds = new List<int> { categoryId },
                From = from,
                Size = size
            };

            return await SearchCoursesAsync(request);
        }

        public async Task<SearchResponse<CourseSearchDocument>> SearchCoursesByInstructorAsync(int instructorId, int from = 0, int size = 10)
        {
            var request = new CourseSearchRequest
            {
                InstructorIds = new List<int> { instructorId },
                From = from,
                Size = size
            };

            return await SearchCoursesAsync(request);
        }

        public async Task<SearchResponse<CourseSearchDocument>> SearchCoursesByPriceRangeAsync(decimal minPrice, decimal maxPrice, int from = 0, int size = 10)
        {
            var request = new CourseSearchRequest
            {
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                From = from,
                Size = size
            };

            return await SearchCoursesAsync(request);
        }

        #endregion

        #region Order Search

        public async Task<SearchResponse<OrderSearchDocument>> SearchOrdersAsync(OrderSearchRequest request)
        {
            var searchRequest = new SearchRequest
            {
                Query = request.Query,
                From = request.From,
                Size = request.Size,
                SortFields = request.SortFields
            };

            // Add filters
            if (request.UserIds.Any())
                searchRequest.Filters["userId"] = request.UserIds;
            
            if (request.Statuses.Any())
                searchRequest.Filters["status"] = request.Statuses;
            
            if (request.PaymentMethods.Any())
                searchRequest.Filters["paymentMethod"] = request.PaymentMethods;
            
            if (request.PaymentStatuses.Any())
                searchRequest.Filters["paymentStatus"] = request.PaymentStatuses;
            
            if (request.OrderDateFrom.HasValue)
                searchRequest.Filters["orderDate"] = new { gte = request.OrderDateFrom.Value };
            
            if (request.OrderDateTo.HasValue)
                searchRequest.Filters["orderDate"] = new { lte = request.OrderDateTo.Value };
            
            if (request.MinAmount.HasValue)
                searchRequest.Filters["totalAmount"] = new { gte = request.MinAmount.Value };
            
            if (request.MaxAmount.HasValue)
                searchRequest.Filters["totalAmount"] = new { lte = request.MaxAmount.Value };

            return await SearchAsync<OrderSearchDocument>("orders", searchRequest);
        }

        public async Task<SearchResponse<OrderSearchDocument>> SearchOrdersByUserAsync(int userId, int from = 0, int size = 10)
        {
            var request = new OrderSearchRequest
            {
                UserIds = new List<int> { userId },
                From = from,
                Size = size
            };

            return await SearchOrdersAsync(request);
        }

        public async Task<SearchResponse<OrderSearchDocument>> SearchOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int from = 0, int size = 10)
        {
            var request = new OrderSearchRequest
            {
                OrderDateFrom = startDate,
                OrderDateTo = endDate,
                From = from,
                Size = size
            };

            return await SearchOrdersAsync(request);
        }

        #endregion

        #region User Search

        public async Task<SearchResponse<UserSearchDocument>> SearchUsersAsync(UserSearchRequest request)
        {
            var searchRequest = new SearchRequest
            {
                Query = request.Query,
                From = request.From,
                Size = request.Size,
                SortFields = request.SortFields
            };

            // Add filters
            if (request.Roles.Any())
                searchRequest.Filters["roles"] = request.Roles;
            
            if (request.IsActive.HasValue)
                searchRequest.Filters["isActive"] = request.IsActive.Value;
            
            if (request.CreatedAfter.HasValue)
                searchRequest.Filters["createdAt"] = new { gte = request.CreatedAfter.Value };
            
            if (request.CreatedBefore.HasValue)
                searchRequest.Filters["createdAt"] = new { lte = request.CreatedBefore.Value };
            
            if (request.Skills.Any())
                searchRequest.Filters["skills"] = request.Skills;
            
            if (!string.IsNullOrEmpty(request.Location))
                searchRequest.Filters["location"] = request.Location;

            return await SearchAsync<UserSearchDocument>("users", searchRequest);
        }

        public async Task<SearchResponse<UserSearchDocument>> SearchUsersByRoleAsync(string role, int from = 0, int size = 10)
        {
            var request = new UserSearchRequest
            {
                Roles = new List<string> { role },
                From = from,
                Size = size
            };

            return await SearchUsersAsync(request);
        }

        #endregion

        #region Analytics

        public async Task<AnalyticsResponse> GetCourseAnalyticsAsync(AnalyticsRequest request)
        {
            // Implementation for course analytics
            return new AnalyticsResponse();
        }

        public async Task<AnalyticsResponse> GetOrderAnalyticsAsync(AnalyticsRequest request)
        {
            // Implementation for order analytics
            return new AnalyticsResponse();
        }

        public async Task<AnalyticsResponse> GetUserAnalyticsAsync(AnalyticsRequest request)
        {
            // Implementation for user analytics
            return new AnalyticsResponse();
        }

        #endregion

        #region Health & Monitoring

        public async Task<ElasticsearchHealth> GetHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/_cluster/health");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ElasticsearchHealth>(content) ?? new ElasticsearchHealth();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Elasticsearch health");
            }

            return new ElasticsearchHealth();
        }

        public async Task<ClusterInfo> GetClusterInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ClusterInfo>(content) ?? new ClusterInfo();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cluster info");
            }

            return new ClusterInfo();
        }

        public async Task<IndexStats> GetIndexStatsAsync(string indexName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/{indexName}/_stats");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                    // Parse index stats from result
                    return new IndexStats { IndexName = indexName };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting index stats for {IndexName}", indexName);
            }

            return new IndexStats { IndexName = indexName };
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var health = await GetHealthAsync();
                return health.Status == "green" || health.Status == "yellow";
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Configuration

        public async Task<bool> UpdateMappingAsync<T>(string indexName) where T : class
        {
            try
            {
                var mapping = GenerateMapping<T>();
                var json = JsonSerializer.Serialize(mapping, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/{indexName}/_mapping", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mapping for {IndexName}", indexName);
                return false;
            }
        }

        public async Task<bool> UpdateSettingsAsync(string indexName, Dictionary<string, object> settings)
        {
            try
            {
                var settingsObj = new { settings };
                var json = JsonSerializer.Serialize(settingsObj, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/{indexName}/_settings", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings for {IndexName}", indexName);
                return false;
            }
        }

        #endregion

        #region Aggregations

        public async Task<AggregationResponse> GetAggregationsAsync(string indexName, string field, string aggregationType = "terms")
        {
            try
            {
                var aggregation = new Dictionary<string, object>
                {
                    ["aggs"] = new Dictionary<string, object>
                    {
                        ["field_agg"] = new Dictionary<string, object>
                        {
                            [aggregationType] = new Dictionary<string, object>
                            {
                                ["field"] = field
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(aggregation, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/{indexName}/_search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return ParseAggregationResponse(responseContent);
                }

                return new AggregationResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregations for {Field} in {IndexName}", field, indexName);
                return new AggregationResponse();
            }
        }

        public async Task<AggregationResponse> GetDateHistogramAsync(string indexName, string field, string interval = "1d")
        {
            try
            {
                var aggregation = new Dictionary<string, object>
                {
                    ["aggs"] = new Dictionary<string, object>
                    {
                        ["date_histogram"] = new Dictionary<string, object>
                        {
                            ["date_histogram"] = new Dictionary<string, object>
                            {
                                ["field"] = field,
                                ["calendar_interval"] = interval
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(aggregation, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/{indexName}/_search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return ParseAggregationResponse(responseContent);
                }

                return new AggregationResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting date histogram for {Field} in {IndexName}", field, indexName);
                return new AggregationResponse();
            }
        }

        public async Task<AggregationResponse> GetRangeAggregationAsync(string indexName, string field, List<RangeAggregation> ranges)
        {
            try
            {
                var rangeAggregation = new Dictionary<string, object>
                {
                    ["ranges"] = ranges.Select(r => new Dictionary<string, object>
                    {
                        ["key"] = r.Key,
                        ["from"] = r.From ?? 0,
                        ["to"] = r.To ?? 0
                    }).ToArray()
                };

                var aggregation = new Dictionary<string, object>
                {
                    ["aggs"] = new Dictionary<string, object>
                    {
                        ["range_agg"] = new Dictionary<string, object>
                        {
                            ["range"] = new Dictionary<string, object>
                            {
                                ["field"] = field,
                                ["ranges"] = rangeAggregation["ranges"]
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(aggregation, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/{indexName}/_search", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return ParseAggregationResponse(responseContent);
                }

                return new AggregationResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting range aggregation for {Field} in {IndexName}", field, indexName);
                return new AggregationResponse();
            }
        }

        #endregion

        #region Private Methods

        private Dictionary<string, object> GenerateMapping<T>() where T : class
        {
            var properties = new Dictionary<string, object>();
            var type = typeof(T);

            foreach (var prop in type.GetProperties())
            {
                var fieldType = GetElasticsearchFieldType(prop.PropertyType);
                properties[ToCamelCase(prop.Name)] = new Dictionary<string, object>
                {
                    ["type"] = fieldType
                };
            }

            return new Dictionary<string, object>
            {
                ["properties"] = properties
            };
        }

        private Dictionary<string, object> GenerateSettings()
        {
            return new Dictionary<string, object>
            {
                ["number_of_shards"] = _config.NumberOfShards,
                ["number_of_replicas"] = _config.NumberOfReplicas,
                ["analysis"] = new Dictionary<string, object>
                {
                    ["analyzer"] = new Dictionary<string, object>
                    {
                        ["custom_analyzer"] = new Dictionary<string, object>
                        {
                            ["type"] = "custom",
                            ["tokenizer"] = "standard",
                            ["filter"] = new[] { "lowercase", "stop" }
                        }
                    }
                }
            };
        }

        private string GetElasticsearchFieldType(Type type)
        {
            return type switch
            {
                var t when t == typeof(string) => "text",
                var t when t == typeof(int) || t == typeof(int?) => "integer",
                var t when t == typeof(long) || t == typeof(long?) => "long",
                var t when t == typeof(double) || t == typeof(double?) => "double",
                var t when t == typeof(decimal) || t == typeof(decimal?) => "double",
                var t when t == typeof(bool) || t == typeof(bool?) => "boolean",
                var t when t == typeof(DateTime) || t == typeof(DateTime?) => "date",
                var t when t == typeof(List<string>) => "keyword",
                _ => "text"
            };
        }

        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
                return input;

            return char.ToLowerInvariant(input[0]) + input[1..];
        }

        private Dictionary<string, object> BuildSearchQuery(SearchRequest request)
        {
            var query = new Dictionary<string, object>();

            // Build query
            if (!string.IsNullOrEmpty(request.Query))
            {
                query["query"] = new Dictionary<string, object>
                {
                    ["multi_match"] = new Dictionary<string, object>
                    {
                        ["query"] = request.Query,
                        ["fields"] = new[] { "*" },
                        ["type"] = "best_fields"
                    }
                };
            }
            else
            {
                query["query"] = new Dictionary<string, object>
                {
                    ["match_all"] = new Dictionary<string, object>()
                };
            }

            // Add filters
            if (request.Filters.Any())
            {
                var mustClauses = new List<object>();
                
                foreach (var filter in request.Filters)
                {
                    if (filter.Value is List<object> listValue)
                    {
                        mustClauses.Add(new Dictionary<string, object>
                        {
                            ["terms"] = new Dictionary<string, object>
                            {
                                [filter.Key] = listValue
                            }
                        });
                    }
                    else if (filter.Value is Dictionary<string, object> rangeValue)
                    {
                        mustClauses.Add(new Dictionary<string, object>
                        {
                            ["range"] = new Dictionary<string, object>
                            {
                                [filter.Key] = rangeValue
                            }
                        });
                    }
                    else
                    {
                        mustClauses.Add(new Dictionary<string, object>
                        {
                            ["term"] = new Dictionary<string, object>
                            {
                                [filter.Key] = filter.Value
                            }
                        });
                    }
                }

                query["query"] = new Dictionary<string, object>
                {
                    ["bool"] = new Dictionary<string, object>
                    {
                        ["must"] = new[] { query["query"] },
                        ["filter"] = mustClauses
                    }
                };
            }

            // Add pagination
            query["from"] = request.From;
            query["size"] = request.Size;

            // Add sorting
            if (request.SortFields.Any())
            {
                query["sort"] = request.SortFields.Select(sf => new Dictionary<string, object>
                {
                    [sf.Field] = new Dictionary<string, object>
                    {
                        ["order"] = sf.Order
                    }
                }).ToArray();
            }

            return query;
        }

        private SearchResponse<T> ParseSearchResponse<T>(string responseContent) where T : class
        {
            try
            {
                var result = JsonSerializer.Deserialize<ElasticsearchSearchResponse<T>>(responseContent);
                if (result?.Hits?.Hits != null)
                {
                    return new SearchResponse<T>
                    {
                        Documents = result.Hits.Hits.Select(h => h.Source).ToList(),
                        Total = result.Hits.Total?.Value ?? 0,
                        Took = result.Took,
                        TimedOut = result.TimedOut,
                        MaxScore = result.Hits.MaxScore
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing search response");
            }

            return new SearchResponse<T>();
        }

        private SearchResponse<T> ParseMultiSearchResponse<T>(string responseContent) where T : class
        {
            // Implementation for parsing multi-search response
            return new SearchResponse<T>();
        }

        private AggregationResponse ParseAggregationResponse(string responseContent)
        {
            try
            {
                var result = JsonSerializer.Deserialize<ElasticsearchAggregationResponse>(responseContent);
                return new AggregationResponse
                {
                    Aggregations = result?.Aggregations ?? new Dictionary<string, object>(),
                    Took = result?.Took ?? 0,
                    TimedOut = result?.TimedOut ?? false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing aggregation response");
                return new AggregationResponse();
            }
        }

        #endregion
    }

    #region Helper Classes

    public class IndexResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Index { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Result { get; set; } = string.Empty;
    }

    public class GetDocumentResponse<T>
    {
        public string Index { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public int Version { get; set; }
        public T? Source { get; set; }
    }

    public class ElasticsearchSearchResponse<T>
    {
        public ElasticsearchHits<T> Hits { get; set; } = new();
        public long Took { get; set; }
        public bool TimedOut { get; set; }
    }

    public class ElasticsearchHits<T>
    {
        public ElasticsearchTotal? Total { get; set; }
        public double MaxScore { get; set; }
        public List<ElasticsearchHit<T>> Hits { get; set; } = new();
    }

    public class ElasticsearchTotal
    {
        public long Value { get; set; }
        public string Relation { get; set; } = string.Empty;
    }

    public class ElasticsearchHit<T>
    {
        public string Index { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public double Score { get; set; }
        public T? Source { get; set; }
    }

    public class ElasticsearchAggregationResponse
    {
        public Dictionary<string, object> Aggregations { get; set; } = new();
        public long Took { get; set; }
        public bool TimedOut { get; set; }
    }

    #endregion
}
