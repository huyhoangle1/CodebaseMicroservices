using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CourseManager.API.Data;
using CourseManager.API.Models;
using CourseManager.API.Models.Auth;

namespace CourseManager.API.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly CourseManagerDbContext _context;
        private readonly ILogger<IdentityService> _logger;
        private readonly IConfiguration _configuration;

        public IdentityService(CourseManagerDbContext context, ILogger<IdentityService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Invalid email or password");
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var tokenResponse = await GenerateTokenResponseAsync(user);
                
                _logger.LogInformation("User {Email} logged in successfully", user.Email);
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email {Email}", request.Email);
                throw;
            }
        }

        public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    throw new InvalidOperationException("User with this email already exists");
                }

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var tokenResponse = await GenerateTokenResponseAsync(user);
                
                _logger.LogInformation("User {Email} registered successfully", user.Email);
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for email {Email}", request.Email);
                throw;
            }
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var refreshToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

                if (refreshToken == null || refreshToken.IsUsed || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException("Invalid refresh token");
                }

                // Mark token as used
                refreshToken.IsUsed = true;
                await _context.SaveChangesAsync();

                var tokenResponse = await GenerateTokenResponseAsync(refreshToken.User);
                
                _logger.LogInformation("Token refreshed successfully for user {Email}", refreshToken.User.Email);
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (token != null)
                {
                    token.IsRevoked = true;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Refresh token revoked successfully");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while revoking refresh token");
                throw;
            }
        }

        public async Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            try
            {
                var tokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("All refresh tokens revoked for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while revoking all tokens for user {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by ID {UserId}", id);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by email {Email}", email);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return false;
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Password reset successfully for user {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for email {Email}", email);
                throw;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, User updatedUser)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Phone = updatedUser.Phone;
                user.Address = updatedUser.Address;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User profile updated successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user profile for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User deactivated successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User activated successfully for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all users");
                throw;
            }
        }

        public async Task<bool> AssignRoleAsync(int userId, string role)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.Role = role;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning role to user {UserId}", userId);
                throw;
            }
        }

        private async Task<TokenResponse> GenerateTokenResponseAsync(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "CourseManagerAPI";
            var audience = jwtSettings["Audience"] ?? "CourseManagerUsers";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token to database
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                JwtId = token.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // Refresh token expires in 7 days
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = token.ValidTo,
                User = new UserInfo
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
