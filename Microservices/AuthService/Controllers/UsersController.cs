using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;
using CourseManager.Shared.Repositories;
using CourseManager.Shared.Controllers;
using System.Linq.Expressions;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;

        public UsersController(
            IUserRepository userRepository,
            IIdentityService identityService,
            IMapper mapper, 
            ILogger<UsersController> logger) 
            : base(mapper, logger)
        {
            _userRepository = userRepository;
            _identityService = identityService;
        }

        /// <summary>
        /// Lấy tất cả users (chỉ Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            return await GetAllAsync<User, UserDto>(_userRepository, "users.read");
        }

        /// <summary>
        /// Lấy user theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return HandleBadRequestResult($"User with ID {id} not found");
                }

                // Kiểm tra quyền truy cập - chỉ admin hoặc chính user đó
                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && user.Id != currentUserId)
                {
                    return HandleForbiddenResult("You don't have permission to view this user");
                }

                var userDto = _mapper.Map<UserDto>(user);
                return HandleResult(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user with ID {UserId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật user (chỉ Admin hoặc chính user đó)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                // Kiểm tra quyền truy cập
                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && id != currentUserId)
                {
                    return HandleForbiddenResult("You don't have permission to update this user");
                }

                var user = _mapper.Map<User>(request);
                var result = await _identityService.UpdateUserProfileAsync(id, user);

                if (result)
                {
                    var updatedUser = await _userRepository.GetByIdAsync(id);
                    var userDto = _mapper.Map<UserDto>(updatedUser);
                    return HandleResult(userDto);
                }
                else
                {
                    return HandleBadRequestResult($"User with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with ID {UserId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xóa user (chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            return await SoftDeleteAsync<User>(_userRepository, id);
        }

        /// <summary>
        /// Kích hoạt user (chỉ Admin)
        /// </summary>
        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> ActivateUser(int id)
        {
            try
            {
                var result = await _identityService.ActivateUserAsync(id);
                if (result)
                {
                    var user = await _userRepository.GetByIdAsync(id);
                    var userDto = _mapper.Map<UserDto>(user);
                    return HandleResult(userDto);
                }
                else
                {
                    return HandleBadRequestResult($"User with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating user with ID {UserId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Vô hiệu hóa user (chỉ Admin)
        /// </summary>
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> DeactivateUser(int id)
        {
            try
            {
                var result = await _identityService.DeactivateUserAsync(id);
                if (result)
                {
                    var user = await _userRepository.GetByIdAsync(id);
                    var userDto = _mapper.Map<UserDto>(user);
                    return HandleResult(userDto);
                }
                else
                {
                    return HandleBadRequestResult($"User with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating user with ID {UserId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Gán role cho user (chỉ Admin)
        /// </summary>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> AssignRole(int id, [FromBody] AssignRoleRequest request)
        {
            try
            {
                var result = await _identityService.AssignRoleAsync(id, request.Role);
                if (result)
                {
                    var user = await _userRepository.GetByIdAsync(id);
                    var userDto = _mapper.Map<UserDto>(user);
                    return HandleResult(userDto);
                }
                else
                {
                    return HandleBadRequestResult($"User with ID {id} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role to user with ID {UserId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy users với phân trang
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetPagedUsers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? role = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                Expression<Func<User, bool>>? predicate = null;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    predicate = u => u.FirstName.Contains(searchTerm) || 
                                   u.LastName.Contains(searchTerm) || 
                                   u.Email.Contains(searchTerm);
                }

                if (!string.IsNullOrWhiteSpace(role))
                {
                    var rolePredicate = u => u.Role.ToLower() == role.ToLower();
                    predicate = predicate == null ? rolePredicate : 
                               Expression.Lambda<Func<User, bool>>(
                                   Expression.AndAlso(predicate.Body, rolePredicate.Body), 
                                   predicate.Parameters);
                }

                if (isActive.HasValue)
                {
                    var activePredicate = u => u.IsActive == isActive.Value;
                    predicate = predicate == null ? activePredicate : 
                               Expression.Lambda<Func<User, bool>>(
                                   Expression.AndAlso(predicate.Body, activePredicate.Body), 
                                   predicate.Parameters);
                }

                if (predicate != null)
                {
                    return await GetPagedAsync<User, UserDto>(_userRepository, pageNumber, pageSize, predicate);
                }
                else
                {
                    return await GetPagedAsync<User, UserDto>(_userRepository, pageNumber, pageSize);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged users");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy thống kê users
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            try
            {
                var totalUsers = await _userRepository.CountAsync();
                var activeUsers = await _userRepository.CountAsync(u => u.IsActive);
                var adminUsers = await _userRepository.CountAsync(u => u.Role == "Admin");
                var regularUsers = await _userRepository.CountAsync(u => u.Role == "User");

                var stats = new UserStatsDto
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = totalUsers - activeUsers,
                    AdminUsers = adminUsers,
                    RegularUsers = regularUsers
                };

                return HandleResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user stats");
                return HandleInternalServerErrorResult();
            }
        }
    }

    // DTOs for user management
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int RegularUsers { get; set; }
    }
}
