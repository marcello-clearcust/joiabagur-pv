using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for RefreshToken entity.
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <inheritdoc/>
    public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > now)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> RevokeAllByUserIdAsync(Guid userId, string? revokedByIp = null)
    {
        var now = DateTime.UtcNow;
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = now;
            token.RevokedByIp = revokedByIp;
        }

        return tokens.Count;
    }

    /// <inheritdoc/>
    public async Task<int> RemoveExpiredTokensAsync()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < now || rt.IsRevoked)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        return expiredTokens.Count;
    }
}
