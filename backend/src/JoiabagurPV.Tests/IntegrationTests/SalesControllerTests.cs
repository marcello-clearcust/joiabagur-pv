using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.DTOs.PaymentMethods;
using JoiabagurPV.Application.DTOs.PointOfSales;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.DTOs.Sales;
using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for SalesController.
/// Tests sale creation, validation, authorization, stock updates, and transaction integrity.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class SalesControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;
    private Guid _testProductId;
    private Guid _testPointOfSaleId;
    private Guid _testPaymentMethodId;
    private Guid _operatorUserId;

    public SalesControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await ResetSalesTablesAsync();
        
        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        
        // Create an operator user
        var createOperatorRequest = new CreateUserRequest
        {
            Username = "salesoperator",
            Password = "Operator123!",
            FirstName = "Sales",
            LastName = "Operator",
            Role = "Operator"
        };
        var operatorResponse = await _adminClient.PostAsJsonAsync("/api/users", createOperatorRequest);
        var operatorDto = await operatorResponse.Content.ReadFromJsonAsync<UserDto>();
        _operatorUserId = operatorDto!.Id;
        
        _operatorClient = await CreateAuthenticatedClientAsync("salesoperator", "Operator123!");
        
        // Create test data: POS, Product, Payment Method, Inventory, and Assignments
        await SetupTestDataAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ResetSalesTablesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE ""SalePhotos"" CASCADE;
            TRUNCATE TABLE ""Sales"" CASCADE;
            TRUNCATE TABLE ""InventoryMovements"" CASCADE;
            TRUNCATE TABLE ""Inventories"" CASCADE;
            TRUNCATE TABLE ""ProductPhotos"" CASCADE;
            TRUNCATE TABLE ""Products"" CASCADE;
            TRUNCATE TABLE ""PointOfSalePaymentMethods"" CASCADE;
            TRUNCATE TABLE ""UserPointOfSales"" CASCADE;
            TRUNCATE TABLE ""PaymentMethods"" CASCADE;
            TRUNCATE TABLE ""PointOfSales"" CASCADE;
        ");
    }

    private async Task SetupTestDataAsync()
    {
        // Create Point of Sale
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Code = "TEST-POS",
            Name = "Test Point of Sale",
            Address = "Test Address"
        };
        var posResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var posDto = await posResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();
        _testPointOfSaleId = posDto!.Id;

        // Create Payment Method
        var createPaymentMethodRequest = new CreatePaymentMethodRequest
        {
            Code = "CASH",
            Name = "Cash",
            Description = "Cash payment"
        };
        var pmResponse = await _adminClient!.PostAsJsonAsync("/api/payment-methods", createPaymentMethodRequest);
        var pmDto = await pmResponse.Content.ReadFromJsonAsync<PaymentMethodDto>();
        _testPaymentMethodId = pmDto!.Id;

        // Assign Payment Method to POS
        await _adminClient!.PostAsync($"/api/point-of-sales/{_testPointOfSaleId}/payment-methods/{_testPaymentMethodId}", null);

        // Assign Operator to POS
        await _adminClient!.PostAsync($"/api/users/{_operatorUserId}/point-of-sales/{_testPointOfSaleId}", null);

        // Create Product
        var createProductRequest = new CreateProductRequest
        {
            SKU = "TEST-SKU-001",
            Name = "Test Product",
            Description = "Test product for sales",
            Price = 100.00m
        };
        var productResponse = await _adminClient!.PostAsJsonAsync("/api/products", createProductRequest);
        var productDto = await productResponse.Content.ReadFromJsonAsync<ProductDto>();
        _testProductId = productDto!.Id;

        // Assign Product to POS
        var assignRequest = new AssignProductRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId
        };
        var assignResponse = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", assignRequest);
        assignResponse.EnsureSuccessStatusCode();
        
        // Add initial stock via stock adjustment
        var stockAdjustment = new StockAdjustmentRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            QuantityChange = 10,
            Reason = "Initial stock for tests"
        };
        var adjustResponse = await _adminClient!.PostAsJsonAsync("/api/inventory/adjustment", stockAdjustment);
        adjustResponse.EnsureSuccessStatusCode();
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

    #region Create Sale Tests

    [Fact]
    public async Task CreateSale_ValidRequest_ReturnsCreatedSale()
    {
        // Arrange
        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 2,
            Notes = "Test sale"
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var sale = await context.Sales
            .Include(s => s.InventoryMovement)
            .FirstOrDefaultAsync();
        
        sale.Should().NotBeNull();
        sale!.ProductId.Should().Be(_testProductId);
        sale.Quantity.Should().Be(2);
        sale.Price.Should().Be(100.00m);
        sale.InventoryMovement.Should().NotBeNull();
        sale.InventoryMovement!.MovementType.Should().Be(Domain.Enums.MovementType.Sale);
        sale.InventoryMovement.QuantityChange.Should().Be(-2);
    }

    [Fact]
    public async Task CreateSale_InsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 20, // More than available (10)
            Notes = "Test sale with insufficient stock"
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("insuficiente"); // Spanish error message
    }

    [Fact]
    public async Task CreateSale_InvalidPaymentMethod_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = Guid.NewGuid(), // Non-existent payment method
            Quantity = 1
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_OperatorNotAssignedToPOS_ReturnsBadRequest()
    {
        // Arrange - Create another POS without assigning operator
        var createPosRequest = new CreatePointOfSaleRequest
        {
            Code = "OTHER-POS",
            Name = "Other POS",
            Address = "Other Address"
        };
        var posResponse = await _adminClient!.PostAsJsonAsync("/api/point-of-sales", createPosRequest);
        var posDto = await posResponse.Content.ReadFromJsonAsync<PointOfSaleDto>();

        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = posDto!.Id,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("not assigned");
    }

    [Fact]
    public async Task CreateSale_ZeroQuantity_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 0
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSale_UpdatesInventory_Atomically()
    {
        // Arrange
        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 3
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Verify inventory was updated
        var inventory = await context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
        
        inventory.Should().NotBeNull();
        inventory!.Quantity.Should().Be(7); // 10 - 3

        // Verify movement was created (find the sale movement, not the adjustment from setup)
        var movement = await context.InventoryMovements
            .Where(m => m.InventoryId == inventory.Id && m.MovementType == Domain.Enums.MovementType.Sale)
            .FirstOrDefaultAsync();
        
        movement.Should().NotBeNull();
        movement!.MovementType.Should().Be(Domain.Enums.MovementType.Sale);
        movement.QuantityChange.Should().Be(-3);
        movement.QuantityBefore.Should().Be(10);
        movement.QuantityAfter.Should().Be(7);
    }

    #endregion

    #region Get Sale Tests

    [Fact]
    public async Task GetSaleById_ExistingSale_ReturnsSale()
    {
        // Arrange - Create a sale first
        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1
        };
        var createResponse = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        // Parse the response to get the sale ID
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        var saleId = doc.RootElement.GetProperty("sale").GetProperty("id").GetGuid();

        // Act
        var response = await _operatorClient!.GetAsync($"/api/sales/{saleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var sale = await response.Content.ReadFromJsonAsync<SaleDto>();
        sale.Should().NotBeNull();
        sale!.Id.Should().Be((Guid)saleId);
        sale.ProductId.Should().Be(_testProductId);
        sale.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task GetSaleById_NonExistentSale_ReturnsNotFound()
    {
        // Act
        var response = await _operatorClient!.GetAsync($"/api/sales/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Sales History Tests

    [Fact]
    public async Task GetSalesHistory_ReturnsOperatorSales()
    {
        // Arrange - Create multiple sales
        for (int i = 0; i < 3; i++)
        {
            var createRequest = new CreateSaleRequest
            {
                ProductId = _testProductId,
                PointOfSaleId = _testPointOfSaleId,
                PaymentMethodId = _testPaymentMethodId,
                Quantity = 1
            };
            await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);
        }

        // Act
        var response = await _operatorClient!.GetAsync("/api/sales");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var history = await response.Content.ReadFromJsonAsync<SalesHistoryResponse>();
        history.Should().NotBeNull();
        history!.Sales.Should().HaveCount(3);
        history.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetSalesHistory_WithFilters_ReturnsFilteredResults()
    {
        // Arrange - Create sales
        await _operatorClient!.PostAsJsonAsync("/api/sales", new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1
        });

        // Act - Filter by product
        var response = await _operatorClient!.GetAsync($"/api/sales?productId={_testProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var history = await response.Content.ReadFromJsonAsync<SalesHistoryResponse>();
        history.Should().NotBeNull();
        history!.Sales.Should().HaveCount(1);
        history.Sales[0].ProductId.Should().Be(_testProductId);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task CreateSale_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Low Stock Warning Tests

    [Fact]
    public async Task CreateSale_WhenStockBecomesLow_ReturnsLowStockWarning()
    {
        // Arrange - Set inventory to 6 units
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inventory = await context.Inventories
                .FirstAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
            inventory.Quantity = 6;
            await context.SaveChangesAsync();
        }

        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 2 // After sale, 4 units remain (< 5 threshold)
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("isLowStock");
        content.Should().Contain("\"isLowStock\":true", "low stock warning should be present");
        content.Should().Contain("\"remainingStock\":4", "remaining stock should be 4");
    }

    [Fact]
    public async Task CreateSale_WhenStockAboveThreshold_NoWarning()
    {
        // Arrange - Set inventory to 100 units
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inventory = await context.Inventories
                .FirstAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
            inventory.Quantity = 100;
            await context.SaveChangesAsync();
        }

        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1 // 99 units remain (well above threshold)
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"isLowStock\":false", "no low stock warning should be present");
    }

    [Fact]
    public async Task CreateSale_SellingLastUnit_ReturnsWarning()
    {
        // Arrange - Set inventory to 1 unit
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inventory = await context.Inventories
                .FirstAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
            inventory.Quantity = 1;
            await context.SaveChangesAsync();
        }

        var createRequest = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1 // Last unit
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/sales", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"isLowStock\":true");
        content.Should().Contain("\"remainingStock\":0");
    }

    #endregion

    #region Concurrent Sales Tests

    [Fact]
    public async Task TwoOperators_SellingLastUnit_OnlyOneShouldSucceed()
    {
        // Arrange - Set inventory to 1 unit
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inventory = await context.Inventories
                .FirstAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
            inventory.Quantity = 1;
            await context.SaveChangesAsync();
        }

        // Create second operator client
        var operator2Client = await CreateAuthenticatedClientAsync("operator2", "Operator123!");

        var request1 = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1
        };

        var request2 = new CreateSaleRequest
        {
            ProductId = _testProductId,
            PointOfSaleId = _testPointOfSaleId,
            PaymentMethodId = _testPaymentMethodId,
            Quantity = 1
        };

        // Act - Simulate concurrent requests
        var task1 = _operatorClient!.PostAsJsonAsync("/api/sales", request1);
        var task2 = operator2Client.PostAsJsonAsync("/api/sales", request2);

        var responses = await Task.WhenAll(task1, task2);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var failureCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        // Exactly one should succeed
        successCount.Should().Be(1, "only one sale should succeed when selling the last unit");
        failureCount.Should().Be(1, "one sale should fail due to insufficient stock");

        // Verify final inventory is 0
        using var finalScope = _factory.Services.CreateScope();
        var finalContext = finalScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var finalInventory = await finalContext.Inventories
            .FirstAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
        
        finalInventory.Quantity.Should().Be(0, "final quantity should be 0 after selling last unit");
    }

    [Fact]
    public async Task MultipleOperators_SellingConcurrently_MaintainDataConsistency()
    {
        // Arrange - Set inventory to 10 units
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inventory = await context.Inventories
                .FirstAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
            inventory.Quantity = 10;
            await context.SaveChangesAsync();
        }

        // Create 10 concurrent requests
        var tasks = new List<Task<HttpResponseMessage>>();
        
        for (int i = 0; i < 10; i++)
        {
            var request = new CreateSaleRequest
            {
                ProductId = _testProductId,
                PointOfSaleId = _testPointOfSaleId,
                PaymentMethodId = _testPaymentMethodId,
                Quantity = 1
            };

            tasks.Add(_operatorClient!.PostAsJsonAsync("/api/sales", request));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        
        // All should succeed (or some might fail due to timing, but total sales should be correct)
        successCount.Should().BeGreaterThan(0);

        // Verify final inventory + total sales = 10
        using var finalScope2 = _factory.Services.CreateScope();
        var finalContext2 = finalScope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var finalInventory = await finalContext2.Inventories
            .FirstAsync(i => i.ProductId == _testProductId && i.PointOfSaleId == _testPointOfSaleId);
        
        var sales = await finalContext2.Sales
            .Where(s => s.ProductId == _testProductId)
            .ToListAsync();
        
        var totalSold = sales.Sum(s => s.Quantity);
        
        // Total sold + remaining inventory should equal starting inventory
        (totalSold + finalInventory.Quantity).Should().Be(10, "inventory consistency must be maintained");
        
        // Inventory should never be negative
        finalInventory.Quantity.Should().BeGreaterThanOrEqualTo(0, "inventory cannot be negative");
    }

    #endregion
}
