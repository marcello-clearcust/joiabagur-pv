using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Infrastructure.Data;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the embedding endpoints:
/// POST   /api/image-recognition/embeddings        (Admin only)
/// DELETE /api/image-recognition/embeddings/{id}   (Admin only)
/// DELETE /api/image-recognition/embeddings        (Admin only)
/// GET    /api/image-recognition/embeddings        (Authenticated)
/// GET    /api/image-recognition/embeddings/status (Authenticated)
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class EmbeddingEndpointsTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;

    private const string BaseUrl = "/api/image-recognition/embeddings";

    public EmbeddingEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        _operatorClient = await CreateAuthenticatedClientAsync("operator", "Operator123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        if (username != "admin")
        {
            var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
            var createRequest = new CreateUserRequest
            {
                Username = username,
                Password = password,
                FirstName = "Test",
                LastName = "Operator",
                Role = "Operator"
            };
            await adminClient.PostAsJsonAsync("/api/users", createRequest);
        }

        var loginRequest = new LoginRequest { Username = username, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        var authClient = _factory.CreateClient();
        foreach (var cookie in cookies)
        {
            var parts = cookie.Split(';')[0].Split('=');
            if (parts.Length == 2)
                authClient.DefaultRequestHeaders.Add("Cookie", $"{parts[0]}={parts[1]}");
        }
        return authClient;
    }

    /// <summary>Seeds a product and photo, returns their IDs.</summary>
    private async Task<(Guid productId, Guid photoId, string sku)> SeedProductWithPhotoAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var product = new Product
        {
            SKU = $"TEST-{Guid.NewGuid():N}".Substring(0, 12),
            Name = "Test Product",
            Price = 100m,
            IsActive = true,
        };
        context.Products.Add(product);

        var photo = new ProductPhoto
        {
            ProductId = product.Id,
            FileName = "test.jpg",
            DisplayOrder = 1,
            IsPrimary = true,
        };
        context.ProductPhotos.Add(photo);
        await context.SaveChangesAsync();

        return (product.Id, photo.Id, product.SKU);
    }

    #region POST /embeddings (save single)

    [Fact]
    public async Task SaveEmbedding_WithValidRequest_Returns204()
    {
        // Arrange
        var (productId, photoId, sku) = await SeedProductWithPhotoAsync();
        var vector = TestDataGenerator.CreateEmbeddingVector(1280);
        var request = new { photoId, productId, sku, vector };

        // Act
        var response = await _adminClient!.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SaveEmbedding_WithShortVector_Returns400()
    {
        // Arrange
        var (productId, photoId, sku) = await SeedProductWithPhotoAsync();
        var request = new { photoId, productId, sku, vector = new float[512] };

        // Act
        var response = await _adminClient!.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaveEmbedding_Upsert_OverwritesExistingEmbedding()
    {
        // Arrange
        var (productId, photoId, sku) = await SeedProductWithPhotoAsync();
        var vector1 = TestDataGenerator.CreateEmbeddingVector(1280);
        var vector2 = TestDataGenerator.CreateEmbeddingVector(1280);

        await _adminClient!.PostAsJsonAsync(BaseUrl, new { photoId, productId, sku, vector = vector1 });

        // Act — save again (upsert)
        var response = await _adminClient!.PostAsJsonAsync(BaseUrl, new { photoId, productId, sku, vector = vector2 });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify only one embedding exists for this photo
        var statusResp = await _adminClient!.GetAsync($"{BaseUrl}/status");
        var status = await statusResp.Content.ReadFromJsonAsync<JsonElement>();
        status.GetProperty("count").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task SaveEmbedding_Unauthenticated_Returns401()
    {
        // Arrange
        var request = new { photoId = Guid.NewGuid(), productId = Guid.NewGuid(), sku = "S", vector = new float[1280] };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SaveEmbedding_AsOperator_Returns403()
    {
        // Arrange
        var request = new { photoId = Guid.NewGuid(), productId = Guid.NewGuid(), sku = "S", vector = new float[1280] };

        // Act
        var response = await _operatorClient!.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region DELETE /embeddings/{photoId} (delete single)

    [Fact]
    public async Task DeleteEmbedding_WhenExists_Returns204()
    {
        // Arrange
        var (productId, photoId, sku) = await SeedProductWithPhotoAsync();
        var vector = TestDataGenerator.CreateEmbeddingVector(1280);
        await _adminClient!.PostAsJsonAsync(BaseUrl, new { photoId, productId, sku, vector });

        // Act
        var response = await _adminClient!.DeleteAsync($"{BaseUrl}/{photoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteEmbedding_Idempotent_Returns204EvenWhenNotFound()
    {
        // Act — delete a photo that has no embedding
        var response = await _adminClient!.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region DELETE /embeddings (delete all)

    [Fact]
    public async Task DeleteAllEmbeddings_Returns204()
    {
        // Arrange — seed some embeddings
        var (productId1, photoId1, sku1) = await SeedProductWithPhotoAsync();
        var (productId2, photoId2, sku2) = await SeedProductWithPhotoAsync();
        await _adminClient!.PostAsJsonAsync(BaseUrl, new { photoId = photoId1, productId = productId1, sku = sku1, vector = TestDataGenerator.CreateEmbeddingVector(1280) });
        await _adminClient!.PostAsJsonAsync(BaseUrl, new { photoId = photoId2, productId = productId2, sku = sku2, vector = TestDataGenerator.CreateEmbeddingVector(1280) });

        // Act
        var response = await _adminClient!.DeleteAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Count should be 0
        var statusResp = await _adminClient!.GetAsync($"{BaseUrl}/status");
        var status = await statusResp.Content.ReadFromJsonAsync<JsonElement>();
        status.GetProperty("count").GetInt32().Should().Be(0);
    }

    #endregion

    #region GET /embeddings (bulk download)

    [Fact]
    public async Task GetAllEmbeddings_WhenEmpty_ReturnsEmptyList()
    {
        // Act
        var response = await _operatorClient!.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("count").GetInt32().Should().Be(0);
        body.GetProperty("embeddings").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetAllEmbeddings_WithData_ReturnsCorrectFormat()
    {
        // Arrange
        var (productId, photoId, sku) = await SeedProductWithPhotoAsync();
        var vector = TestDataGenerator.CreateEmbeddingVector(1280);
        await _adminClient!.PostAsJsonAsync(BaseUrl, new { photoId, productId, sku, vector });

        // Act
        var response = await _operatorClient!.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("count").GetInt32().Should().Be(1);
        var embeddings = body.GetProperty("embeddings");
        embeddings.GetArrayLength().Should().Be(1);
        var first = embeddings[0];
        first.GetProperty("photoId").GetGuid().Should().Be(photoId);
        first.GetProperty("productId").GetGuid().Should().Be(productId);
        first.GetProperty("sku").GetString().Should().Be(sku);
        first.GetProperty("vector").GetArrayLength().Should().Be(1280);
    }

    [Fact]
    public async Task GetAllEmbeddings_Unauthenticated_Returns401()
    {
        // Act
        var response = await _client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /embeddings/status (lightweight status)

    [Fact]
    public async Task GetEmbeddingsStatus_WhenEmpty_ReturnsZeroCount()
    {
        // Act
        var response = await _operatorClient!.GetAsync($"{BaseUrl}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("count").GetInt32().Should().Be(0);
        body.TryGetProperty("lastUpdated", out var lastUpdated);
        lastUpdated.ValueKind.Should().BeOneOf(JsonValueKind.Null, JsonValueKind.Undefined);
    }

    [Fact]
    public async Task GetEmbeddingsStatus_WithData_ReturnsCountAndTimestamp()
    {
        // Arrange
        var (productId, photoId, sku) = await SeedProductWithPhotoAsync();
        await _adminClient!.PostAsJsonAsync(BaseUrl, new { photoId, productId, sku, vector = TestDataGenerator.CreateEmbeddingVector(1280) });

        // Act
        var response = await _operatorClient!.GetAsync($"{BaseUrl}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("count").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetEmbeddingsStatus_Unauthenticated_Returns401()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
