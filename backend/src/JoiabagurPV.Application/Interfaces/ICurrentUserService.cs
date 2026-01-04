namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for accessing current authenticated user information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's username.
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Gets the current user's role.
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user is an administrator.
    /// </summary>
    bool IsAdmin { get; }
}
