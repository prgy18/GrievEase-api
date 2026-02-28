
using GrievEase.API.Models.DTOs.Auth;

namespace GrievEase.API.Services.Interfaces;

/// <summary>
/// Service for handling user authentication (register, login, logout, password management)
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user (Locality Member or Government Official)
    /// </summary>
    /// <param name="registerDto">Registration details</param>
    /// <returns>Auth response with JWT token and user info</returns>
    /// <exception cref="InvalidOperationException">If email already exists</exception>
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);

    /// <summary>
    /// Login existing user with email and password
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Auth response with JWT token and user info</returns>
    /// <exception cref="UnauthorizedAccessException">If credentials are invalid or account is inactive</exception>
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

    /// <summary>
    /// Logout user by blacklisting their current JWT token
    /// </summary>
    /// <param name="token">JWT token to blacklist</param>
    /// <param name="userId">User ID from the token</param>
    Task LogoutAsync(string token, Guid userId);

    /// <summary>
    /// Change user password and invalidate all existing sessions
    /// This increments TokenVersion, making all old tokens invalid
    /// </summary>
    /// <param name="userId">User ID requesting password change</param>
    /// <param name="changePasswordDto">Current and new password</param>
    /// <returns>New JWT token (user must re-login on all devices)</returns>
    /// <exception cref="UnauthorizedAccessException">If current password is wrong</exception>
    Task<string> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

    /// <summary>
    /// Get current authenticated user's details
    /// </summary>
    /// <param name="userId">User ID from JWT token</param>
    /// <returns>User details</returns>
    /// <exception cref="KeyNotFoundException">If user not found</exception>
    Task<UserDto> GetCurrentUserAsync(Guid userId);

    /// <summary>
    /// Verify if a user's token version matches the stored version
    /// Used to invalidate old tokens after password change
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="tokenVersion">Token version from JWT claim</param>
    /// <returns>True if token is still valid, False if version mismatch</returns>
    Task<bool> ValidateTokenVersionAsync(Guid userId, int tokenVersion);
}