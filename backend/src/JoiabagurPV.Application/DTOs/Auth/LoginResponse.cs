namespace JoiabagurPV.Application.DTOs.Auth;

/// <summary>
/// Response DTO for successful login.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public Guid UserId { get; set; }

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
    /// The user's role.
    /// </summary>
    public required string Role { get; set; }
}
