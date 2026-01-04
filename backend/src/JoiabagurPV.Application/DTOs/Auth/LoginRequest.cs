namespace JoiabagurPV.Application.DTOs.Auth;

/// <summary>
/// Request DTO for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username for authentication.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The password for authentication.
    /// </summary>
    public required string Password { get; set; }
}
