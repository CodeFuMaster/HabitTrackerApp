using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services
{
    public class AuthUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public DateTime? LastLoginDate { get; set; }
        public string AuthToken { get; set; }
        public DateTime TokenExpiration { get; set; }
    }

    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string email, string password);
        Task<bool> LogoutAsync();
        Task<bool> RefreshTokenAsync();
        
        AuthUser CurrentUser { get; }
        bool IsAuthenticated { get; }
        bool HasRole(string role);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IErrorHandlingService _errorHandlingService;
        private AuthUser _currentUser;

        public AuthUser CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null && 
            !string.IsNullOrEmpty(_currentUser.AuthToken) && 
            _currentUser.TokenExpiration > DateTime.UtcNow;

        public AuthenticationService(
            ILogger<AuthenticationService> logger,
            IErrorHandlingService errorHandlingService)
        {
            _logger = logger;
            _errorHandlingService = errorHandlingService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("User attempting to login: {Username}", username);

                // In a real implementation, this would call an API endpoint
                // For now, we'll simulate authentication with a mock user
                if (username == "demo" && password == "password")
                {
                    _currentUser = new AuthUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        Username = username,
                        Email = "demo@example.com",
                        DisplayName = "Demo User",
                        Roles = new[] { "User" },
                        LastLoginDate = DateTime.UtcNow,
                        AuthToken = Guid.NewGuid().ToString(),
                        TokenExpiration = DateTime.UtcNow.AddDays(1)
                    };

                    // In a real implementation, save the token securely
                    await SecureStorage.Default.SetAsync("auth_token", _currentUser.AuthToken);
                    await SecureStorage.Default.SetAsync("token_expiration", _currentUser.TokenExpiration.ToString("o"));
                    await SecureStorage.Default.SetAsync("user_id", _currentUser.Id);

                    _logger.LogInformation("User successfully logged in: {Username}", username);
                    return true;
                }

                _logger.LogWarning("Failed login attempt for username: {Username}", username);
                return false;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.HandleExceptionAsync(ex, "Login");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            try
            {
                _logger.LogInformation("User attempting to register: {Username}, {Email}", username, email);

                // In a real implementation, this would call an API endpoint
                // For now, we'll simulate successful registration
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    // Normally we'd send this data to an API
                    _logger.LogInformation("User registered successfully: {Username}", username);
                    return true;
                }

                _logger.LogWarning("Failed registration attempt: {Username}, {Email}", username, email);
                return false;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.HandleExceptionAsync(ex, "Registration");
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _logger.LogInformation("User logging out: {Username}", _currentUser?.Username);

                // Clear local authentication data
                _currentUser = null;
                SecureStorage.Default.Remove("auth_token");
                SecureStorage.Default.Remove("token_expiration");
                SecureStorage.Default.Remove("user_id");

                return true;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.HandleExceptionAsync(ex, "Logout");
                return false;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                // Check if we have a stored token
                string storedToken = await SecureStorage.Default.GetAsync("auth_token");
                if (string.IsNullOrEmpty(storedToken))
                {
                    return false;
                }

                string storedExpiration = await SecureStorage.Default.GetAsync("token_expiration");
                if (string.IsNullOrEmpty(storedExpiration) || 
                    !DateTime.TryParse(storedExpiration, out DateTime expiration) ||
                    expiration <= DateTime.UtcNow)
                {
                    // Token expired
                    return false;
                }

                // In a real implementation, validate the token with the server
                // For now, we'll just rehydrate the user from stored data
                string userId = await SecureStorage.Default.GetAsync("user_id");

                _currentUser = new AuthUser
                {
                    Id = userId,
                    Username = "demo", // Would normally fetch this from the server
                    Email = "demo@example.com",
                    DisplayName = "Demo User",
                    Roles = new[] { "User" },
                    LastLoginDate = DateTime.UtcNow,
                    AuthToken = storedToken,
                    TokenExpiration = expiration
                };

                _logger.LogInformation("User token refreshed: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.HandleExceptionAsync(ex, "RefreshToken");
                return false;
            }
        }

        public bool HasRole(string role)
        {
            if (!IsAuthenticated || _currentUser?.Roles == null)
            {
                return false;
            }

            return Array.Exists(_currentUser.Roles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }
    }
}