using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.PointOfSales;
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
/// Integration tests for UsersController.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class UsersControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;

    public UsersControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
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

    #region GetAll Tests

    [Fact]
    public async Task GetAll_AsAdmin_ShouldReturnAllUsers()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = await context.Users.FirstAsync(u => u.Username == "admin");

        // Act
        var response = await _adminClient!.GetAsync($"/api/users/{admin.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("admin");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _adminClient!.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "NewPass123!",
            FirstName = "New",
            LastName = "User",
            Email = "newuser@example.com",
            Role = "Operator"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("newuser");
        user.FirstName.Should().Be("New");
        user.Role.Should().Be("Operator");
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithDuplicateUsername_ShouldReturnConflict()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "admin", // Already exists
            Password = "NewPass123!",
            FirstName = "Test",
            LastName = "User",
            Role = "Operator"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithInvalidRole_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Password = "NewPass123!",
            FirstName = "Test",
            LastName = "User",
            Role = "InvalidRole"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "testuser",
            Password = "short", // Too short
            FirstName = "Test",
            LastName = "User",
            Role = "Operator"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_ShouldReturnUpdatedUser()
    {
        // Arrange
        // First create a user
        var createRequest = new CreateUserRequest
        {
            Username = "updatetest",
            Password = "Pass123!",
            FirstName = "Original",
            LastName = "Name",
            Role = "Operator"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/users", createRequest);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var updateRequest = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com",
            Role = "Operator",
            IsActive = true
        };

        // Act
        var response = await _adminClient!.PutAsJsonAsync($"/api/users/{createdUser!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.FirstName.Should().Be("Updated");
        user.LastName.Should().Be("User");
        user.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Role = "Operator",
            IsActive = true
        };

        // Act
        var response = await _adminClient!.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ChangePassword Tests

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        // First create a user
        var createRequest = new CreateUserRequest
        {
            Username = "passwordtest",
            Password = "OldPass123!",
            FirstName = "Password",
            LastName = "Test",
            Role = "Operator"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/users", createRequest);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var passwordRequest = new ChangePasswordRequest { NewPassword = "NewPass456!" };

        // Act
        var response = await _adminClient!.PutAsJsonAsync($"/api/users/{createdUser!.Id}/password", passwordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify new password works
        var loginRequest = new LoginRequest { Username = "passwordtest", Password = "NewPass456!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Point of Sale Assignment Tests

    [Fact]
    public async Task GetUserPointOfSales_ShouldReturnAssignments()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = await context.Users.FirstAsync(u => u.Username == "admin");

        // Act
        var response = await _adminClient!.GetAsync($"/api/users/{admin.Id}/point-of-sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignments = await response.Content.ReadFromJsonAsync<List<UserPointOfSaleDto>>();
        assignments.Should().NotBeNull();
    }

    [Fact]
    public async Task AssignPointOfSale_WithOperator_ShouldReturnCreated()
    {
        // Arrange
        // Create a point of sale first
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "User Assignment Test Store",
            Code = "UATS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Create an operator user
        var createRequest = new CreateUserRequest
        {
            Username = "operator1",
            Password = "Pass123!",
            FirstName = "Operator",
            LastName = "One",
            Role = "Operator"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/users", createRequest);
        var operatorUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/users/{operatorUser!.Id}/point-of-sales/{createdPos!.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AssignPointOfSale_WithAdmin_ShouldReturnBadRequest()
    {
        // Arrange
        // Create a point of sale first
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Admin Assignment Test Store",
            Code = "AATS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = await context.Users.FirstAsync(u => u.Username == "admin");

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/users/{admin.Id}/point-of-sales/{createdPos!.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
