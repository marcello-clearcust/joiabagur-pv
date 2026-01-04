using JoiabagurPV.Application.DTOs.Auth;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>A tuple containing the login response and tokens (access, refresh).</returns>
    Task<(LoginResponse Response, string AccessToken, string RefreshToken)> LoginAsync(
        LoginRequest request, 
        string? ipAddress);

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The current refresh token.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <returns>A tuple containing the new tokens (access, refresh).</returns>
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(
        string refreshToken, 
        string? ipAddress);

    /// <summary>
    /// Logs out the user by revoking the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    Task LogoutAsync(string refreshToken, string? ipAddress);

    /// <summary>
    /// Gets the current user's information.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>The current user response.</returns>
    Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId);
}
