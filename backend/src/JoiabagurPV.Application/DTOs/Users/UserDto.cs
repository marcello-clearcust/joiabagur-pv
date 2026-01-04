namespace JoiabagurPV.Application.DTOs.Users;

/// <summary>
/// DTO for user information.
/// </summary>
public class UserDto
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The user's username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// The user's email (if provided).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The user's role.
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// Whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
