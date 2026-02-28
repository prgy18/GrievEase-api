using GrievEase.API.Data;
using GrievEase.API.Helpers;
using GrievEase.API.Models.DTOs.Auth;
using GrievEase.API.Models.Entities;
using GrievEase.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrievEase.API.Services.Implementations;

/// <summary>
/// Implementation of authentication service (register, login, logout, password management)
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHelper _passwordHelper;
    private readonly JwtHelper _jwtHelper;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    public AuthService(
        ApplicationDbContext context,
        PasswordHelper passwordHelper,
        JwtHelper jwtHelper,
        ITokenBlacklistService tokenBlacklistService)
    {
        _context = context;
        _passwordHelper = passwordHelper;
        _jwtHelper = jwtHelper;
        _tokenBlacklistService = tokenBlacklistService;
    }

    /// <summary>
    /// Register a new user (Locality Member or Government Official)
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered. Please login or use a different email.");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = registerDto.Name,
            Email = registerDto.Email.ToLower(), // Store emails in lowercase for consistency
            PasswordHash = _passwordHelper.HashPassword(registerDto.Password),
            PhoneNumber = registerDto.PhoneNumber,
            Address = registerDto.Address,
            SignInType = registerDto.SignInType,
            IsActive = true,
            TokenVersion = 0,
            LastLogin = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _jwtHelper.GenerateToken(user);

        // Return auth response
        return new AuthResponseDto
        {
            Token = token,
            User = MapToUserDto(user)
        };
    }

    /// <summary>
    /// Login existing user
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is deactivated. Please contact support.");
        }

        // Verify password
        bool isPasswordValid = _passwordHelper.VerifyPassword(loginDto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Update last login timestamp
        user.LastLogin = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _jwtHelper.GenerateToken(user);

        // Return auth response
        return new AuthResponseDto
        {
            Token = token,
            User = MapToUserDto(user)
        };
    }

    /// <summary>
    /// Logout user by blacklisting their JWT token
    /// </summary>
    public async Task LogoutAsync(string token, Guid userId)
    {
        // Get token expiry time from JWT
        var expiresAt = _jwtHelper.GetTokenExpiry(token);

        // Add token to blacklist
        await _tokenBlacklistService.BlacklistTokenAsync(token, userId, expiresAt);
    }

    /// <summary>
    /// Change user password and invalidate all existing sessions
    /// This increments TokenVersion, making all old JWT tokens invalid
    /// </summary>
    public async Task<string> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
    {
        // Find user
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Verify current password
        bool isCurrentPasswordValid = _passwordHelper.VerifyPassword(
            changePasswordDto.CurrentPassword,
            user.PasswordHash);

        if (!isCurrentPasswordValid)
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        // Update password
        user.PasswordHash = _passwordHelper.HashPassword(changePasswordDto.NewPassword);

        // Increment TokenVersion - this invalidates ALL existing tokens for this user
        user.TokenVersion++;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate new token with updated TokenVersion
        var newToken = _jwtHelper.GenerateToken(user);

        return newToken;
    }

    /// <summary>
    /// Get current authenticated user details
    /// </summary>
    public async Task<UserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return MapToUserDto(user);
    }

    /// <summary>
    /// Validate if user's token version matches stored version
    /// Called by middleware/controllers to check if token is still valid after password change
    /// </summary>
    public async Task<bool> ValidateTokenVersionAsync(Guid userId, int tokenVersion)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return false; // User not found
        }

        // Token is valid only if versions match
        return user.TokenVersion == tokenVersion;
    }

    // Helper method to map User entity to UserDto
    private UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            SignInType = user.SignInType,
            IsActive = user.IsActive,
            LastLogin = user.LastLogin,
            CreatedAt = user.CreatedAt
        };
    }
}