using CourseManager.Shared.Services;
using System.Security.Claims;

namespace CourseManager.Shared.Middleware
{
    /// <summary>
    /// Middleware để cache quyền hạn và tối ưu hiệu suất
    /// </summary>
    public class PermissionCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionCacheMiddleware> _logger;
        private readonly IPermissionCacheService _cacheService;

        public PermissionCacheMiddleware(
            RequestDelegate next,
            ILogger<PermissionCacheMiddleware> logger,
            IPermissionCacheService cacheService)
        {
            _next = next;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Chỉ xử lý cho các request có authentication
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = GetUserId(context);
                if (userId > 0)
                {
                    // Preload user permissions vào cache nếu chưa có
                    await PreloadUserPermissionsAsync(userId);
                }
            }

            await _next(context);
        }

        private async Task PreloadUserPermissionsAsync(int userId)
        {
            try
            {
                // Kiểm tra xem user permissions đã được cache chưa
                var cachedPermissions = await _cacheService.GetUserPermissionsAsync(userId);
                if (!cachedPermissions.Any())
                {
                    _logger.LogDebug("User {UserId} permissions not in cache, will be loaded on demand", userId);
                }
                else
                {
                    _logger.LogDebug("User {UserId} permissions found in cache ({Count} permissions)", 
                        userId, cachedPermissions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading permissions for user {UserId}", userId);
            }
        }

        private int GetUserId(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }

    /// <summary>
    /// Extension methods cho PermissionCacheMiddleware
    /// </summary>
    public static class PermissionCacheMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermissionCache(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionCacheMiddleware>();
        }
    }
}
