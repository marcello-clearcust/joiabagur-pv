using FluentAssertions;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for JwtTokenService.
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;
    private readonly IConfiguration _configuration;
    private const string SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposes12345!";

    public JwtTokenServiceTests()
    {
        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", SecretKey },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:AccessTokenExpirationMinutes", "60" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _sut = new JwtTokenService(_configuration);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithMissingSecretKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        var act = () => new JwtTokenService(configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT SecretKey not configured");
    }

    #endregion

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Administrator);

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserClaims()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Operator);

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Subject.Should().Be(user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == user.Username);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Operator");
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetExpirationTime()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser();

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiration = DateTime.UtcNow.AddMinutes(60);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(UserRole.Administrator)]
    [InlineData(UserRole.Operator)]
    public void GenerateAccessToken_WithDifferentRoles_ShouldIncludeCorrectRole(UserRole role)
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(role);

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role.ToString());
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        var act = () => Convert.FromBase64String(token);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var tokens = Enumerable.Range(0, 100).Select(_ => _sut.GenerateRefreshToken()).ToList();

        // Assert
        tokens.Distinct().Should().HaveCount(100);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldHaveMinimumLength()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert - 64 bytes = 88 chars in base64
        token.Length.Should().BeGreaterOrEqualTo(80);
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Administrator);
        var token = _sut.GenerateAccessToken(user);

        // Act
        var principal = _sut.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _sut.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithTamperedToken_ShouldReturnNull()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser();
        var token = _sut.GenerateAccessToken(user);
        var tamperedToken = token.Substring(0, token.Length - 5) + "xxxxx";

        // Act
        var principal = _sut.ValidateToken(tamperedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange - Create service with very short expiration
        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:SecretKey", SecretKey },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:AccessTokenExpirationMinutes", "0" } // Immediate expiration
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
        var service = new JwtTokenService(configuration);

        var user = TestDataGenerator.CreateUser();
        var token = service.GenerateAccessToken(user);

        // Wait for token to expire
        Thread.Sleep(1100);

        // Act
        var principal = service.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    #endregion

    #region GetUserIdFromPrincipal Tests

    [Fact]
    public void GetUserIdFromPrincipal_WithValidPrincipal_ShouldReturnUserId()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser();
        var token = _sut.GenerateAccessToken(user);
        var principal = _sut.ValidateToken(token);

        // Act
        var userId = _sut.GetUserIdFromPrincipal(principal!);

        // Assert
        userId.Should().NotBeNull();
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void GetUserIdFromPrincipal_WithNoSubClaim_ShouldReturnNull()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Administrator")
        }, "test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.GetUserIdFromPrincipal(principal);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromPrincipal_WithInvalidGuid_ShouldReturnNull()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.GetUserIdFromPrincipal(principal);

        // Assert
        userId.Should().BeNull();
    }

    #endregion
}
