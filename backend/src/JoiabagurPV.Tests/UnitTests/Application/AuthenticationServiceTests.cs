using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for AuthenticationService.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserPointOfSaleRepository> _userPointOfSaleRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _userPointOfSaleRepositoryMock = new Mock<IUserPointOfSaleRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();

        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:RefreshTokenExpirationHours", "8" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _sut = new AuthenticationService(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _userPointOfSaleRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _unitOfWorkMock.Object,
            _configuration,
            _loggerMock.Object);
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokensAndUserInfo()
    {
        // Arrange
        var password = "ValidPass123!";
        var user = TestDataGenerator.CreateUser(UserRole.Administrator, isActive: true, password: password);
        var request = new LoginRequest { Username = user.Username, Password = password };
        var expectedAccessToken = "access-token-123";
        var expectedRefreshToken = "refresh-token-456";

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user))
            .Returns(expectedAccessToken);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(expectedRefreshToken);

        // Act
        var (response, accessToken, refreshToken) = await _sut.LoginAsync(request, "127.0.0.1");

        // Assert
        response.Should().NotBeNull();
        response.UserId.Should().Be(user.Id);
        response.Username.Should().Be(user.Username);
        response.FirstName.Should().Be(user.FirstName);
        response.LastName.Should().Be(user.LastName);
        response.Role.Should().Be(user.Role.ToString());

        accessToken.Should().Be(expectedAccessToken);
        refreshToken.Should().Be(expectedRefreshToken);

        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldThrowDomainException()
    {
        // Arrange
        var request = new LoginRequest { Username = "nonexistent", Password = "password" };
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.LoginAsync(request, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario o contraseña incorrectos");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowDomainException()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(password: "CorrectPassword123!");
        var request = new LoginRequest { Username = user.Username, Password = "WrongPassword" };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.LoginAsync(request, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario o contraseña incorrectos");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowDomainException()
    {
        // Arrange
        var password = "ValidPass123!";
        var user = TestDataGenerator.CreateUser(isActive: false, password: password);
        var request = new LoginRequest { Username = user.Username, Password = password };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.LoginAsync(request, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario desactivado. Contacte al administrador");
    }

    [Fact]
    public async Task LoginAsync_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var password = "ValidPass123!";
        var user = TestDataGenerator.CreateUser(isActive: true, password: password);
        user.LastLoginAt = null;
        var request = new LoginRequest { Username = user.Username, Password = password };

        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(user.Username))
            .ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("token");
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");

        // Act
        await _sut.LoginAsync(request, "127.0.0.1");

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(isActive: true);
        var storedToken = TestDataGenerator.CreateRefreshToken(user.Id, isRevoked: false, user: user);
        var expectedAccessToken = "new-access-token";
        var expectedRefreshToken = "new-refresh-token";

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(storedToken.Token))
            .ReturnsAsync(storedToken);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(user))
            .Returns(expectedAccessToken);
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(expectedRefreshToken);

        // Act
        var (accessToken, refreshToken) = await _sut.RefreshTokenAsync(storedToken.Token, "127.0.0.1");

        // Assert
        accessToken.Should().Be(expectedAccessToken);
        refreshToken.Should().Be(expectedRefreshToken);
        storedToken.IsRevoked.Should().BeTrue();
        storedToken.ReplacedByToken.Should().Be(expectedRefreshToken);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowDomainException()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("invalid-token"))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = () => _sut.RefreshTokenAsync("invalid-token", "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Token de refresco inválido");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ShouldThrowDomainException()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(isActive: true);
        var storedToken = TestDataGenerator.CreateRefreshToken(user.Id, isRevoked: true, user: user);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(storedToken.Token))
            .ReturnsAsync(storedToken);

        // Act
        var act = () => _sut.RefreshTokenAsync(storedToken.Token, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Token de refresco expirado o revocado");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowDomainException()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(isActive: true);
        var storedToken = TestDataGenerator.CreateRefreshToken(
            user.Id,
            isRevoked: false,
            expiresAt: DateTime.UtcNow.AddHours(-1),
            user: user);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(storedToken.Token))
            .ReturnsAsync(storedToken);

        // Act
        var act = () => _sut.RefreshTokenAsync(storedToken.Token, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Token de refresco expirado o revocado");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInactiveUser_ShouldThrowDomainException()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(isActive: false);
        var storedToken = TestDataGenerator.CreateRefreshToken(user.Id, isRevoked: false, user: user);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(storedToken.Token))
            .ReturnsAsync(storedToken);

        // Act
        var act = () => _sut.RefreshTokenAsync(storedToken.Token, "127.0.0.1");

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario desactivado");
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(isActive: true);
        var storedToken = TestDataGenerator.CreateRefreshToken(user.Id, isRevoked: false, user: user);

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(storedToken.Token))
            .ReturnsAsync(storedToken);

        // Act
        await _sut.LogoutAsync(storedToken.Token, "127.0.0.1");

        // Assert
        storedToken.IsRevoked.Should().BeTrue();
        storedToken.RevokedAt.Should().NotBeNull();
        storedToken.RevokedByIp.Should().Be("127.0.0.1");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ShouldNotThrow()
    {
        // Arrange
        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("invalid-token"))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = () => _sut.LogoutAsync("invalid-token", "127.0.0.1");

        // Assert
        await act.Should().NotThrowAsync();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region GetCurrentUserAsync Tests

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserId_ShouldReturnUserInfo()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Administrator, isActive: true);

        _userRepositoryMock.Setup(x => x.GetWithAssignmentsAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetCurrentUserAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role.ToString());
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithOperatorRole_ShouldIncludeAssignedPointOfSales()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Operator, isActive: true);
        var assignedPosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _userRepositoryMock.Setup(x => x.GetWithAssignmentsAsync(user.Id))
            .ReturnsAsync(user);
        _userPointOfSaleRepositoryMock.Setup(x => x.GetAssignedPointOfSaleIdsAsync(user.Id))
            .ReturnsAsync(assignedPosIds);

        // Act
        var result = await _sut.GetCurrentUserAsync(user.Id);

        // Assert
        result.AssignedPointOfSales.Should().NotBeNull();
        result.AssignedPointOfSales.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidUserId_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetWithAssignmentsAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.GetCurrentUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario no encontrado");
    }

    #endregion
}
