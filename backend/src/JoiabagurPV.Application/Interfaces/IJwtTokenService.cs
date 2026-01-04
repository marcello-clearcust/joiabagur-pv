using JoiabagurPV.Domain.Entities;
using System.Security.Claims;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for JWT token operations.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for the user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>The JWT access token.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>The refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates an access token and returns the principal if valid.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The claims principal if valid, null otherwise.</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets the user ID from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user ID if found.</returns>
    Guid? GetUserIdFromPrincipal(ClaimsPrincipal principal);
}
