using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;
using CourseManager.Shared.Repositories;
using CourseManager.Shared.Controllers;
using CourseManager.Shared.Models.Auth;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IIdentityService _identityService;

        public AuthController(
            IIdentityService identityService,
            IMapper mapper, 
            ILogger<AuthController> logger) 
            : base(mapper, logger)
        {
            _identityService = identityService;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var tokenResponse = await _identityService.LoginAsync(request);
                return HandleResult(tokenResponse);
            }
            catch (UnauthorizedAccessException)
            {
                return HandleUnauthorizedResult("Invalid email or password");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email {Email}", request.Email);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Đăng ký
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var tokenResponse = await _identityService.RegisterAsync(request);
                return HandleResult(tokenResponse);
            }
            catch (InvalidOperationException ex)
            {
                return HandleBadRequestResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for email {Email}", request.Email);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Refresh token
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var tokenResponse = await _identityService.RefreshTokenAsync(request);
                return HandleResult(tokenResponse);
            }
            catch (UnauthorizedAccessException)
            {
                return HandleUnauthorizedResult("Invalid refresh token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Đăng xuất (revoke token)
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        public async Task<ActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            try
            {
                var result = await _identityService.RevokeTokenAsync(request.RefreshToken);
                if (result)
                {
                    return HandleResult(new { message = "Token revoked successfully" });
                }
                else
                {
                    return HandleBadRequestResult("Token not found or already revoked");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token revocation");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Đăng xuất tất cả (revoke all user tokens)
        /// </summary>
        [HttpPost("revoke-all")]
        [Authorize]
        public async Task<ActionResult> RevokeAllTokens()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                var result = await _identityService.RevokeAllUserTokensAsync(currentUserId);
                if (result)
                {
                    return HandleResult(new { message = "All tokens revoked successfully" });
                }
                else
                {
                    return HandleBadRequestResult("Failed to revoke tokens");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during revoking all tokens for user {UserId}", GetCurrentUserId());
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserInfo>> GetCurrentUser()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                var user = await _identityService.GetUserByIdAsync(currentUserId);
                if (user == null)
                {
                    return HandleBadRequestResult("User not found");
                }

                var userInfo = _mapper.Map<UserInfo>(user);
                return HandleResult(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                var result = await _identityService.ChangePasswordAsync(
                    currentUserId, 
                    request.CurrentPassword, 
                    request.NewPassword);

                if (result)
                {
                    return HandleResult(new { message = "Password changed successfully" });
                }
                else
                {
                    return HandleBadRequestResult("Failed to change password");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return HandleBadRequestResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user {UserId}", GetCurrentUserId());
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Reset mật khẩu (chỉ Admin)
        /// </summary>
        [HttpPost("reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _identityService.ResetPasswordAsync(
                    request.Email, 
                    request.NewPassword);

                if (result)
                {
                    return HandleResult(new { message = "Password reset successfully" });
                }
                else
                {
                    return HandleBadRequestResult("User not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for email {Email}", request.Email);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserInfo>> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                var user = _mapper.Map<User>(request);
                var result = await _identityService.UpdateUserProfileAsync(currentUserId, user);

                if (result)
                {
                    var updatedUser = await _identityService.GetUserByIdAsync(currentUserId);
                    var userInfo = _mapper.Map<UserInfo>(updatedUser);
                    return HandleResult(userInfo);
                }
                else
                {
                    return HandleBadRequestResult("Failed to update profile");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile for user {UserId}", GetCurrentUserId());
                return HandleInternalServerErrorResult();
            }
        }
    }

    // DTOs for auth requests
    public class RevokeTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
