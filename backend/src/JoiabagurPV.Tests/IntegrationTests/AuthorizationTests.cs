using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for authorization (role-based and resource-based).
/// </summary>
public class AuthorizationTests : IClassFixture<ApiWebApplicationFactory>, IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;
    private User? _operatorUser;

    public AuthorizationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();

        // Create an operator user for testing
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _operatorUser = new User
        {
            Username = "test_operator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operator123!", workFactor: 4),
            FirstName = "Test",
            LastName = "Operator",
            Role = UserRole.Operator,
            IsActive = true
        };
        context.Users.Add(_operatorUser);
        await context.SaveChangesAsync();

        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        _operatorClient = await CreateAuthenticatedClientAsync("test_operator", "Operator123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var loginRequest = new LoginRequest { Username = username, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

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

        return authClient;
    }

    #region Role-Based Authorization Tests

    [Fact]
    public async Task UsersEndpoint_AsOperator_ShouldReturnForbidden()
    {
        // Act - Operator trying to access administrator-only endpoint
        var response = await _operatorClient!.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UsersEndpoint_AsAdmin_ShouldReturnOk()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "Pass123!",
            FirstName = "New",
            LastName = "User",
            Role = "Operator"
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUser_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Role = "Operator",
            IsActive = true
        };

        // Act
        var response = await _operatorClient!.PutAsJsonAsync($"/api/users/{_operatorUser!.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangePassword_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var request = new ChangePasswordRequest { NewPassword = "NewPass123!" };

        // Act
        var response = await _operatorClient!.PutAsJsonAsync(
            $"/api/users/{_operatorUser!.Id}/password",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignPointOfSale_AsOperator_ShouldReturnForbidden()
    {
        // Act
        var response = await _operatorClient!.PostAsync(
            $"/api/users/{_operatorUser!.Id}/point-of-sales/{Guid.NewGuid()}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Authentication Required Tests

    [Fact]
    public async Task ProtectedEndpoint_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_RequiresAuthentication()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_AsOperator_ShouldReturnOk()
    {
        // Act - Operators can access /me endpoint
        var response = await _operatorClient!.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("test_operator");
        user.Role.Should().Be("Operator");
    }

    [Fact]
    public async Task GetCurrentUser_AsAdmin_ShouldReturnOk()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("admin");
        user.Role.Should().Be("Administrator");
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public async Task ExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange - Create a client with an invalid token
        var invalidClient = _factory.CreateClient();
        invalidClient.DefaultRequestHeaders.Add("Cookie", "access_token=invalid.token.here");

        // Act
        var response = await invalidClient.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TamperedToken_ShouldReturnUnauthorized()
    {
        // Arrange - Login to get a valid token structure, then tamper it
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();

        var accessTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("access_token="));
        var tamperedToken = accessTokenCookie?.Split(';')[0].Split('=')[1];
        if (tamperedToken != null && tamperedToken.Length > 10)
        {
            // Tamper the token
            tamperedToken = tamperedToken.Substring(0, tamperedToken.Length - 5) + "xxxxx";
        }

        var tamperedClient = _factory.CreateClient();
        tamperedClient.DefaultRequestHeaders.Add("Cookie", $"access_token={tamperedToken}");

        // Act
        var response = await tamperedClient.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
