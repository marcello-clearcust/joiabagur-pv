using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.PaymentMethods;
using JoiabagurPV.Application.DTOs.PointOfSales;
using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for PointOfSalesController.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class PointOfSalesControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;

    public PointOfSalesControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        
        // Create an operator user for testing
        var createOperatorRequest = new CreateUserRequest
        {
            Username = "testoperator",
            Password = "Operator123!",
            FirstName = "Test",
            LastName = "Operator",
            Role = "Operator"
        };
        await _adminClient.PostAsJsonAsync("/api/users", createOperatorRequest);
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
    public async Task GetAll_AsAdmin_ShouldReturnAllPointOfSales()
    {
        // Arrange - Create a point of sale first
        var createRequest = new CreatePointOfSaleRequest
        {
            Name = "Test Store",
            Code = "TS-001",
            Address = "123 Test St"
        };
        await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createRequest);

        // Act
        var response = await _adminClient!.GetAsync("/api/point-of-sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pointOfSales = await response.Content.ReadFromJsonAsync<List<PointOfSaleDto>>();
        pointOfSales.Should().NotBeNull();
        pointOfSales.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/point-of-sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnPointOfSale()
    {
        // Arrange
        var createRequest = new CreatePointOfSaleRequest
        {
            Name = "Test Store",
            Code = "TS-002"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createRequest);
        var createdPos = await createResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Act
        var response = await _adminClient!.GetAsync($"/api/point-of-sales/{createdPos!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pointOfSale = await response.Content.ReadFromJsonAsync<PointOfSaleDto>();
        pointOfSale.Should().NotBeNull();
        pointOfSale!.Name.Should().Be("Test Store");
        pointOfSale.Code.Should().Be("TS-002");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _adminClient!.GetAsync($"/api/point-of-sales/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreatedPointOfSale()
    {
        // Arrange
        var request = new CreatePointOfSaleRequest
        {
            Name = "New Store",
            Code = "NS-001",
            Address = "456 New St",
            Phone = "555-1234",
            Email = "newstore@example.com"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var pointOfSale = await response.Content.ReadFromJsonAsync<PointOfSaleDto>();
        pointOfSale.Should().NotBeNull();
        pointOfSale!.Name.Should().Be("New Store");
        pointOfSale.Code.Should().Be("NS-001");
        pointOfSale.Address.Should().Be("456 New St");
        pointOfSale.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ShouldReturnConflict()
    {
        // Arrange
        var request = new CreatePointOfSaleRequest
        {
            Name = "First Store",
            Code = "DUP-001"
        };
        await _adminClient!.PostAsJsonAsync("/api/point-of-sales", request);

        var duplicateRequest = new CreatePointOfSaleRequest
        {
            Name = "Second Store",
            Code = "DUP-001" // Same code
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithMissingName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreatePointOfSaleRequest
        {
            Name = "", // Empty name
            Code = "EMPTY-001"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithMissingCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreatePointOfSaleRequest
        {
            Name = "Store Without Code",
            Code = "" // Empty code
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidData_ShouldReturnUpdatedPointOfSale()
    {
        // Arrange
        var createRequest = new CreatePointOfSaleRequest
        {
            Name = "Original Store",
            Code = "UPD-001"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createRequest);
        var createdPos = await createResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        var updateRequest = new UpdatePointOfSaleRequest
        {
            Name = "Updated Store",
            Address = "789 Updated St",
            Phone = "555-9999",
            Email = "updated@example.com",
            IsActive = true
        };

        // Act
        var response = await _adminClient!.PutAsJsonAsync($"/api/point-of-sales/{createdPos!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pointOfSale = await response.Content.ReadFromJsonAsync<PointOfSaleDto>();
        pointOfSale.Should().NotBeNull();
        pointOfSale!.Name.Should().Be("Updated Store");
        pointOfSale.Address.Should().Be("789 Updated St");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var updateRequest = new UpdatePointOfSaleRequest
        {
            Name = "Updated Store",
            IsActive = true
        };

        // Act
        var response = await _adminClient!.PutAsJsonAsync($"/api/point-of-sales/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ChangeStatus Tests

    [Fact]
    public async Task ChangeStatus_Activate_ShouldReturnOk()
    {
        // Arrange
        var createRequest = new CreatePointOfSaleRequest
        {
            Name = "Status Test Store",
            Code = "STS-001"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createRequest);
        var createdPos = await createResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Act
        var response = await _adminClient!.PatchAsJsonAsync(
            $"/api/point-of-sales/{createdPos!.Id}/status",
            new { IsActive = true });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pointOfSale = await response.Content.ReadFromJsonAsync<PointOfSaleDto>();
        pointOfSale.Should().NotBeNull();
        pointOfSale!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeStatus_Deactivate_WithoutAssignments_ShouldReturnOk()
    {
        // Arrange
        var createRequest = new CreatePointOfSaleRequest
        {
            Name = "Deactivate Test Store",
            Code = "DTS-001"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createRequest);
        var createdPos = await createResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Act
        var response = await _adminClient!.PatchAsJsonAsync(
            $"/api/point-of-sales/{createdPos!.Id}/status",
            new { IsActive = false });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pointOfSale = await response.Content.ReadFromJsonAsync<PointOfSaleDto>();
        pointOfSale.Should().NotBeNull();
        pointOfSale!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeStatus_Deactivate_ShouldStillBeReturnedInList()
    {
        // Arrange
        var createRequest = new CreatePointOfSaleRequest
        {
            Name = "Deactivate List Test Store",
            Code = "DLTS-001"
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createRequest);
        var createdPos = await createResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Act - Deactivate the POS
        await _adminClient!.PatchAsJsonAsync(
            $"/api/point-of-sales/{createdPos!.Id}/status",
            new { IsActive = false });

        // Assert - POS should still be in the list (inactive)
        var listResponse = await _adminClient!.GetAsync("/api/point-of-sales");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pointOfSales = await listResponse.Content.ReadFromJsonAsync<List<PointOfSaleDto>>();
        pointOfSales.Should().NotBeNull();
        pointOfSales.Should().Contain(pos => pos.Id == createdPos.Id && pos.IsActive == false);
    }

    #endregion

    #region Operator Assignment Tests

    [Fact]
    public async Task AssignOperator_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Assignment Test Store",
            Code = "ATS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get the operator user
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var operatorUser = await context.Users.FirstAsync(u => u.Username == "testoperator");

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/operators/{operatorUser.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AssignOperator_WithNonExistentPointOfSale_ShouldReturnNotFound()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var operatorUser = await context.Users.FirstAsync(u => u.Username == "testoperator");

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/point-of-sales/{Guid.NewGuid()}/operators/{operatorUser.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignOperator_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "User Not Found Store",
            Code = "UNF-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/operators/{Guid.NewGuid()}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnassignOperator_WithValidAssignment_ShouldReturnNoContent()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Unassign Test Store 1",
            Code = "UTS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos1 = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Create second point of sale (needed because operator must have at least one assignment)
        var createPosRequest2 = new CreatePointOfSaleRequest
        {
            Name = "Unassign Test Store 2",
            Code = "UTS-002"
        };
        var createPosResponse2 = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest2);
        var createdPos2 = await createPosResponse2.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get the operator user
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var operatorUser = await context.Users.FirstAsync(u => u.Username == "testoperator");

        // Assign operator to both points of sale
        await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos1!.Id}/operators/{operatorUser.Id}",
            null);
        await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos2!.Id}/operators/{operatorUser.Id}",
            null);

        // Act
        var response = await _adminClient!.DeleteAsync(
            $"/api/point-of-sales/{createdPos1.Id}/operators/{operatorUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnassignOperator_WithLastAssignment_ShouldReturnBadRequest()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Last Assignment Store",
            Code = "LAS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Create a new operator specifically for this test
        var createOperatorRequest = new CreateUserRequest
        {
            Username = "singleassignoperator",
            Password = "Operator123!",
            FirstName = "Single",
            LastName = "Assign",
            Role = "Operator"
        };
        var createOperatorResponse = await _adminClient!.PostAsJsonAsync("/api/users", createOperatorRequest);
        var createdOperator = await createOperatorResponse.Content.ReadFromJsonAsync<UserDto>();

        // Assign operator to point of sale (this is the only assignment)
        await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/operators/{createdOperator!.Id}",
            null);

        // Act - Try to unassign the only assignment
        var response = await _adminClient!.DeleteAsync(
            $"/api/point-of-sales/{createdPos.Id}/operators/{createdOperator.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task Create_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        // First, create a point of sale and assign the operator to it so they can log in
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Operator Auth Test",
            Code = "OAT-001"
        };
        await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);

        // Authenticate as operator
        _operatorClient = await CreateAuthenticatedClientAsync("testoperator", "Operator123!");

        var request = new CreatePointOfSaleRequest
        {
            Name = "Unauthorized Store",
            Code = "UNAUTH-001"
        };

        // Act
        var response = await _operatorClient.PostAsJsonAsync("/api/point-of-sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Operator Update Auth Test",
            Code = "OUAT-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        _operatorClient = await CreateAuthenticatedClientAsync("testoperator", "Operator123!");

        var updateRequest = new UpdatePointOfSaleRequest
        {
            Name = "Updated By Operator",
            IsActive = true
        };

        // Act
        var response = await _operatorClient.PutAsJsonAsync($"/api/point-of-sales/{createdPos!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignOperator_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Operator Assign Auth Test",
            Code = "OAAT-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        _operatorClient = await CreateAuthenticatedClientAsync("testoperator", "Operator123!");

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var operatorUser = await context.Users.FirstAsync(u => u.Username == "testoperator");

        // Act
        var response = await _operatorClient.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/operators/{operatorUser.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Payment Method Assignment Tests

    [Fact]
    public async Task AssignPaymentMethod_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Payment Method Test Store",
            Code = "PMTS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        createPosResponse.StatusCode.Should().Be(HttpStatusCode.Created, "POS creation should succeed");
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get a pre-seeded payment method (CASH is seeded by default)
        var getPaymentMethodsResponse = await _adminClient!.GetAsync("/api/payment-methods");
        getPaymentMethodsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var paymentMethods = await getPaymentMethodsResponse.Content.ReadFromJsonAsync<List<PaymentMethodDto>>();
        var cashMethod = paymentMethods!.First(pm => pm.Code == "CASH");

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/payment-methods/{cashMethod.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var assignment = await response.Content.ReadFromJsonAsync<PointOfSalePaymentMethodDto>();
        assignment.Should().NotBeNull();
        assignment!.PointOfSaleId.Should().Be(createdPos.Id);
        assignment.PaymentMethodId.Should().Be(cashMethod.Id);
        assignment.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AssignPaymentMethod_WithNonExistentPointOfSale_ShouldReturnNotFound()
    {
        // Arrange
        // Get a pre-seeded payment method
        var getPaymentMethodsResponse = await _adminClient!.GetAsync("/api/payment-methods");
        var paymentMethods = await getPaymentMethodsResponse.Content.ReadFromJsonAsync<List<PaymentMethodDto>>();
        var bizumMethod = paymentMethods!.First(pm => pm.Code == "BIZUM");

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/point-of-sales/{Guid.NewGuid()}/payment-methods/{bizumMethod.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignPaymentMethod_WithNonExistentPaymentMethod_ShouldReturnNotFound()
    {
        // Arrange
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Payment Method Not Found Store",
            Code = "PMNF-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Act
        var response = await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/payment-methods/{Guid.NewGuid()}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignPaymentMethod_AlreadyAssigned_ShouldReturnConflict()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Duplicate Assignment Store",
            Code = "DAS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get a pre-seeded payment method
        var getPaymentMethodsResponse = await _adminClient!.GetAsync("/api/payment-methods");
        var paymentMethods = await getPaymentMethodsResponse.Content.ReadFromJsonAsync<List<PaymentMethodDto>>();
        var cardMethod = paymentMethods!.First(pm => pm.Code == "CARD_OWN");

        // First assignment
        await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/payment-methods/{cardMethod.Id}",
            null);

        // Act - Try to assign again
        var response = await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos.Id}/payment-methods/{cardMethod.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetPaymentMethods_ShouldReturnAssignments()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Get Payment Methods Store",
            Code = "GPMS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get a pre-seeded payment method
        var getPaymentMethodsResponse = await _adminClient!.GetAsync("/api/payment-methods");
        var paymentMethods = await getPaymentMethodsResponse.Content.ReadFromJsonAsync<List<PaymentMethodDto>>();
        var transferMethod = paymentMethods!.First(pm => pm.Code == "TRANSFER");

        // Assign the payment method
        await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/payment-methods/{transferMethod.Id}",
            null);

        // Act
        var response = await _adminClient!.GetAsync(
            $"/api/point-of-sales/{createdPos.Id}/payment-methods");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignments = await response.Content.ReadFromJsonAsync<List<PointOfSalePaymentMethodDto>>();
        assignments.Should().NotBeNull();
        assignments.Should().ContainSingle();
        assignments!.First().PaymentMethodId.Should().Be(transferMethod.Id);
    }

    [Fact]
    public async Task UnassignPaymentMethod_WithValidAssignment_ShouldReturnNoContent()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Unassign Payment Method Store",
            Code = "UPMS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get a pre-seeded payment method
        var getPaymentMethodsResponse = await _adminClient!.GetAsync("/api/payment-methods");
        var paymentMethods = await getPaymentMethodsResponse.Content.ReadFromJsonAsync<List<PaymentMethodDto>>();
        var cardPosMethod = paymentMethods!.First(pm => pm.Code == "CARD_POS");

        // Assign the payment method
        await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/payment-methods/{cardPosMethod.Id}",
            null);

        // Act
        var response = await _adminClient!.DeleteAsync(
            $"/api/point-of-sales/{createdPos.Id}/payment-methods/{cardPosMethod.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePaymentMethodStatus_ShouldUpdateStatus()
    {
        // Arrange
        // Create a point of sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Change Status Store",
            Code = "CSS-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get a pre-seeded payment method
        var getPaymentMethodsResponse = await _adminClient!.GetAsync("/api/payment-methods");
        var paymentMethods = await getPaymentMethodsResponse.Content.ReadFromJsonAsync<List<PaymentMethodDto>>();
        var paypalMethod = paymentMethods!.First(pm => pm.Code == "PAYPAL");

        // Assign the payment method
        await _adminClient!.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/payment-methods/{paypalMethod.Id}",
            null);

        // Act - Deactivate the assignment
        var response = await _adminClient!.PatchAsJsonAsync(
            $"/api/point-of-sales/{createdPos.Id}/payment-methods/{paypalMethod.Id}/status",
            new { IsActive = false });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignment = await response.Content.ReadFromJsonAsync<PointOfSalePaymentMethodDto>();
        assignment.Should().NotBeNull();
        assignment!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AssignPaymentMethod_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Name = "Operator PM Auth Test",
            Code = "OPMAT-001"
        };
        var createPosResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var createdPos = await createPosResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        // Get a pre-seeded payment method
        var getPaymentMethodsResponse = await _adminClient!.GetAsync("/api/payment-methods");
        var paymentMethods = await getPaymentMethodsResponse.Content.ReadFromJsonAsync<List<PaymentMethodDto>>();
        var cashMethod = paymentMethods!.First(pm => pm.Code == "CASH");

        _operatorClient = await CreateAuthenticatedClientAsync("testoperator", "Operator123!");

        // Act
        var response = await _operatorClient.PostAsync(
            $"/api/point-of-sales/{createdPos!.Id}/payment-methods/{cashMethod.Id}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
