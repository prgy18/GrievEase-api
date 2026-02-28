namespace GrievEase.API.Services.Interfaces;


public interface ITokenBlacklistService
{
    /// <summary>
    /// Add a token to the blacklist (when user logs out)
    /// </summary>
    /// <param name="token">JWT token to blacklist</param>
    /// <param name="userId">User who owns this token</param>
    /// <param name="expiresAt">When this token expires naturally</param>
    Task BlacklistTokenAsync(string token, Guid userId, DateTime expiresAt);

    /// <summary>
    /// Check if a token is blacklisted
    /// </summary>
    /// <param name="token">JWT token to check</param>
    /// <returns>True if token is blacklisted (user logged out), False otherwise</returns>
    Task<bool> IsTokenBlacklistedAsync(string token);

    /// <summary>
    /// Cleanup expired tokens from blacklist table (should run periodically)
    /// This keeps the table small - no need to store expired tokens
    /// </summary>
    Task CleanupExpiredTokensAsync();
}