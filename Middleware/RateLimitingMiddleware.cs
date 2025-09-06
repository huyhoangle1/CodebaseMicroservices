using System.Collections.Concurrent;
using System.Net;

namespace CourseManager.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ConcurrentDictionary<string, ClientInfo> _clients = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _maxRequests = configuration.GetValue<int>("RateLimiting:MaxRequests", 100);
            _timeWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimiting:TimeWindowMinutes", 1));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var now = DateTime.UtcNow;

            var clientInfo = _clients.AddOrUpdate(clientId,
                new ClientInfo { RequestCount = 1, FirstRequestTime = now },
                (key, existing) =>
                {
                    if (now - existing.FirstRequestTime > _timeWindow)
                    {
                        return new ClientInfo { RequestCount = 1, FirstRequestTime = now };
                    }
                    return new ClientInfo 
                    { 
                        RequestCount = existing.RequestCount + 1, 
                        FirstRequestTime = existing.FirstRequestTime 
                    };
                });

            if (clientInfo.RequestCount > _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId}. Requests: {RequestCount}", 
                    clientId, clientInfo.RequestCount);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Rate limit exceeded",
                    message = $"Maximum {_maxRequests} requests per {_timeWindow.TotalMinutes} minute(s)",
                    retryAfter = _timeWindow.TotalSeconds
                };

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get client IP from various headers
            var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                          context.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                          context.Connection.RemoteIpAddress?.ToString() ??
                          "unknown";

            return clientIp;
        }

        private class ClientInfo
        {
            public int RequestCount { get; set; }
            public DateTime FirstRequestTime { get; set; }
        }
    }
}

