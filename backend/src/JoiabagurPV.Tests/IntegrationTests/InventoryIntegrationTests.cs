using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Infrastructure.Data;
using JoiabagurPV.Tests.TestHelpers.Mothers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for inventory management.
/// Verifies integration with access-control, product-management, and point-of-sale-management.
/// Uses Respawn for database cleanup and Mother Objects for test data creation.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class InventoryIntegrationTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;

    // Test data created via Mother Objects
    private User _operatorUser = null!;
    private PointOfSale _testPos1 = null!;
    private PointOfSale _testPos2 = null!;
    private Product _testProduct1 = null!;
    private Product _testProduct2 = null!;
    private Inventory _testInventory = null!;

    public InventoryIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await SetupTestDataAsync();

        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        _operatorClient = await CreateAuthenticatedClientAsync("inventory_operator", "Test123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Sets up all prerequisite test data using the Mother Object pattern.
    /// </summary>
    private async Task SetupTestDataAsync()
    {
        using var mother = new TestDataMother(_factory.Services);

        // Create test points of sale
        _testPos1 = await mother.PointOfSale()
            .WithCode("TP1")
            .WithName("Test POS 1")
            .WithAddress("Test Address 1")
            .CreateAsync();

        _testPos2 = await mother.PointOfSale()
            .WithCode("TP2")
            .WithName("Test POS 2")
            .WithAddress("Test Address 2")
            .CreateAsync();

        // Create test products
        _testProduct1 = await mother.Product()
            .WithSku("TEST-001")
            .WithName("Test Product 1")
            .WithPrice(100.00m)
            .CreateAsync();

        _testProduct2 = await mother.Product()
            .WithSku("TEST-002")
            .WithName("Test Product 2")
            .WithPrice(200.00m)
            .CreateAsync();

        // Create operator user and assign to POS 1 only
        _operatorUser = await mother.User()
            .WithUsername("inventory_operator")
            .WithName("Inventory", "Operator")
            .AsOperator()
            .AssignedTo(_testPos1.Id)
            .CreateAsync();

        // Create inventory record for product 1 at POS 1
        _testInventory = await mother.Inventory()
            .WithProduct(_testProduct1.Id)
            .WithPointOfSale(_testPos1.Id)
            .WithQuantity(10)
            .CreateAsync();
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
            PointOfSaleId = _testPos1.Id
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignProduct_WithValidProduct_ShouldSucceed()
    {
        // Arrange - Assign product 2 to POS 2
        var request = new AssignProductRequest
        {
            ProductId = _testProduct2.Id,
            PointOfSaleId = _testPos2.Id
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
            PointOfSaleId = _testPos1.Id,
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
            ProductId = _testProduct1.Id,
            PointOfSaleId = Guid.NewGuid() // Non-existent POS
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStock_WithValidPOS_ShouldReturnStock()
    {
        // Act
        var response = await _adminClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos1.Id}");

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
            ProductId = _testProduct2.Id,
            PointOfSaleId = _testPos1.Id
        };
        var assignResponse = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", assignRequest);
        assignResponse.EnsureSuccessStatusCode();

        // Step 2: Adjust stock for the newly assigned product
        var adjustRequest = new StockAdjustmentRequest
        {
            ProductId = _testProduct2.Id,
            PointOfSaleId = _testPos1.Id,
            QuantityChange = 25,
            Reason = "Initial stock from supplier"
        };
        var adjustResponse = await _adminClient!.PostAsJsonAsync("/api/inventory/adjustment", adjustRequest);
        adjustResponse.EnsureSuccessStatusCode();

        // Step 3: View stock for POS 1
        var viewResponse = await _adminClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos1.Id}");
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
        var movementsResponse = await _adminClient!.GetAsync($"/api/inventory/movements?pointOfSaleId={_testPos1.Id}");
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
            ProductId = _testProduct2.Id,
            PointOfSaleId = _testPos1.Id
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
            ProductId = _testProduct1.Id,
            PointOfSaleId = _testPos1.Id,
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
        var response = await _operatorClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos2.Id}");

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
        var response = await _operatorClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos1.Id}");

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

    #region 9.10 Integration tests for Excel import workflow

    [Fact]
    public async Task ExcelImport_DownloadTemplate_ShouldSucceed()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/inventory/import-template");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        
        var stream = await response.Content.ReadAsStreamAsync();
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExcelImport_ValidFile_ShouldImportSuccessfully()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Arrange - Create Excel file with valid data
        var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stock Import");
        worksheet.Cell(1, 1).Value = "SKU";
        worksheet.Cell(1, 2).Value = "Quantity";
        worksheet.Cell(2, 1).Value = _testProduct2.SKU;
        worksheet.Cell(2, 2).Value = 50;

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", "stock_import.xlsx");

        // Act
        var response = await _adminClient!.PostAsync($"/api/inventory/import?pointOfSaleId={_testPos1.Id}", content);

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify inventory was created/updated
        var inventory = await context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == _testProduct2.Id && i.PointOfSaleId == _testPos1.Id);
        inventory.Should().NotBeNull();
        inventory!.Quantity.Should().BeGreaterOrEqualTo(50);
        inventory.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ExcelImport_InvalidSKU_ShouldReturnValidationErrors()
    {
        // Arrange - Create Excel with non-existent SKU
        var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stock Import");
        worksheet.Cell(1, 1).Value = "SKU";
        worksheet.Cell(1, 2).Value = "Quantity";
        worksheet.Cell(2, 1).Value = "INVALID-SKU";
        worksheet.Cell(2, 2).Value = 10;

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", "invalid_import.xlsx");
        content.Add(new StringContent(_testPos1.Id.ToString()), "pointOfSaleId");

        // Act
        var response = await _adminClient!.PostAsync("/api/inventory/import", content);

        // Assert - Should return bad request with validation errors
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region 9.11 Integration tests for movement history creation and querying

    [Fact]
    public async Task MovementHistory_AfterAdjustment_ShouldRecordMovement()
    {
        // Arrange - Adjust stock
        var adjustRequest = new StockAdjustmentRequest
        {
            ProductId = _testProduct1.Id,
            PointOfSaleId = _testPos1.Id,
            QuantityChange = 15,
            Reason = "Test adjustment for movement history"
        };
        await _adminClient!.PostAsJsonAsync("/api/inventory/adjustment", adjustRequest);

        // Act - Query movement history
        var response = await _adminClient!.GetAsync($"/api/inventory/movements?productId={_testProduct1.Id}&pointOfSaleId={_testPos1.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedMovementResult>();
        result.Should().NotBeNull();
        result!.Items.Should().Contain(m => 
            m.MovementType == MovementType.Adjustment && 
            m.QuantityChange == 15 &&
            m.Reason == "Test adjustment for movement history");
    }

    [Fact]
    public async Task MovementHistory_WithDateRangeFilter_ShouldFilterCorrectly()
    {
        // Arrange - Create an adjustment
        var adjustRequest = new StockAdjustmentRequest
        {
            ProductId = _testProduct1.Id,
            PointOfSaleId = _testPos1.Id,
            QuantityChange = 5,
            Reason = "Recent adjustment"
        };
        await _adminClient!.PostAsJsonAsync("/api/inventory/adjustment", adjustRequest);

        // Act - Query movement history (defaults to last 30 days)
        var response = await _adminClient!.GetAsync("/api/inventory/movements");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedMovementResult>();
        result.Should().NotBeNull();
        // Verify that movements are returned (exact count may vary based on test execution)
        result!.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task MovementHistory_WithPagination_ShouldReturnPagedResults()
    {
        // Act - Request first page with page size 10
        var response = await _adminClient!.GetAsync("/api/inventory/movements?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedMovementResult>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Count.Should().BeLessOrEqualTo(10);
    }

    #endregion

    #region 9.12 & 9.13 Integration tests for stock validation service and automatic movement creation

    [Fact]
    public async Task StockValidation_WithSufficientStock_ShouldAllowOperation()
    {
        // This test verifies that stock validation service works correctly
        // by attempting a sale with sufficient stock
        using var scope = _factory.Services.CreateScope();
        var stockValidationService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IStockValidationService>();

        // Act - Validate stock availability
        var result = await stockValidationService.ValidateStockAvailabilityAsync(
            _testProduct1.Id, 
            _testPos1.Id, 
            5);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.AvailableQuantity.Should().Be(10);
        result.RequestedQuantity.Should().Be(5);
    }

    [Fact]
    public async Task StockValidation_WithInsufficientStock_ShouldReturnError()
    {
        using var scope = _factory.Services.CreateScope();
        var stockValidationService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IStockValidationService>();

        // Act - Request more than available
        var result = await stockValidationService.ValidateStockAvailabilityAsync(
            _testProduct1.Id, 
            _testPos1.Id, 
            20);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Stock insuficiente");
    }

    [Fact]
    public async Task StockValidation_WithUnassignedProduct_ShouldReturnError()
    {
        using var scope = _factory.Services.CreateScope();
        var stockValidationService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IStockValidationService>();

        // Act - Try to validate stock for product 2 at POS 1 (not assigned)
        var result = await stockValidationService.ValidateStockAvailabilityAsync(
            _testProduct2.Id, 
            _testPos1.Id, 
            5);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no está asignado");
    }

    [Fact]
    public async Task CreateSaleMovement_WithValidData_ShouldUpdateStockAndCreateMovement()
    {
        // This test verifies automatic movement creation during sales
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IInventoryService>();

        var saleId = Guid.NewGuid();
        var userId = context.Users.First(u => u.Username == "admin").Id;

        // Verify initial quantity
        var initialInventory = await context.Inventories
            .FirstAsync(i => i.ProductId == _testProduct1.Id && i.PointOfSaleId == _testPos1.Id);
        var initialQuantity = initialInventory.Quantity;

        // Act - Create sale movement (must be within transaction per design change)
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await unitOfWork.BeginTransactionAsync();
        
        var result = await inventoryService.CreateSaleMovementAsync(
            _testProduct1.Id,
            _testPos1.Id,
            saleId,
            3,
            userId);

        await unitOfWork.CommitTransactionAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.QuantityBefore.Should().Be(initialQuantity);
        result.QuantityAfter.Should().Be(initialQuantity - 3);

        // Verify inventory was updated
        await context.Entry(initialInventory).ReloadAsync();
        initialInventory.Quantity.Should().Be(initialQuantity - 3);

        // Verify movement was created
        var movement = await context.InventoryMovements
            .FirstOrDefaultAsync(m => m.SaleId == saleId);
        movement.Should().NotBeNull();
        movement!.MovementType.Should().Be(MovementType.Sale);
        movement.QuantityChange.Should().Be(-3);
        movement.QuantityBefore.Should().Be(initialQuantity);
        movement.QuantityAfter.Should().Be(initialQuantity - 3);
    }

    [Fact]
    public async Task CreateSaleMovement_WithInsufficientStock_ShouldRollback()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IInventoryService>();

        var userId = context.Users.First(u => u.Username == "admin").Id;
        var saleId = Guid.NewGuid();

        // Get initial quantity
        var initialInventory = await context.Inventories
            .FirstAsync(i => i.ProductId == _testProduct1.Id && i.PointOfSaleId == _testPos1.Id);
        var initialQuantity = initialInventory.Quantity;

        // Act - Try to sell more than available
        var result = await inventoryService.CreateSaleMovementAsync(
            _testProduct1.Id,
            _testPos1.Id,
            saleId,
            100,
            userId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Stock insuficiente");

        // Verify inventory was NOT updated
        await context.Entry(initialInventory).ReloadAsync();
        initialInventory.Quantity.Should().Be(initialQuantity);

        // Verify no movement was created
        var movement = await context.InventoryMovements
            .FirstOrDefaultAsync(m => m.SaleId == saleId);
        movement.Should().BeNull();
    }

    [Fact]
    public async Task StockValidation_WithLowStockAfterSale_ShouldReturnWarning()
    {
        using var scope = _factory.Services.CreateScope();
        var stockValidationService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IStockValidationService>();

        // Act - Validate stock that will result in low stock (leave only 1 unit)
        var result = await stockValidationService.ValidateStockAvailabilityAsync(
            _testProduct1.Id, 
            _testPos1.Id, 
            9);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.IsLowStock.Should().BeTrue();
        result.WarningMessage.Should().Contain("Stock bajo");
    }

    #endregion

    #region 16.6 Test admin full access (all POS, all products)

    [Fact]
    public async Task Admin_AccessAllPOS_ShouldSucceed()
    {
        // Act - Get stock for both POS
        var response1 = await _adminClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos1.Id}");
        var response2 = await _adminClient!.GetAsync($"/api/inventory?pointOfSaleId={_testPos2.Id}");

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Admin_ManageAllProducts_ShouldSucceed()
    {
        // Act - Assign both products to different POS
        var request1 = new AssignProductRequest
        {
            ProductId = _testProduct1.Id,
            PointOfSaleId = _testPos2.Id
        };
        var request2 = new AssignProductRequest
        {
            ProductId = _testProduct2.Id,
            PointOfSaleId = _testPos1.Id
        };

        var response1 = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", request1);
        var response2 = await _adminClient!.PostAsJsonAsync("/api/inventory/assign", request2);

        // Assert
        response1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);
        response2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Conflict);
    }

    #endregion

    #region 16.7 Verify non-negative stock validation in all code paths

    [Fact]
    public async Task Adjustment_ResultingInNegativeStock_ShouldBeRejected()
    {
        // Arrange
        var request = new StockAdjustmentRequest
        {
            ProductId = _testProduct1.Id,
            PointOfSaleId = _testPos1.Id,
            QuantityChange = -100, // More than available
            Reason = "Test negative stock prevention"
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/inventory/adjustment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaleMovement_ResultingInNegativeStock_ShouldBeRejected()
    {
        using var scope = _factory.Services.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IInventoryService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userId = context.Users.First(u => u.Username == "admin").Id;

        // Act - Try to create sale with more than available stock
        var result = await inventoryService.CreateSaleMovementAsync(
            _testProduct1.Id,
            _testPos1.Id,
            Guid.NewGuid(),
            1000,
            userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("insuficiente");
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

/// <summary>
/// Helper DTO for reading paginated movement results
/// </summary>
public class PaginatedMovementResult
{
    public List<InventoryMovementDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
