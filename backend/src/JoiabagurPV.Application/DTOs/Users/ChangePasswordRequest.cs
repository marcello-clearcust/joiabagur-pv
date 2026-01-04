namespace JoiabagurPV.Application.DTOs.Users;

/// <summary>
/// Request DTO for changing a user's password.
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// The new password.
    /// </summary>
    public required string NewPassword { get; set; }
}
