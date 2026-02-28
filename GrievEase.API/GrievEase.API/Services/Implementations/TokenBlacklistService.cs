using GrievEase.API.Data;
using GrievEase.API.Models.Entities;
using GrievEase.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrievEase.API.Services.Implementations;

/// <summary>
/// Implementation of token blacklist service for logout functionality
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ApplicationDbContext _context;

    public TokenBlacklistService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Add a JWT token to the blacklist (called during logout)
    /// </summary>
    public async Task BlacklistTokenAsync(string token, Guid userId, DateTime expiresAt)
    {
        // Check if token is already blacklisted (prevent duplicates)
        var existingBlacklist = await _context.TokenBlacklists
            .FirstOrDefaultAsync(tb => tb.Token == token);

        if (existingBlacklist != null)
        {
            return; // Already blacklisted, no need to add again
        }

        // Create new blacklist entry
        var tokenBlacklist = new TokenBlacklist
        {
            Token = token,
            UserId = userId,
            BlacklistedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _context.TokenBlacklists.Add(tokenBlacklist);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Check if a token exists in the blacklist
    /// This is called by middleware/controllers to verify if user has logged out
    /// </summary>
    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return await _context.TokenBlacklists
            .AnyAsync(tb => tb.Token == token && tb.ExpiresAt > DateTime.UtcNow);

        // Note: We only check non-expired tokens
        // Expired tokens are harmless (won't pass JWT validation anyway)
    }

    /// <summary>
    /// Remove expired tokens from blacklist table
    /// This should be called periodically (e.g., daily via background job or scheduled task)
    /// Keeps the TokenBlacklists table small
    /// </summary>
    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.TokenBlacklists
            .Where(tb => tb.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.TokenBlacklists.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}