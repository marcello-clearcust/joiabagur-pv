using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.Returns;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Infrastructure.Data;
using JoiabagurPV.Tests.TestHelpers.Mothers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for ReturnsController.
/// Tests return creation, eligible sales, history, authorization, and transaction integrity.
/// Uses Respawn for database cleanup and Mother Objects for test data creation.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class ReturnsControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;

    // Test data created via Mother Objects
    private Product _testProduct = null!;
    private PointOfSale _testPos = null!;
    private PaymentMethod _testPaymentMethod = null!;
    private User _testOperator = null!;
    private Sale _testSale = null!;
    private Inventory _testInventory = null!;

    public ReturnsControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Use Respawn to reset database - no manual TRUNCATEs needed
        await _factory.ResetDatabaseAsync();
        
        // Authenticate as admin (created by seeder)
        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        
        // Create test data using Mother Objects
        await SetupTestDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Sets up all test data using the Mother Object pattern.
    /// Creates interconnected entities needed for return testing.
    /// </summary>
    private async Task SetupTestDataAsync()
    {
        using var mother = new TestDataMother(_factory.Services);

        // Create point of sale
        _testPos = await mother.PointOfSale()
            .WithCode("RET-TEST-POS")
            .WithName("Returns Test POS")
            .WithAddress("Test Address")
            .CreateAsync();

        // Create payment method
        _testPaymentMethod = await mother.PaymentMethod()
            .WithCode("CASH-RET")
            .WithName("Cash")
            .WithDescription("Cash payment for returns tests")
            .CreateAsync();

        // Associate payment method with POS
        mother.Context.PointOfSalePaymentMethods.Add(new PointOfSalePaymentMethod
        {
            Id = Guid.NewGuid(),
            PointOfSaleId = _testPos.Id,
            PaymentMethodId = _testPaymentMethod.Id,
            CreatedAt = DateTime.UtcNow
        });
        await mother.Context.SaveChangesAsync();

        // Create product
        _testProduct = await mother.Product()
            .WithSku("RET-TEST-001")
            .WithName("Returns Test Product")
            .WithDescription("Product for returns integration tests")
            .WithPrice(150.00m)
            .CreateAsync();

        // Create inventory with initial stock
        _testInventory = await mother.Inventory()
            .WithProduct(_testProduct.Id)
            .WithPointOfSale(_testPos.Id)
            .WithQuantity(20)
            .CreateAsync();

        // Create operator and assign to POS
        _testOperator = await mother.User()
            .WithUsername("returnsoperator")
            .WithName("Returns", "Operator")
            .AsOperator()
            .AssignedTo(_testPos.Id)
            .CreateAsync();

        // Authenticate as the operator
        _operatorClient = await CreateAuthenticatedClientAsync("returnsoperator", "Test123!");

        // Create a sale (to be eligible for returns)
        _testSale = await mother.Sale()
            .WithProduct(_testProduct.Id)
            .WithPointOfSale(_testPos.Id)
            .WithPaymentMethod(_testPaymentMethod.Id)
            .WithUser(_testOperator.Id)
            .WithQuantity(5)
            .WithUnitPrice(150.00m)
            .WithSaleDate(DateTime.UtcNow.AddDays(-10)) // Within 30-day return window
            .WithNotes("Test sale for returns")
            .CreateAsync();

        // Update inventory to reflect the sale
        _testInventory.Quantity -= 5;
        await mother.Context.SaveChangesAsync();
    }

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
            if (cookieParts.Length == 2)
            {
                authClient.DefaultRequestHeaders.Add("Cookie", $"{cookieParts[0]}={cookieParts[1]}");
            }
        }

        return authClient;
    }

    #region Get Eligible Sales Tests

    [Fact]
    public async Task GetEligibleSales_WithValidProductAndPOS_ReturnsEligibleSales()
    {
        // Act
        var response = await _operatorClient!.GetAsync(
            $"/api/returns/eligible-sales?productId={_testProduct.Id}&pointOfSaleId={_testPos.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<EligibleSalesResponse>();
        result.Should().NotBeNull();
        result!.EligibleSales.Should().HaveCount(1);
        result.EligibleSales[0].SaleId.Should().Be(_testSale.Id);
        result.EligibleSales[0].OriginalQuantity.Should().Be(5);
        result.EligibleSales[0].AvailableForReturn.Should().Be(5);
        result.EligibleSales[0].UnitPrice.Should().Be(150.00m);
        result.TotalAvailableForReturn.Should().Be(5);
    }

    [Fact]
    public async Task GetEligibleSales_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync(
            $"/api/returns/eligible-sales?productId={_testProduct.Id}&pointOfSaleId={_testPos.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEligibleSales_SaleOutsideReturnWindow_ReturnsEmpty()
    {
        // Arrange - Create an old sale outside the 30-day window
        using var mother = new TestDataMother(_factory.Services);
        var oldSale = await mother.Sale()
            .WithProduct(_testProduct.Id)
            .WithPointOfSale(_testPos.Id)
            .WithPaymentMethod(_testPaymentMethod.Id)
            .WithUser(_testOperator.Id)
            .WithQuantity(3)
            .WithUnitPrice(150.00m)
            .WithSaleDate(DateTime.UtcNow.AddDays(-35)) // Outside 30-day window
            .CreateAsync();

        // Act - Query for a product that only has old sales
        var newProduct = await mother.Product()
            .WithSku("OLD-SALE-PROD")
            .WithName("Old Sale Product")
            .WithPrice(100m)
            .CreateAsync();

        // Create inventory and old sale for the new product
        await mother.Inventory()
            .WithProduct(newProduct.Id)
            .WithPointOfSale(_testPos.Id)
            .WithQuantity(10)
            .CreateAsync();

        await mother.Sale()
            .WithProduct(newProduct.Id)
            .WithPointOfSale(_testPos.Id)
            .WithPaymentMethod(_testPaymentMethod.Id)
            .WithUser(_testOperator.Id)
            .WithQuantity(2)
            .WithUnitPrice(100m)
            .WithSaleDate(DateTime.UtcNow.AddDays(-40)) // Way outside window
            .CreateAsync();

        var response = await _operatorClient!.GetAsync(
            $"/api/returns/eligible-sales?productId={newProduct.Id}&pointOfSaleId={_testPos.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<EligibleSalesResponse>();
        result!.EligibleSales.Should().BeEmpty();
        result.TotalAvailableForReturn.Should().Be(0);
    }

    #endregion

    #region Create Return Tests

    [Fact]
    public async Task CreateReturn_ValidRequest_ReturnsCreatedReturn()
    {
        // Arrange
        var createRequest = new CreateReturnRequest
        {
            ProductId = _testProduct.Id,
            PointOfSaleId = _testPos.Id,
            Quantity = 2,
            Category = ReturnCategory.Defectuoso,
            Reason = "Product had defects",
            SaleAssociations = new List<SaleAssociationRequest>
            {
                new() { SaleId = _testSale.Id, Quantity = 2 }
            }
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/returns", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CreateReturnResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Return.Should().NotBeNull();
        result.Return!.Quantity.Should().Be(2);
        result.Return.Category.Should().Be(ReturnCategory.Defectuoso);
        result.Return.TotalValue.Should().Be(300.00m); // 2 * 150

        // Verify database state
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var returnEntity = await context.Returns
            .Include(r => r.ReturnSales)
            .FirstOrDefaultAsync(r => r.Id == result.Return.Id);
        
        returnEntity.Should().NotBeNull();
        returnEntity!.Quantity.Should().Be(2);
        returnEntity.ReturnSales.Should().HaveCount(1);
        returnEntity.ReturnSales.First().Quantity.Should().Be(2);
        returnEntity.ReturnSales.First().UnitPrice.Should().Be(150.00m);

        // Verify inventory was updated (stock increased)
        var inventory = await context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == _testProduct.Id && i.PointOfSaleId == _testPos.Id);
        
        // Initial: 20, Sale: -5, Return: +2 = 17
        inventory!.Quantity.Should().Be(17);

        // Verify inventory movement was created
        var movement = await context.InventoryMovements
            .Where(m => m.ReturnId == result.Return.Id)
            .FirstOrDefaultAsync();
        
        movement.Should().NotBeNull();
        movement!.MovementType.Should().Be(MovementType.Return);
        movement.QuantityChange.Should().Be(2);
    }

    [Fact]
    public async Task CreateReturn_ExceedingQuantity_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateReturnRequest
        {
            ProductId = _testProduct.Id,
            PointOfSaleId = _testPos.Id,
            Quantity = 10, // More than the 5 sold
            Category = ReturnCategory.Defectuoso,
            SaleAssociations = new List<SaleAssociationRequest>
            {
                new() { SaleId = _testSale.Id, Quantity = 10 }
            }
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/returns", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReturn_PartialReturn_ReducesAvailableForFutureReturns()
    {
        // Arrange - First return of 2 units
        var firstReturn = new CreateReturnRequest
        {
            ProductId = _testProduct.Id,
            PointOfSaleId = _testPos.Id,
            Quantity = 2,
            Category = ReturnCategory.TamañoIncorrecto,
            SaleAssociations = new List<SaleAssociationRequest>
            {
                new() { SaleId = _testSale.Id, Quantity = 2 }
            }
        };

        var firstResponse = await _operatorClient!.PostAsJsonAsync("/api/returns", firstReturn);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Check eligible sales after first return
        var eligibleResponse = await _operatorClient!.GetAsync(
            $"/api/returns/eligible-sales?productId={_testProduct.Id}&pointOfSaleId={_testPos.Id}");

        // Assert
        eligibleResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await eligibleResponse.Content.ReadFromJsonAsync<EligibleSalesResponse>();
        result!.EligibleSales.Should().HaveCount(1);
        result.EligibleSales[0].AvailableForReturn.Should().Be(3); // 5 - 2 = 3
        result.TotalAvailableForReturn.Should().Be(3);
    }

    [Fact]
    public async Task CreateReturn_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateReturnRequest
        {
            ProductId = _testProduct.Id,
            PointOfSaleId = _testPos.Id,
            Quantity = 1,
            Category = ReturnCategory.Defectuoso,
            SaleAssociations = new List<SaleAssociationRequest>
            {
                new() { SaleId = _testSale.Id, Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/returns", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Returns History Tests

    [Fact]
    public async Task GetReturnsHistory_WithExistingReturns_ReturnsPagedResults()
    {
        // Arrange - Create a return first
        var createRequest = new CreateReturnRequest
        {
            ProductId = _testProduct.Id,
            PointOfSaleId = _testPos.Id,
            Quantity = 1,
            Category = ReturnCategory.NoSatisfecho,
            Reason = "Customer not satisfied",
            SaleAssociations = new List<SaleAssociationRequest>
            {
                new() { SaleId = _testSale.Id, Quantity = 1 }
            }
        };
        await _operatorClient!.PostAsJsonAsync("/api/returns", createRequest);

        // Act
        var response = await _operatorClient!.GetAsync("/api/returns");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ReturnsHistoryResponse>();
        result.Should().NotBeNull();
        result!.Returns.Should().HaveCountGreaterThanOrEqualTo(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetReturnsHistory_WithFilters_ReturnsFilteredResults()
    {
        // Arrange - Create returns for different categories
        await _operatorClient!.PostAsJsonAsync("/api/returns", new CreateReturnRequest
        {
            ProductId = _testProduct.Id,
            PointOfSaleId = _testPos.Id,
            Quantity = 1,
            Category = ReturnCategory.Defectuoso,
            SaleAssociations = new List<SaleAssociationRequest> { new() { SaleId = _testSale.Id, Quantity = 1 } }
        });

        // Act - Filter by point of sale
        var response = await _operatorClient!.GetAsync($"/api/returns?pointOfSaleId={_testPos.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ReturnsHistoryResponse>();
        result!.Returns.Should().AllSatisfy(r => r.PointOfSaleId.Should().Be(_testPos.Id));
    }

    #endregion

    #region Get Return By Id Tests

    [Fact]
    public async Task GetReturnById_ExistingReturn_ReturnsReturnDetails()
    {
        // Arrange - Create a return first
        var createRequest = new CreateReturnRequest
        {
            ProductId = _testProduct.Id,
            PointOfSaleId = _testPos.Id,
            Quantity = 2,
            Category = ReturnCategory.Defectuoso,
            Reason = "Test return for details",
            SaleAssociations = new List<SaleAssociationRequest>
            {
                new() { SaleId = _testSale.Id, Quantity = 2 }
            }
        };
        var createResponse = await _operatorClient!.PostAsJsonAsync("/api/returns", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateReturnResult>();
        var returnId = createResult!.Return!.Id;

        // Act
        var response = await _operatorClient!.GetAsync($"/api/returns/{returnId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<ReturnDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(returnId);
        result.Quantity.Should().Be(2);
        result.Category.Should().Be(ReturnCategory.Defectuoso);
        result.Reason.Should().Be("Test return for details");
        result.AssociatedSales.Should().HaveCount(1);
        result.TotalValue.Should().Be(300.00m); // 2 * 150
    }

    [Fact]
    public async Task GetReturnById_NonExistentReturn_ReturnsNotFound()
    {
        // Act
        var response = await _operatorClient!.GetAsync($"/api/returns/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
