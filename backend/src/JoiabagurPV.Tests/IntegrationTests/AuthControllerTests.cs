using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for AuthController.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class AuthControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndSetCookies()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin", // Default seeded admin user
            Password = "Admin123!" // Default admin password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Username.Should().Be("admin");
        loginResponse.Role.Should().Be("Administrator");

        // Check cookies were set
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.StartsWith("access_token="));
        cookies.Should().Contain(c => c.StartsWith("refresh_token="));
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldReturnUnauthorized()
    {
        // Arrange
        // Create an inactive user
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var inactiveUser = new User
        {
            Username = "inactive_user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!", workFactor: 4),
            FirstName = "Inactive",
            LastName = "User",
            Role = UserRole.Operator,
            IsActive = false
        };
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "inactive_user",
            Password = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnOkAndNewTokens()
    {
        // Arrange - First login to get tokens
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Extract cookies and create new request with them
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var refreshClient = _factory.CreateClient();
        foreach (var cookie in cookies)
        {
            var cookieParts = cookie.Split(';')[0].Split('=');
            if (cookieParts.Length == 2)
            {
                refreshClient.DefaultRequestHeaders.Add("Cookie", $"{cookieParts[0]}={cookieParts[1]}");
            }
        }

        // Act
        var response = await refreshClient.PostAsync("/api/auth/refresh", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Refresh_WithNoRefreshToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.PostAsync("/api/auth/refresh", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_ShouldClearCookies()
    {
        // Arrange - First login
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin123!" };
        await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange - Login first
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        // Extract access token and create authenticated client
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var authClient = _factory.CreateClient();
        foreach (var cookie in cookies)
        {
            var cookieParts = cookie.Split(';')[0].Split('=');
            if (cookieParts.Length == 2 && cookieParts[0] == "access_token")
            {
                authClient.DefaultRequestHeaders.Add("Cookie", $"access_token={cookieParts[1]}");
            }
        }

        // Act
        var response = await authClient.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userResponse = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        userResponse.Should().NotBeNull();
        userResponse!.Username.Should().Be("admin");
        userResponse.Role.Should().Be("Administrator");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
