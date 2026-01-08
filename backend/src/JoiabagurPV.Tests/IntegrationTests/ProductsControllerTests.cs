using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for ProductsController.
/// Tests product creation, validation, authorization, and database state verification.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class ProductsControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;

    public ProductsControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        
        // Also reset product tables (not included in base ResetDatabaseAsync)
        await ResetProductTablesAsync();
        
        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        
        // Create an operator user for authorization tests
        var createOperatorRequest = new CreateUserRequest
        {
            Username = "productoperator",
            Password = "Operator123!",
            FirstName = "Product",
            LastName = "Operator",
            Role = "Operator"
        };
        await _adminClient.PostAsJsonAsync("/api/users", createOperatorRequest);
        
        _operatorClient = await CreateAuthenticatedClientAsync("productoperator", "Operator123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ResetProductTablesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE ""ProductPhotos"" CASCADE;
            TRUNCATE TABLE ""Products"" CASCADE;
            TRUNCATE TABLE ""Collections"" CASCADE;
        ");
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

    private async Task<Collection> CreateCollectionAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Test collection: {name}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        context.Collections.Add(collection);
        await context.SaveChangesAsync();
        
        return collection;
    }

    #region Create Product - Success Tests

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreatedProduct()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "JOY-001",
            Name = "Gold Ring 18K",
            Description = "Beautiful 18 karat gold ring",
            Price = 299.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.SKU.Should().Be("JOY-001");
        product.Name.Should().Be("Gold Ring 18K");
        product.Description.Should().Be("Beautiful 18 karat gold ring");
        product.Price.Should().Be(299.99m);
        product.IsActive.Should().BeTrue();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Create_WithMinimalData_ShouldReturnCreatedProduct()
    {
        // Arrange - Only required fields
        var createRequest = new CreateProductRequest
        {
            SKU = "MIN-001",
            Name = "Minimal Product",
            Price = 49.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.SKU.Should().Be("MIN-001");
        product.Name.Should().Be("Minimal Product");
        product.Description.Should().BeNull();
        product.CollectionId.Should().BeNull();
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithCollection_ShouldAssignProductToCollection()
    {
        // Arrange
        var collection = await CreateCollectionAsync("Summer 2024");
        
        var createRequest = new CreateProductRequest
        {
            SKU = "SUM-001",
            Name = "Summer Ring",
            Description = "Beautiful summer collection ring",
            Price = 199.99m,
            CollectionId = collection.Id
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.CollectionId.Should().Be(collection.Id);
        product.CollectionName.Should().Be("Summer 2024");
    }

    [Fact]
    public async Task Create_ShouldPersistToDatabase()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "DB-001",
            Name = "Database Test Product",
            Price = 99.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();

        // Assert - Verify in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var dbProduct = await context.Products.FindAsync(created!.Id);
        dbProduct.Should().NotBeNull();
        dbProduct!.SKU.Should().Be("DB-001");
        dbProduct.Name.Should().Be("Database Test Product");
        dbProduct.Price.Should().Be(99.99m);
        dbProduct.IsActive.Should().BeTrue();
        dbProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dbProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Create Product - Validation Tests

    [Fact]
    public async Task Create_WithMissingSku_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new
        {
            Name = "Test Product",
            Price = 99.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithEmptySku_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "",
            Name = "Test Product",
            Price = 99.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithMissingName_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new
        {
            SKU = "TEST-001",
            Price = 99.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "TEST-001",
            Name = "",
            Price = 99.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithZeroPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "ZERO-001",
            Name = "Zero Price Product",
            Price = 0m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNegativePrice_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "NEG-001",
            Name = "Negative Price Product",
            Price = -10.00m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Create Product - SKU Uniqueness Tests

    [Fact]
    public async Task Create_WithDuplicateSku_ShouldReturnConflict()
    {
        // Arrange - Create first product
        var firstRequest = new CreateProductRequest
        {
            SKU = "DUP-001",
            Name = "First Product",
            Price = 99.99m
        };
        await _adminClient!.PostAsJsonAsync("/api/products", firstRequest);

        // Arrange - Try to create second product with same SKU
        var duplicateRequest = new CreateProductRequest
        {
            SKU = "DUP-001",
            Name = "Duplicate Product",
            Price = 199.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_WithDifferentCaseSku_ShouldReturnConflict()
    {
        // Arrange - Create first product with lowercase
        var firstRequest = new CreateProductRequest
        {
            SKU = "case-001",
            Name = "First Product",
            Price = 99.99m
        };
        await _adminClient!.PostAsJsonAsync("/api/products", firstRequest);

        // Arrange - Try with uppercase (should still conflict)
        var duplicateRequest = new CreateProductRequest
        {
            SKU = "CASE-001",
            Name = "Uppercase Product",
            Price = 199.99m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region Create Product - Collection Tests

    [Fact]
    public async Task Create_WithNonExistentCollection_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "COL-001",
            Name = "Collection Test",
            Price = 99.99m,
            CollectionId = Guid.NewGuid() // Non-existent collection
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNullCollection_ShouldSucceed()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "NOCOL-001",
            Name = "No Collection Product",
            Price = 99.99m,
            CollectionId = null
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product!.CollectionId.Should().BeNull();
    }

    #endregion

    #region Create Product - Authorization Tests

    [Fact]
    public async Task Create_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "UNAUTH-001",
            Name = "Unauthorized Product",
            Price = 99.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsOperator_ShouldReturnForbidden()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "OP-001",
            Name = "Operator Product",
            Price = 99.99m
        };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync("/api/products", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Get Products Tests

    [Fact]
    public async Task GetAll_AsAdmin_ShouldReturnProducts()
    {
        // Arrange - Create some products
        var products = new[]
        {
            new CreateProductRequest { SKU = "GET-001", Name = "Product 1", Price = 100m },
            new CreateProductRequest { SKU = "GET-002", Name = "Product 2", Price = 200m },
            new CreateProductRequest { SKU = "GET-003", Name = "Product 3", Price = 300m }
        };

        foreach (var product in products)
        {
            await _adminClient!.PostAsJsonAsync("/api/products", product);
        }

        // Act
        var response = await _adminClient!.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        result.Should().HaveCount(3);
        result.Should().Contain(p => p.SKU == "GET-001");
        result.Should().Contain(p => p.SKU == "GET-002");
        result.Should().Contain(p => p.SKU == "GET-003");
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "GETID-001",
            Name = "Get By ID Test",
            Price = 150m
        };
        var createResponse = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();

        // Act
        var response = await _adminClient!.GetAsync($"/api/products/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.Id.Should().Be(created.Id);
        product.SKU.Should().Be("GETID-001");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _adminClient!.GetAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBySku_WithValidSku_ShouldReturnProduct()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "SKU-TEST-001",
            Name = "SKU Test Product",
            Price = 175m
        };
        await _adminClient!.PostAsJsonAsync("/api/products", createRequest);

        // Act
        var response = await _adminClient!.GetAsync("/api/products/by-sku/SKU-TEST-001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.SKU.Should().Be("SKU-TEST-001");
    }

    [Fact]
    public async Task GetBySku_WithInvalidSku_ShouldReturnNotFound()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/products/by-sku/NONEXISTENT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Database State Verification Tests

    [Fact]
    public async Task Create_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            SKU = "ACTIVE-001",
            Name = "Active Test",
            Price = 50m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();

        // Assert
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var dbProduct = await context.Products.FindAsync(created!.Id);
        dbProduct!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ShouldSetAuditTimestamps()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow.AddSeconds(-1);
        
        var createRequest = new CreateProductRequest
        {
            SKU = "AUDIT-001",
            Name = "Audit Test",
            Price = 75m
        };

        // Act
        var response = await _adminClient!.PostAsJsonAsync("/api/products", createRequest);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        
        var afterCreate = DateTime.UtcNow.AddSeconds(1);

        // Assert
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var dbProduct = await context.Products.FindAsync(created!.Id);
        dbProduct!.CreatedAt.Should().BeAfter(beforeCreate);
        dbProduct.CreatedAt.Should().BeBefore(afterCreate);
        dbProduct.UpdatedAt.Should().BeAfter(beforeCreate);
        dbProduct.UpdatedAt.Should().BeBefore(afterCreate);
    }

    #endregion
}

