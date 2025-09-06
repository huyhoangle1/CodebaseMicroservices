using CourseManager.API.Models;
using CourseManager.API.Models.Auth;

namespace CourseManager.API.Services
{
    public interface IIdentityService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RegisterAsync(RegisterRequest request);
        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> RevokeAllUserTokensAsync(int userId);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task<bool> UpdateUserProfileAsync(int userId, User user);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> AssignRoleAsync(int userId, string role);
    }
}
