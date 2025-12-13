# Testing de Autenticación JWT

[← Volver al índice](../../testing-backend.md)

## Helper para Generar Tokens de Test

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Joyeria.IntegrationTests.Helpers;

/// <summary>
/// Helper para generar tokens JWT en tests
/// </summary>
public static class JwtTestHelper
{
    private const string TestSecretKey = "test-secret-key-minimum-32-characters-long-for-tests";
    private const string TestIssuer = "JoyeriaAPI-Test";
    private const string TestAudience = "JoyeriaClient-Test";

    /// <summary>
    /// Genera un token JWT válido para tests
    /// </summary>
    public static string GenerateToken(
        string userId,
        string username,
        string role,
        int expirationMinutes = 60)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new("sub", userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Genera token para usuario Admin
    /// </summary>
    public static string GenerateAdminToken() =>
        GenerateToken("admin-test-id", "admin", "Admin");

    /// <summary>
    /// Genera token para usuario Operador
    /// </summary>
    public static string GenerateOperatorToken() =>
        GenerateToken("operator-test-id", "operador", "Operador");

    /// <summary>
    /// Genera token expirado para tests de expiración
    /// </summary>
    public static string GenerateExpiredToken() =>
        GenerateToken("expired-user", "expired", "Admin", expirationMinutes: -1);

    /// <summary>
    /// Genera token inválido (firma incorrecta)
    /// </summary>
    public static string GenerateInvalidToken()
    {
        var validToken = GenerateAdminToken();
        // Modificar la firma para hacerlo inválido
        return validToken.Substring(0, validToken.Length - 5) + "XXXXX";
    }
}
```

---

## ApiFixture con Autenticación Configurada

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Joyeria.IntegrationTests.Fixtures;

public class AuthenticatedApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;

    public AuthenticatedApiFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("joyeria_auth_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configurar BD de test
            ConfigureTestDatabase(services);

            // Configurar JWT para tests
            ConfigureTestAuthentication(services);
        });

        builder.UseEnvironment("Testing");
    }

    private void ConfigureTestDatabase(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<JoyeriaDbContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<JoyeriaDbContext>(options =>
            options.UseNpgsql(_postgresContainer.GetConnectionString()));

        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JoyeriaDbContext>();
        db.Database.Migrate();
        SeedTestUsers(db);
    }

    private void ConfigureTestAuthentication(IServiceCollection services)
    {
        // Reconfigurar JWT para usar claves de test
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "JoyeriaAPI-Test",
                ValidAudience = "JoyeriaClient-Test",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("test-secret-key-minimum-32-characters-long-for-tests")
                ),
                ClockSkew = TimeSpan.Zero // Sin tolerancia para tests
            };
        });
    }

    private void SeedTestUsers(JoyeriaDbContext db)
    {
        if (db.Users.Any()) return;

        db.Users.AddRange(
            new User
            {
                Id = "admin-test-id",
                Username = "admin",
                Email = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                IsActive = true
            },
            new User
            {
                Id = "operator-test-id",
                Username = "operador",
                Email = "operador@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("operador123"),
                Role = "Operador",
                IsActive = true
            }
        );
        db.SaveChanges();
    }

    /// <summary>
    /// Crea un HttpClient con token de Admin
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string? token = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", 
                token ?? JwtTestHelper.GenerateAdminToken()
            );
        return client;
    }
}
```

---

## Tests Completos de Autenticación

```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Joyeria.IntegrationTests.Api;

[Collection("AuthApi")]
public class AuthenticationTests
{
    private readonly AuthenticatedApiFixture _fixture;
    private readonly HttpClient _client;

    public AuthenticationTests(AuthenticatedApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokenWithCorrectClaims()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "admin", Password = "admin123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().BeGreaterThan(0);
        result.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        var loginDto = new LoginDto { Username = "admin", Password = "wrongpassword" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        var loginDto = new LoginDto { Username = "noexiste", Password = "password" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/sales");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_ShouldReturnUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var invalidToken = JwtTestHelper.GenerateInvalidToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", invalidToken);

        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithMalformedToken_ShouldReturnUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "not-a-valid-jwt-token");

        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Role Authorization Tests

    [Fact]
    public async Task AdminEndpoint_WithAdminRole_ShouldReturnOk()
    {
        var adminToken = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminEndpoint_WithOperatorRole_ShouldReturnForbidden()
    {
        var operatorToken = JwtTestHelper.GenerateOperatorToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", operatorToken);

        var response = await _client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task OperatorEndpoint_WithOperatorRole_ShouldReturnOk()
    {
        var operatorToken = JwtTestHelper.GenerateOperatorToken();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", operatorToken);

        var response = await _client.GetAsync("/api/sales");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange - Login first to get refresh token
        var loginDto = new LoginDto { Username = "admin", Password = "admin123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        // Act
        var refreshDto = new RefreshTokenDto { RefreshToken = loginResult!.RefreshToken };
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result!.Token.Should().NotBe(loginResult.Token); // Nuevo token
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturnUnauthorized()
    {
        var refreshDto = new RefreshTokenDto { RefreshToken = "invalid-refresh-token" };

        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
```

---

## Test Básico de Autenticación JWT

```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Joyeria.IntegrationTests.Fixtures;
using Joyeria.Core.DTOs;

namespace Joyeria.IntegrationTests.Api;

[Collection("Api")]
public class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(ApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "admin",
            Password = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().BePositive();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "admin",
            Password = "wrong_password"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ShouldReturnOk()
    {
        // Arrange - Obtener token
        var loginDto = new LoginDto { Username = "admin", Password = "admin123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var tokenResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        // Configurar header de autorización
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResult!.Token);

        // Act
        var response = await _client.GetAsync("/api/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

