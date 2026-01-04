namespace JoiabagurPV.Application.DTOs.Users;

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// The username for the new user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The password for the new user.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// The user's email (optional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The user's role (Administrator or Operator).
    /// </summary>
    public required string Role { get; set; }
}
