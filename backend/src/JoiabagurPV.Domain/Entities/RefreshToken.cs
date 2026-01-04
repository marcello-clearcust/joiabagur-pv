namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a refresh token for session management.
/// Stored in database for revocation capability.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// The token value.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The user this token belongs to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the token has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// When the token was revoked (if applicable).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// IP address from which the token was created.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP address from which the token was revoked (if applicable).
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// The token that replaced this one (if rotated).
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Checks if the token is currently valid.
    /// </summary>
    public bool IsValid => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}
