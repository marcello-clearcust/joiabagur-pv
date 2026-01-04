namespace JoiabagurPV.Application.DTOs.Users;

/// <summary>
/// Request DTO for updating an existing user.
/// </summary>
public class UpdateUserRequest
{
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

    /// <summary>
    /// Whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }
}
