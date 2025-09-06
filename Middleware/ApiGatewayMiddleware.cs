using System.Text.Json;
using Serilog;

namespace CourseManager.API.Middleware
{
    public class ApiGatewayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiGatewayMiddleware> _logger;
        private readonly HttpClient _httpClient;

        public ApiGatewayMiddleware(RequestDelegate next, ILogger<ApiGatewayMiddleware> logger, HttpClient httpClient)
        {
            _next = next;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            // Route to appropriate microservice
            if (path.StartsWith("/api/courses") || path.StartsWith("/api/categories"))
            {
                await RouteToService(context, "http://localhost:5001", "CourseService");
            }
            else if (path.StartsWith("/api/orders") || path.StartsWith("/api/payments"))
            {
                await RouteToService(context, "http://localhost:5002", "OrderService");
            }
            else if (path.StartsWith("/api/auth") || path.StartsWith("/api/users"))
            {
                await RouteToService(context, "http://localhost:5003", "AuthService");
            }
            else
            {
                await _next(context);
            }
        }

        private async Task RouteToService(HttpContext context, string serviceUrl, string serviceName)
        {
            try
            {
                _logger.LogInformation("Routing request to {ServiceName} at {ServiceUrl}", serviceName, serviceUrl);

                var requestMessage = new HttpRequestMessage();
                
                // Copy method
                requestMessage.Method = new HttpMethod(context.Request.Method);
                
                // Copy URL
                var targetUrl = $"{serviceUrl}{context.Request.Path}{context.Request.QueryString}";
                requestMessage.RequestUri = new Uri(targetUrl);
                
                // Copy headers
                foreach (var header in context.Request.Headers)
                {
                    if (!header.Key.StartsWith(":"))
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
                
                // Copy body for POST, PUT, PATCH
                if (context.Request.Method != "GET" && context.Request.Method != "DELETE")
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                    
                    if (!string.IsNullOrEmpty(body))
                    {
                        requestMessage.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    }
                }

                // Send request to microservice
                var response = await _httpClient.SendAsync(requestMessage);
                
                // Copy response status
                context.Response.StatusCode = (int)response.StatusCode;
                
                // Copy response headers
                foreach (var header in response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
                
                foreach (var header in response.Content.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
                
                // Copy response body
                var responseBody = await response.Content.ReadAsStringAsync();
                await context.Response.WriteAsync(responseBody);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to route request to {ServiceName}", serviceName);
                
                context.Response.StatusCode = 503;
                context.Response.ContentType = "application/json";
                
                var errorResponse = new
                {
                    error = "Service Unavailable",
                    message = $"The {serviceName} is currently unavailable",
                    timestamp = DateTime.UtcNow
                };
                
                var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}

