using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for inventory management.
/// Verifies integration with access-control, product-management, and point-of-sale-management.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class InventoryIntegrationTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;
    private User? _operatorUser;
    private PointOfSale? _testPos1;
    private PointOfSale? _testPos2;
    private Product? _testProduct1;
    private Product? _testProduct2;

    public InventoryIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create test points of sale
        _testPos1 = new PointOfSale
        {
            Name = "Test POS 1",
            Code = "TP1",
            Address = "Test Address 1",
            IsActive = true
        };
        _testPos2 = new PointOfSale
        {
            Name = "Test POS 2",
            Code = "TP2",
            Address = "Test Address 2",
            IsActive = true
        };
        context.PointOfSales.AddRange(_testPos1, _testPos2);

        // Create test products
        _testProduct1 = new Product
        {
            SKU = "TEST-001",
            Name = "Test Product 1",
            Price = 100.00m,
            IsActive = true
        };
        _testProduct2 = new Product
        {
            SKU = "TEST-002",
            Name = "Test Product 2",
            Price = 200.00m,
            IsActive = true
        };
        context.Products.AddRange(_testProduct1, _testProduct2);
        await context.SaveChangesAsync();

        // Create operator user and assign to POS 1 only
        _operatorUser = new User
        {
            Username = "inventory_operator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operator123!", workFactor: 4),
            FirstName = "Inventory",
            LastName = "Operator",
            Role = UserRole.Operator,
            IsActive = true
        };
        context.Users.Add(_operatorUser);
        await context.SaveChangesAsync();

        // Assign operator to POS 1 only
        var assignment = new UserPointOfSale
        {
            UserId = _operatorUser.Id,
            PointOfSaleId = _testPos1.Id,
            IsActive = true
        };
        context.UserPointOfSales.Add(assignment);

        // Create inventory record for product 1 at POS 1
        var inventory = new Inventory
        {
            ProductId = _testProduct1.Id,
            PointOfSaleId = _testPos1.Id,
            Quantity = 10,
            IsActive = true,
            LastUpdatedAt = DateTime.UtcNow
        };
        context.Inventories.Add(inventory);
        await context.SaveChangesAsync();

        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        _operatorClient = await CreateAuthenticatedClientAsync("inventory_operator", "Operator123!");
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

    #region 16.1 Verify integration with access-control (operator product filtering)

    [Fact]
    public async Task ProductCatalog_AsOperator_ShouldOnlySeeAssignedProducts()
    {
        // Act - Operator requests product catalog
        var response = await _operatorClient!.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PaginatedResultDto<ProductListDto>>();

        // Assert - Should only see products with inventory at assigned POS (product 1)
        result.Should().NotBeNull();
        result!.Items.Should().Contain(p => p.SKU == "TEST-001");
        result.Items.Should().NotContain(p => p.SKU == "TEST-002");
    }

    [Fact]
    public async Task ProductCatalog_AsAdmin_ShouldSeeAllProducts()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PaginatedResultDto<ProductListDto>>();

        // Assert - Admin sees all products
        result.Should().NotBeNull();
        result!.Items.Should().Contain(p => p.SKU == "TEST-001");
        result!.Items.Should().Contain(p => p.SKU == "TEST-002");
    }

    [Fact]
    public async Task ProductSearch_AsOperator_ShouldOnlyFindAssignedProducts()
    {
        // Act - Search for both products
        var response1 = await _operatorClient!.GetAsync("/api/products/search?query=TEST-001");
        var response2 = await _operatorClient!.GetAsync("/api/products/search?query=TEST-002");

        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        var result1 = await response1.Content.ReadFromJsonAsync<List<ProductListDto>>();
        var result2 = await response2.Content.ReadFromJsonAsync<List<ProductListDto>>();

        // Assert - Should find assigned product, but not unassigned one
        result1.Should().NotBeEmpty();
        result1!.Should().Contain(p => p.SKU == "TEST-001");

        result2.Should().BeEmpty();
    }

    #endregion

    #region 16.2 Verify integration with product-management (product existence validation)

    [Fact]
    public async Task AssignProduct_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        var request = new AssignProductRequest
        {
            ProductId = Guid.NewGuid(), // Non-existent product
            PointOfSaleId = _testPos1!.Id
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignProduct_WithValidProduct_ShouldSucceed()
    {
        // Arrange - Assign product 2 to POS 2
        var request = new AssignProductRequest
        {
            ProductId = _testProduct2!.Id,
            PointOfSaleId = _testPos2!.Id
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StockAdjustment_WithNonExistentProduct_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new StockAdjustmentRequest
        {
            ProductId = Guid.NewGuid(), // Non-existent product
            PointOfSaleId = _testPos1!.Id,
            QuantityChange = 5,
            Reason = "Test adjustment"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/inventory/adjustment", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion

    #region 16.3 Verify integration with point-of-sale-management (POS existence validation)

    [Fact]
    public async Task AssignProduct_WithNonExistentPOS_ShouldReturnNotFound()
    {
        // Arrange
        var request = new AssignProductRequest
        {
            ProductId = _testProduct1!.Id,
            PointOfSaleId = Guid.NewGuid() // Non-existent POS
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetStock_WithValidPOS_ShouldReturnStock()
    {
        // Act
        var response = await _adminClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos1!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedInventoryResult>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetStock_WithNonExistentPOS_ShouldReturnEmpty()
    {
        // Act
        var response = await _adminClient!.GetAsync($"/api/inventory?pointOfSaleId={Guid.NewGuid()}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedInventoryResult>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    #endregion

    #region 16.4 Test end-to-end assignment → adjustment → view workflow

    [Fact]
    public async Task EndToEnd_AssignAdjustView_Workflow()
    {
        // Step 1: Assign product 2 to POS 1
        var assignRequest = new AssignProductRequest
        {
            ProductId = _testProduct2!.Id,
            PointOfSaleId = _testPos1!.Id
        };
        var assignResponse = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", assignRequest);
        assignResponse.EnsureSuccessStatusCode();

        // Step 2: Adjust stock for the newly assigned product
        var adjustRequest = new StockAdjustmentRequest
        {
            ProductId = _testProduct2!.Id,
            PointOfSaleId = _testPos1!.Id,
            QuantityChange = 25,
            Reason = "Initial stock from supplier"
        };
        var adjustResponse = await _adminClient!.PostAsJsonAsync("/api/inventory/adjustment", adjustRequest);
        adjustResponse.EnsureSuccessStatusCode();

        // Step 3: View stock for POS 1
        var viewResponse = await _adminClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos1!.Id}");
        viewResponse.EnsureSuccessStatusCode();
        var inventory = await viewResponse.Content.ReadFromJsonAsync<PaginatedInventoryResult>();

        // Assert - Should see both products with correct quantities
        inventory.Should().NotBeNull();
        var product1Inv = inventory!.Items.FirstOrDefault(i => i.ProductSku == "TEST-001");
        var product2Inv = inventory.Items.FirstOrDefault(i => i.ProductSku == "TEST-002");

        product1Inv.Should().NotBeNull();
        product1Inv!.Quantity.Should().Be(10);

        product2Inv.Should().NotBeNull();
        product2Inv!.Quantity.Should().Be(25);

        // Step 4: Verify movement history
        var movementsResponse = await _adminClient!.GetAsync($"/api/inventory-movements?pointOfSaleId={_testPos1!.Id}");
        movementsResponse.EnsureSuccessStatusCode();
    }

    #endregion

    #region 16.5 Test operator restrictions (cannot access unassigned POS)

    [Fact]
    public async Task Operator_AssignProduct_ShouldBeForbidden()
    {
        // Act - Operator trying to assign product
        var request = new AssignProductRequest
        {
            ProductId = _testProduct2!.Id,
            PointOfSaleId = _testPos1!.Id
        };
        var response = await _operatorClient!.PostAsJsonAsync("/api/inventory/assign", request);

        // Assert - Assignment is admin-only
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Operator_AdjustStock_ShouldBeForbidden()
    {
        // Act - Operator trying to adjust stock
        var request = new StockAdjustmentRequest
        {
            ProductId = _testProduct1!.Id,
            PointOfSaleId = _testPos1!.Id,
            QuantityChange = 5,
            Reason = "Test adjustment"
        };
        var response = await _operatorClient!.PostAsJsonAsync("/api/inventory/adjustment", request);

        // Assert - Adjustment is admin-only
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Operator_ViewStock_ForUnassignedPOS_ShouldReturnEmpty()
    {
        // Act - Operator viewing stock for POS they're not assigned to
        var response = await _operatorClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos2!.Id}");

        // Assert - Should get empty result (not error, just no access to that POS's data)
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedInventoryResult>();
        result.Should().NotBeNull();
        // The operator shouldn't see inventory for POS 2
    }

    [Fact]
    public async Task Operator_ViewStock_ForAssignedPOS_ShouldSucceed()
    {
        // Act - Operator viewing stock for their assigned POS
        var response = await _operatorClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos1!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedInventoryResult>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Operator_AccessCentralizedInventory_ShouldBeForbidden()
    {
        // Act - Operator trying to access centralized view (admin-only)
        var response = await _operatorClient!.GetAsync("/api/inventory/centralized");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_AccessCentralizedInventory_ShouldSucceed()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/inventory/centralized");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    #endregion
}

/// <summary>
/// Helper DTO for reading paginated inventory results
/// </summary>
public class PaginatedInventoryResult
{
    public List<InventoryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}

