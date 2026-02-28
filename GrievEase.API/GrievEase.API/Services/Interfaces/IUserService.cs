
using GrievEase.API.Models.DTOs.Auth;

namespace GrievEase.API.Services.Interfaces;

/// <summary>
/// Service for managing user profiles and account settings
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User details</returns>
    /// <exception cref="KeyNotFoundException">If user not found</exception>
    Task<UserDto> GetUserProfileAsync(Guid userId);

    /// <summary>
    /// Update user profile (name, phone, address)
    /// Email cannot be changed. Password change is handled by AuthService.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="name">New name (optional)</param>
    /// <param name="phoneNumber">New phone number (optional)</param>
    /// <param name="address">New address (optional)</param>
    /// <returns>Updated user details</returns>
    Task<UserDto> UpdateUserProfileAsync(Guid userId, string? name, string? phoneNumber, string? address);

    /// <summary>
    /// Deactivate user account (soft delete)
    /// User can no longer login but data is preserved
    /// </summary>
    /// <param name="userId">User ID</param>
    Task DeactivateAccountAsync(Guid userId);

    /// <summary>
    /// Reactivate previously deactivated account
    /// </summary>
    /// <param name="userId">User ID</param>
    Task ReactivateAccountAsync(Guid userId);

    /// <summary>
    /// Check if user exists and is active
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if user exists and is active</returns>
    Task<bool> IsUserActiveAsync(Guid userId);
}