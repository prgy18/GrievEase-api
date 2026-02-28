using GrievEase.API.Data;
using GrievEase.API.Models.DTOs.Auth;
using GrievEase.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrievEase.API.Services.Implementations;

/// <summary>
/// Implementation of user profile management service
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    public async Task<UserDto> GetUserProfileAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return MapToUserDto(user);
    }

    /// <summary>
    /// Update user profile (name, phone, address)
    /// Email cannot be changed (unique identifier)
    /// Password is changed via AuthService.ChangePasswordAsync
    /// </summary>
    public async Task<UserDto> UpdateUserProfileAsync(
        Guid userId,
        string? name,
        string? phoneNumber,
        string? address)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Update only fields that are provided (not null)
        if (!string.IsNullOrWhiteSpace(name))
        {
            user.Name = name;
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            user.PhoneNumber = phoneNumber;
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            user.Address = address;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToUserDto(user);
    }

    /// <summary>
    /// Deactivate user account (soft delete)
    /// User can no longer login but data is preserved
    /// Grievances and upvotes remain in database
    /// </summary>
    public async Task DeactivateAccountAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Account is already deactivated.");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Reactivate previously deactivated account
    /// </summary>
    public async Task ReactivateAccountAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (user.IsActive)
        {
            throw new InvalidOperationException("Account is already active.");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Check if user exists and is active
    /// Used for authorization checks
    /// </summary>
    public async Task<bool> IsUserActiveAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user != null && user.IsActive;
    }

    // Helper method to map User entity to UserDto
    private UserDto MapToUserDto(Models.Entities.User user)
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