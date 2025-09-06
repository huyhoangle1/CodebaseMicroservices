using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.DTOs;
using CourseManager.Shared.Controllers;
using System.Linq.Expressions;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : BaseController
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(
            IPermissionService permissionService,
            IMapper mapper, 
            ILogger<PermissionsController> logger) 
            : base(mapper, logger)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Lấy tất cả quyền hạn (chỉ Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                return HandleResult(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all permissions");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy quyền hạn theo ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PermissionDto>> GetPermissionById(int id)
        {
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);
                if (permission == null)
                {
                    return HandleBadRequestResult($"Permission with ID {id} not found");
                }

                return HandleResult(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permission with ID {PermissionId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo quyền hạn mới (chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PermissionDto>> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            try
            {
                var permission = await _permissionService.CreatePermissionAsync(request);
                return HandleCreatedResult(permission, nameof(GetPermissionById), new { id = permission.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating permission");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật quyền hạn (chỉ Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PermissionDto>> UpdatePermission(int id, [FromBody] UpdatePermissionRequest request)
        {
            try
            {
                var permission = await _permissionService.UpdatePermissionAsync(id, request);
                return HandleResult(permission);
            }
            catch (KeyNotFoundException)
            {
                return HandleBadRequestResult($"Permission with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating permission with ID {PermissionId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xóa quyền hạn (chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePermission(int id)
        {
            try
            {
                var result = await _permissionService.DeletePermissionAsync(id);
                if (result)
                {
                    return HandleNoContentResult();
                }
                else
                {
                    return HandleBadRequestResult($"Permission with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting permission with ID {PermissionId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Kích hoạt quyền hạn (chỉ Admin)
        /// </summary>
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PermissionDto>> ActivatePermission(int id)
        {
            try
            {
                var result = await _permissionService.ActivatePermissionAsync(id);
                if (result)
                {
                    var permission = await _permissionService.GetPermissionByIdAsync(id);
                    return HandleResult(permission!);
                }
                else
                {
                    return HandleBadRequestResult($"Permission with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating permission with ID {PermissionId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Vô hiệu hóa quyền hạn (chỉ Admin)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PermissionDto>> DeactivatePermission(int id)
        {
            try
            {
                var result = await _permissionService.DeactivatePermissionAsync(id);
                if (result)
                {
                    var permission = await _permissionService.GetPermissionByIdAsync(id);
                    return HandleResult(permission!);
                }
                else
                {
                    return HandleBadRequestResult($"Permission with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating permission with ID {PermissionId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy quyền hạn theo resource
        /// </summary>
        [HttpGet("resource/{resource}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissionsByResource(string resource)
        {
            try
            {
                var permissions = await _permissionService.GetPermissionsByResourceAsync(resource);
                return HandleResult(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions by resource {Resource}", resource);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy quyền hạn theo module
        /// </summary>
        [HttpGet("module/{module}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissionsByModule(string module)
        {
            try
            {
                var permissions = await _permissionService.GetPermissionsByModuleAsync(module);
                return HandleResult(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions by module {Module}", module);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy quyền hạn của user
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetUserPermissions(int userId)
        {
            try
            {
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                return HandleResult(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions for user {UserId}", userId);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy quyền hạn của user hiện tại
        /// </summary>
        [HttpGet("my-permissions")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetMyPermissions()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                var permissions = await _permissionService.GetUserPermissionsAsync(currentUserId);
                return HandleResult(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting permissions for current user");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Gán quyền hạn cho user
        /// </summary>
        [HttpPost("user/{userId}/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignPermissionToUser(int userId, [FromBody] AssignPermissionRequest request)
        {
            try
            {
                var result = await _permissionService.AssignPermissionToUserAsync(userId, request.PermissionId, request.ExpiresAt);
                if (result)
                {
                    return HandleResult(new { message = "Permission assigned successfully" });
                }
                else
                {
                    return HandleBadRequestResult("Failed to assign permission");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning permission to user {UserId}", userId);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Thu hồi quyền hạn từ user
        /// </summary>
        [HttpDelete("user/{userId}/revoke/{permissionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RevokePermissionFromUser(int userId, int permissionId)
        {
            try
            {
                var result = await _permissionService.RevokePermissionFromUserAsync(userId, permissionId);
                if (result)
                {
                    return HandleResult(new { message = "Permission revoked successfully" });
                }
                else
                {
                    return HandleBadRequestResult("Failed to revoke permission");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while revoking permission from user {UserId}", userId);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Kiểm tra quyền hạn của user
        /// </summary>
        [HttpGet("check")]
        [Authorize]
        public async Task<ActionResult<bool>> CheckPermission([FromQuery] string resource, [FromQuery] string action)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                var hasPermission = await _permissionService.CheckPermissionAsync(currentUserId, resource, action);
                return HandleResult(hasPermission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking permission for resource {Resource} and action {Action}", resource, action);
                return HandleInternalServerErrorResult();
            }
        }
    }

    // DTOs for permission requests
    public class AssignPermissionRequest
    {
        public int PermissionId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
