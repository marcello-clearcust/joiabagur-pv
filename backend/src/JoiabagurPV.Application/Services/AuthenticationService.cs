using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for authentication operations.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserPointOfSaleRepository _userPointOfSaleRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly int _refreshTokenExpirationHours;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUserPointOfSaleRepository userPointOfSaleRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _userPointOfSaleRepository = userPointOfSaleRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _refreshTokenExpirationHours = int.Parse(configuration["Jwt:RefreshTokenExpirationHours"] ?? "8");
    }

    /// <inheritdoc/>
    public async Task<(LoginResponse Response, string AccessToken, string RefreshToken)> LoginAsync(
        LoginRequest request,
        string? ipAddress)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username} from IP: {IpAddress}",
                request.Username, ipAddress);
            throw new DomainException("Usuario o contraseña incorrectos");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Username} from IP: {IpAddress}",
                request.Username, ipAddress);
            throw new DomainException("Usuario desactivado. Contacte al administrador");
        }

        _logger.LogInformation("User loaded: Username={Username}, Role={Role}, IsActive={IsActive}",
            user.Username, user.Role, user.IsActive);

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        // Store refresh token
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(_refreshTokenExpirationHours),
            CreatedByIp = ipAddress
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Username} logged in successfully from IP: {IpAddress}", 
            user.Username, ipAddress);

        var response = new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString()
        };

        return (response, accessToken, refreshTokenString);
    }

    /// <inheritdoc/>
    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null)
        {
            throw new DomainException("Token de refresco inválido");
        }

        if (!storedToken.IsValid)
        {
            throw new DomainException("Token de refresco expirado o revocado");
        }

        var user = storedToken.User;
        if (!user.IsActive)
        {
            throw new DomainException("Usuario desactivado");
        }

        // Revoke old token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken();

        storedToken.ReplacedByToken = newRefreshTokenString;

        // Store new refresh token
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(_refreshTokenExpirationHours),
            CreatedByIp = ipAddress
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Token refreshed for user {Username} from IP: {IpAddress}", 
            user.Username, ipAddress);

        return (newAccessToken, newRefreshTokenString);
    }

    /// <inheritdoc/>
    public async Task LogoutAsync(string refreshToken, string? ipAddress)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken != null && storedToken.IsValid)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged out from IP: {IpAddress}", 
                storedToken.UserId, ipAddress);
        }
    }

    /// <inheritdoc/>
    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetWithAssignmentsAsync(userId);

        if (user == null)
        {
            throw new DomainException("Usuario no encontrado");
        }

        var response = new CurrentUserResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString()
        };

        // For operators, include assigned point of sales
        // Note: In a full implementation, we'd join with PointOfSale entity to get names/codes
        // For now, we return the IDs - this will be enhanced when PointOfSale entity exists
        if (user.Role == Domain.Enums.UserRole.Operator)
        {
            var assignedIds = await _userPointOfSaleRepository.GetAssignedPointOfSaleIdsAsync(userId);
            response.AssignedPointOfSales = assignedIds.Select(id => new AssignedPointOfSaleDto
            {
                Id = id,
                Name = "Pending", // Will be populated when PointOfSale entity exists
                Code = "Pending"
            }).ToList();
        }

        return response;
    }
}
