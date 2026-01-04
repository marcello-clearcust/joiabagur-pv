using JoiabagurPV.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace JoiabagurPV.API.Services;

/// <summary>
/// Service for accessing current authenticated user information from HttpContext.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                             ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    /// <inheritdoc/>
    public string? Username => 
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value 
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("unique_name")?.Value;

    /// <inheritdoc/>
    public string? Role
    {
        get
        {
            // Try multiple claim types for role
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("role")
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("roles");

            var role = roleClaim?.Value;
            // Temporary debugging - remove after fixing
            _logger.LogInformation("CurrentUserService.Role: ClaimType='{Type}', Claim found={Found}, Value='{Value}'",
                roleClaim?.Type, roleClaim != null, role);
            return role;
        }
    }

    /// <inheritdoc/>
    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc/>
    public bool IsAdmin
    {
        get
        {
            var role = Role;
            var isAdmin = role?.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ?? false;
            // Temporary debugging - remove after fixing
            _logger.LogInformation("CurrentUserService.IsAdmin check: Role='{Role}', IsAdmin={IsAdmin}", role, isAdmin);
            return isAdmin;
        }
    }
}
