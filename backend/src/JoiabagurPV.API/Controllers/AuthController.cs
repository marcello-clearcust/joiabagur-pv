using FluentValidation;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    // Cookie names
    private const string AccessTokenCookieName = "access_token";
    private const string RefreshTokenCookieName = "refresh_token";

    public AuthController(
        IAuthenticationService authenticationService,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _authenticationService = authenticationService;
        _loginValidator = loginValidator;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>User information on successful authentication.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginRateLimit")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var ipAddress = GetIpAddress();
            var (response, accessToken, refreshToken) = await _authenticationService.LoginAsync(request, ipAddress);

            // Set cookies for same-origin scenarios
            SetTokenCookies(accessToken, refreshToken);

            // Also include tokens in response body for cross-origin scenarios
            // where third-party cookies are blocked
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;

            return Ok(response);
        }
        catch (DomainException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes the access token using the refresh token cookie.
    /// </summary>
    /// <returns>200 OK if successful, new tokens are set in cookies.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Token de refresco no proporcionado" });
        }

        try
        {
            var ipAddress = GetIpAddress();
            var (accessToken, newRefreshToken) = await _authenticationService.RefreshTokenAsync(refreshToken, ipAddress);

            SetTokenCookies(accessToken, newRefreshToken);

            return Ok(new { message = "Token refrescado correctamente" });
        }
        catch (DomainException ex)
        {
            ClearTokenCookies();
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Logs out the current user by revoking tokens and clearing cookies.
    /// </summary>
    /// <returns>204 No Content on success.</returns>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var ipAddress = GetIpAddress();
            await _authenticationService.LogoutAsync(refreshToken, ipAddress);
        }

        ClearTokenCookies();

        return NoContent();
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>Current user information.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "Usuario no autenticado" });
        }

        try
        {
            var response = await _authenticationService.GetCurrentUserAsync(userId);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var accessTokenExpiration = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        var refreshTokenExpiration = int.Parse(_configuration["Jwt:RefreshTokenExpirationHours"] ?? "8");

        // In Development we commonly run over plain HTTP (e.g. http://localhost:5056),
        // so Secure cookies would never be set/sent and everything becomes 401.
        var secure = !_environment.IsDevelopment() && Request.IsHttps;
        // Use SameSite=None for cross-origin requests (CloudFront -> App Runner)
        // This requires Secure=true, which we have in production
        var sameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;

        // Access token cookie
        Response.Cookies.Append(AccessTokenCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpiration)
        });

        // Refresh token cookie
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Path = "/api/auth", // Only send to auth endpoints
            Expires = DateTimeOffset.UtcNow.AddHours(refreshTokenExpiration)
        });
    }

    private void ClearTokenCookies()
    {
        Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
        {
            Path = "/"
        });

        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/api/auth"
        });
    }

    private string? GetIpAddress()
    {
        // Check for X-Forwarded-For header (when behind proxy)
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
