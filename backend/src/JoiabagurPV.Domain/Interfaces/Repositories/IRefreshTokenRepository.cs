using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for RefreshToken entity operations.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Gets a refresh token by its token value.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Gets all refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of refresh tokens.</returns>
    Task<List<RefreshToken>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all valid (non-revoked, non-expired) tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of valid refresh tokens.</returns>
    Task<List<RefreshToken>> GetValidTokensByUserIdAsync(Guid userId);

    /// <summary>
    /// Revokes all tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="revokedByIp">The IP address that initiated the revocation.</param>
    /// <returns>Number of tokens revoked.</returns>
    Task<int> RevokeAllByUserIdAsync(Guid userId, string? revokedByIp = null);

    /// <summary>
    /// Removes expired tokens from the database.
    /// </summary>
    /// <returns>Number of tokens removed.</returns>
    Task<int> RemoveExpiredTokensAsync();
}
