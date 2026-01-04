using JoiabagurPV.Domain.Enums;

namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents a system user with authentication credentials and role.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Unique username for authentication.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// BCrypt hashed password.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Optional email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User's role in the system.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp of the user's last successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Navigation property for point of sale assignments.
    /// </summary>
    public ICollection<UserPointOfSale> PointOfSaleAssignments { get; set; } = new List<UserPointOfSale>();

    /// <summary>
    /// Navigation property for refresh tokens.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}
