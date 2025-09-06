using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CourseManager.Shared.Services;
using System.Security.Claims;

namespace CourseManager.Shared.Attributes
{
    /// <summary>
    /// Attribute để kiểm tra quyền hạn với cache Redis
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string _action;

        public RequirePermissionAttribute(string resource, string action)
        {
            _resource = resource;
            _action = action;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Kiểm tra authentication
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy user ID từ claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy permission cache service
            var cacheService = context.HttpContext.RequestServices.GetService<IPermissionCacheService>();
            if (cacheService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            try
            {
                // Kiểm tra quyền hạn từ cache
                var hasPermission = await cacheService.UserHasPermissionAsync(userId, _resource, _action);
                if (!hasPermission)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<RequirePermissionAttribute>>();
                logger?.LogError(ex, "Error checking permission {Resource}:{Action} for user {UserId}", 
                    _resource, _action, userId);
                
                context.Result = new StatusCodeResult(500);
                return;
            }
        }
    }

    /// <summary>
    /// Attribute để kiểm tra quyền hạn với multiple permissions (OR logic)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAnyPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _permissions;

        public RequireAnyPermissionAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Kiểm tra authentication
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy user ID từ claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy permission cache service
            var cacheService = context.HttpContext.RequestServices.GetService<IPermissionCacheService>();
            if (cacheService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            try
            {
                // Kiểm tra ít nhất một quyền hạn
                var hasAnyPermission = false;
                foreach (var permission in _permissions)
                {
                    var parts = permission.Split(':');
                    if (parts.Length == 2)
                    {
                        var hasPermission = await cacheService.UserHasPermissionAsync(userId, parts[0], parts[1]);
                        if (hasPermission)
                        {
                            hasAnyPermission = true;
                            break;
                        }
                    }
                }

                if (!hasAnyPermission)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<RequireAnyPermissionAttribute>>();
                logger?.LogError(ex, "Error checking any permission for user {UserId}", userId);
                
                context.Result = new StatusCodeResult(500);
                return;
            }
        }
    }

    /// <summary>
    /// Attribute để kiểm tra quyền hạn với multiple permissions (AND logic)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAllPermissionsAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _permissions;

        public RequireAllPermissionsAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Kiểm tra authentication
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy user ID từ claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy permission cache service
            var cacheService = context.HttpContext.RequestServices.GetService<IPermissionCacheService>();
            if (cacheService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            try
            {
                // Kiểm tra tất cả quyền hạn
                foreach (var permission in _permissions)
                {
                    var parts = permission.Split(':');
                    if (parts.Length == 2)
                    {
                        var hasPermission = await cacheService.UserHasPermissionAsync(userId, parts[0], parts[1]);
                        if (!hasPermission)
                        {
                            context.Result = new ForbidResult();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<RequireAllPermissionsAttribute>>();
                logger?.LogError(ex, "Error checking all permissions for user {UserId}", userId);
                
                context.Result = new StatusCodeResult(500);
                return;
            }
        }
    }

    /// <summary>
    /// Attribute để kiểm tra role với cache
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _role;

        public RequireRoleAttribute(string role)
        {
            _role = role;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Kiểm tra authentication
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy user ID từ claims
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy permission cache service
            var cacheService = context.HttpContext.RequestServices.GetService<IPermissionCacheService>();
            if (cacheService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            try
            {
                // Kiểm tra role từ cache
                var hasRole = await cacheService.UserHasRoleAsync(userId, _role);
                if (!hasRole)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<RequireRoleAttribute>>();
                logger?.LogError(ex, "Error checking role {Role} for user {UserId}", _role, userId);
                
                context.Result = new StatusCodeResult(500);
                return;
            }
        }
    }
}
