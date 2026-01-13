using FluentAssertions;
using JoiabagurPV.Application.DTOs.Auth;
using JoiabagurPV.Application.DTOs.ImageRecognition;
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
/// Integration tests for ImageRecognitionController.
/// Tests model metadata, health metrics, and training job management.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public class ImageRecognitionControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private HttpClient? _adminClient;
    private HttpClient? _operatorClient;

    public ImageRecognitionControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await ResetImageRecognitionTablesAsync();
        
        _adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
        _operatorClient = await CreateAuthenticatedClientAsync("operator", "Operator123!");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task ResetImageRecognitionTablesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.ExecuteSqlRawAsync(@"
            TRUNCATE TABLE ""ModelTrainingJobs"" CASCADE;
            TRUNCATE TABLE ""ModelMetadata"" CASCADE;
            TRUNCATE TABLE ""ProductPhotos"" CASCADE;
            TRUNCATE TABLE ""Products"" CASCADE;
        ");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        // Check if user exists, if not create (for operator)
        if (username != "admin")
        {
            var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin123!");
            var createRequest = new CreateUserRequest
            {
                Username = username,
                Password = password,
                FirstName = "Test",
                LastName = "User",
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
            var cookieParts = cookie.Split(';')[0].Split('=');
            if (cookieParts.Length == 2)
            {
                authClient.DefaultRequestHeaders.Add("Cookie", $"{cookieParts[0]}={cookieParts[1]}");
            }
        }

        return authClient;
    }

    #region Model Metadata Tests

    [Fact]
    public async Task GetModelMetadata_WhenNoModel_ReturnsNotFound()
    {
        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model/metadata");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("No AI model available");
    }

    [Fact]
    public async Task GetModelMetadata_WhenModelExists_ReturnsMetadata()
    {
        // Arrange - Create a model
        await CreateTestModelAsync();

        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model/metadata");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var metadata = await response.Content.ReadFromJsonAsync<ModelMetadataDto>();
        metadata.Should().NotBeNull();
        metadata!.Version.Should().Be("v1_20260112");
        metadata.IsActive.Should().BeTrue();
    }

    #endregion

    #region Model Health Tests

    [Fact]
    public async Task GetModelHealth_AsAdmin_ReturnsHealthMetrics()
    {
        // Arrange - Create test products and model
        await CreateTestProductsWithPhotosAsync();
        await CreateTestModelAsync();

        // Act
        var response = await _adminClient!.GetAsync("/api/image-recognition/model/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var health = await response.Content.ReadFromJsonAsync<ModelHealthDto>();
        health.Should().NotBeNull();
        health!.AlertLevel.Should().NotBeNullOrEmpty();
        health.CatalogMetrics.Should().NotBeNull();
        health.PhotoMetrics.Should().NotBeNull();
    }

    [Fact]
    public async Task GetModelHealth_AsOperator_ReturnsForbidden()
    {
        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetModelHealth_WhenNoModel_ShowsCriticalAlert()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/image-recognition/model/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var health = await response.Content.ReadFromJsonAsync<ModelHealthDto>();
        health.Should().NotBeNull();
        health!.AlertLevel.Should().Be("CRITICAL");
        health.AlertMessage.Should().Contain("No AI model exists");
    }

    #endregion

    #region Training Job Tests

    [Fact]
    public async Task InitiateRetraining_WithNoPhotos_ReturnsBadRequest()
    {
        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/retrain", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("No products have photos");
    }

    [Fact]
    public async Task InitiateRetraining_WithPhotos_ReturnsAccepted()
    {
        // Arrange - Create products with photos
        await CreateTestProductsWithPhotosAsync();

        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/retrain", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var jobId = doc.RootElement.GetProperty("jobId").GetGuid();
        
        jobId.Should().NotBeEmpty();

        // Verify job was created in database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var job = await context.ModelTrainingJobs.FindAsync(jobId);
        job.Should().NotBeNull();
        job!.Status.Should().Be("Queued");
    }

    [Fact]
    public async Task InitiateRetraining_WhenJobInProgress_ReturnsConflict()
    {
        // Arrange - Create products and a job in progress
        await CreateTestProductsWithPhotosAsync();
        await CreateInProgressJobAsync();

        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/retrain", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already in progress");
    }

    [Fact]
    public async Task InitiateRetraining_AsOperator_ReturnsForbidden()
    {
        // Arrange
        await CreateTestProductsWithPhotosAsync();

        // Act
        var response = await _operatorClient!.PostAsync("/api/image-recognition/retrain", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTrainingStatus_ExistingJob_ReturnsStatus()
    {
        // Arrange - Create a job
        var jobId = await CreateQueuedJobAsync();

        // Act
        var response = await _operatorClient!.GetAsync($"/api/image-recognition/retrain/status/{jobId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var status = await response.Content.ReadFromJsonAsync<TrainingJobStatusDto>();
        status.Should().NotBeNull();
        status!.JobId.Should().Be(jobId);
        status.Status.Should().Be("Queued");
    }

    [Fact]
    public async Task GetTrainingStatus_NonExistentJob_ReturnsNotFound()
    {
        // Act
        var response = await _operatorClient!.GetAsync($"/api/image-recognition/retrain/status/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Model File Tests

    [Fact]
    public async Task GetModel_WhenNoModel_ReturnsNotFound()
    {
        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetModel_WhenModelExists_ReturnsModelPath()
    {
        // Arrange
        await CreateTestModelAsync();

        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var modelPath = doc.RootElement.GetProperty("modelPath").GetString();
        var version = doc.RootElement.GetProperty("version").GetString();
        var modelJsonUrl = doc.RootElement.GetProperty("modelJsonUrl").GetString();
        
        modelPath.Should().NotBeNullOrEmpty();
        modelPath.Should().Contain("models/v1_20260112");
        version.Should().Be("v1_20260112");
        modelJsonUrl.Should().Contain("/api/image-recognition/model/files/v1_20260112/model.json");
    }

    [Fact]
    public async Task GetModelFile_WithValidFile_ReturnsFile()
    {
        // Arrange - Create model and mock files
        await CreateTestModelAsync();
        await CreateMockModelFilesAsync("v1_20260112");

        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model/files/v1_20260112/model.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetModelFile_WithInvalidFileName_ReturnsBadRequest()
    {
        // Arrange
        await CreateTestModelAsync();

        // Act - Try directory traversal in fileName parameter
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model/files/v1_20260112/..%2F..%2Fsecrets.txt");

        // Assert
        // Should be BadRequest or NotFound (both are acceptable for security)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetModelFile_WithNonExistentVersion_ReturnsNotFound()
    {
        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model/files/v999_20991231/model.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Model File Serving Tests

    [Fact]
    public async Task ModelFileServing_CachingHeaders_ShouldBeSet()
    {
        // Arrange
        await CreateTestModelAsync();
        await CreateMockModelFilesAsync("v1_20260112");

        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/model/files/v1_20260112/model.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl?.Public.Should().BeTrue();
        response.Headers.CacheControl?.MaxAge.Should().Be(TimeSpan.FromDays(1));
    }

    #endregion

    #region Browser-Trained Model Upload Tests

    [Fact]
    public async Task UploadTrainedModel_AsAdmin_WithValidData_ReturnsSuccess()
    {
        // Arrange - Create products with photos first
        await CreateTestProductsWithPhotosAsync();

        var modelTopology = System.Text.Json.JsonSerializer.Serialize(new
        {
            format = "graph-model",
            generatedBy = "tensorflow.js-2.0.0",
            modelTopology = new
            {
                node = new[] { new { name = "input" }, new { name = "output" } }
            },
            weightsManifest = new[]
            {
                new
                {
                    paths = new[] { "group1-shard1of1.bin" },
                    weights = new[] { new { name = "dense/kernel", dtype = "float32", shape = new[] { 128, 10 } } }
                }
            }
        });

        var weightData = new byte[128 * 10 * 4]; // Mock weight data
        for (int i = 0; i < weightData.Length; i++)
        {
            weightData[i] = (byte)(i % 256);
        }

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("v1_20260112_browser"), "Version");
        form.Add(new StringContent(modelTopology), "ModelTopologyJson");
        form.Add(new StringContent("85.5"), "TrainingAccuracy");
        form.Add(new StringContent("82.3"), "ValidationAccuracy");
        form.Add(new StringContent("10"), "TotalPhotosUsed");
        form.Add(new StringContent("5"), "TotalProductsUsed");
        form.Add(new StringContent("900"), "TrainingDurationSeconds");

        var weightFile = new ByteArrayContent(weightData);
        weightFile.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        form.Add(weightFile, "WeightFiles", "group1-shard1of1.bin");

        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/upload-trained-model", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<UploadTrainedModelResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Version.Should().Be("v1_20260112_browser");
        result.Metadata.Should().NotBeNull();
        result.Metadata!.IsActive.Should().BeTrue();

        // Verify model was saved to database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var savedModel = await context.ModelMetadata.FirstOrDefaultAsync(m => m.Version == "v1_20260112_browser");
        savedModel.Should().NotBeNull();
        savedModel!.IsActive.Should().BeTrue();
        savedModel.TotalPhotosUsed.Should().Be(10);
        savedModel.TotalProductsUsed.Should().Be(5);
        
        // Verify model files were saved
        var modelsPath = Path.Combine(Directory.GetCurrentDirectory(), "models", "v1_20260112_browser");
        Directory.Exists(modelsPath).Should().BeTrue();
        File.Exists(Path.Combine(modelsPath, "model.json")).Should().BeTrue();
        File.Exists(Path.Combine(modelsPath, "group1-shard1of1.bin")).Should().BeTrue();
    }

    [Fact]
    public async Task UploadTrainedModel_AsOperator_ReturnsForbidden()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("v1_test"), "Version");
        form.Add(new StringContent("{}"), "ModelTopologyJson");
        
        var weightFile = new ByteArrayContent(new byte[] { 0x00 });
        form.Add(weightFile, "WeightFiles", "weights.bin");

        // Act
        var response = await _operatorClient!.PostAsync("/api/image-recognition/upload-trained-model", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadTrainedModel_WithMissingVersion_ReturnsBadRequest()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("{}"), "ModelTopologyJson");
        
        var weightFile = new ByteArrayContent(new byte[] { 0x00 });
        form.Add(weightFile, "WeightFiles", "weights.bin");

        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/upload-trained-model", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("version is required");
    }

    [Fact]
    public async Task UploadTrainedModel_WithMissingWeightFiles_ReturnsBadRequest()
    {
        // Arrange
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("v1_test"), "Version");
        form.Add(new StringContent("{}"), "ModelTopologyJson");
        // No weight files added

        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/upload-trained-model", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("weight file is required");
    }

    [Fact]
    public async Task UploadTrainedModel_WithDuplicateVersion_ReturnsBadRequest()
    {
        // Arrange - Create existing model
        await CreateTestModelAsync();

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("v1_20260112"), "Version"); // Same version as existing
        form.Add(new StringContent("{}"), "ModelTopologyJson");
        
        var weightFile = new ByteArrayContent(new byte[] { 0x00 });
        form.Add(weightFile, "WeightFiles", "weights.bin");

        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/upload-trained-model", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("already exists");
    }

    [Fact]
    public async Task UploadTrainedModel_DeactivatesPreviousActiveModel()
    {
        // Arrange - Create existing active model
        await CreateTestModelAsync();
        await CreateTestProductsWithPhotosAsync();

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("v2_20260113"), "Version");
        form.Add(new StringContent("{}"), "ModelTopologyJson");
        form.Add(new StringContent("88.0"), "ValidationAccuracy");
        
        var weightFile = new ByteArrayContent(new byte[] { 0x00 });
        form.Add(weightFile, "WeightFiles", "weights.bin");

        // Act
        var response = await _adminClient!.PostAsync("/api/image-recognition/upload-trained-model", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify old model was deactivated
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var oldModel = await context.ModelMetadata.FirstOrDefaultAsync(m => m.Version == "v1_20260112");
        oldModel.Should().NotBeNull();
        oldModel!.IsActive.Should().BeFalse();
        
        var newModel = await context.ModelMetadata.FirstOrDefaultAsync(m => m.Version == "v2_20260113");
        newModel.Should().NotBeNull();
        newModel!.IsActive.Should().BeTrue();
    }

    #endregion

    #region Training Dataset Tests

    [Fact]
    public async Task GetTrainingDataset_AsAdmin_ReturnsDataset()
    {
        // Arrange - Create products with photos
        await CreateTestProductsWithPhotosAsync();

        // Act
        var response = await _adminClient!.GetAsync("/api/image-recognition/training-dataset");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dataset = await response.Content.ReadFromJsonAsync<TrainingDatasetResponse>();
        dataset.Should().NotBeNull();
        dataset!.TotalProducts.Should().Be(5); // 5 products created
        dataset.TotalPhotos.Should().Be(10); // 2 photos per product
        dataset.Photos.Should().HaveCount(10);
        dataset.ClassLabels.Should().HaveCount(5);
        
        // Verify photo structure
        var firstPhoto = dataset.Photos.First();
        firstPhoto.ProductId.Should().NotBeEmpty();
        firstPhoto.ProductSku.Should().NotBeNullOrEmpty();
        firstPhoto.ProductName.Should().NotBeNullOrEmpty();
        firstPhoto.PhotoId.Should().NotBeEmpty();
        firstPhoto.PhotoUrl.Should().StartWith("/api/products/");
        firstPhoto.PhotoUrl.Should().Contain("/photos/");
        firstPhoto.PhotoUrl.Should().EndWith("/file");
    }

    [Fact]
    public async Task GetTrainingDataset_AsOperator_ReturnsForbidden()
    {
        // Act
        var response = await _operatorClient!.GetAsync("/api/image-recognition/training-dataset");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTrainingDataset_WhenNoPhotos_ReturnsEmptyDataset()
    {
        // Act
        var response = await _adminClient!.GetAsync("/api/image-recognition/training-dataset");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dataset = await response.Content.ReadFromJsonAsync<TrainingDatasetResponse>();
        dataset.Should().NotBeNull();
        dataset!.TotalProducts.Should().Be(0);
        dataset.TotalPhotos.Should().Be(0);
        dataset.Photos.Should().BeEmpty();
        dataset.ClassLabels.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTrainingDataset_OnlyIncludesActiveProducts()
    {
        // Arrange - Create active and inactive products
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Active product with photo
        var activeProduct = new Product
        {
            SKU = "ACTIVE-001",
            Name = "Active Product",
            Price = 100m,
            IsActive = true
        };
        context.Products.Add(activeProduct);
        await context.SaveChangesAsync();

        var activePhoto = new ProductPhoto
        {
            ProductId = activeProduct.Id,
            FileName = "photo1.jpg",
            DisplayOrder = 1,
            IsPrimary = true
        };
        context.ProductPhotos.Add(activePhoto);

        // Inactive product with photo
        var inactiveProduct = new Product
        {
            SKU = "INACTIVE-001",
            Name = "Inactive Product",
            Price = 100m,
            IsActive = false
        };
        context.Products.Add(inactiveProduct);
        await context.SaveChangesAsync();

        var inactivePhoto = new ProductPhoto
        {
            ProductId = inactiveProduct.Id,
            FileName = "photo2.jpg",
            DisplayOrder = 1,
            IsPrimary = true
        };
        context.ProductPhotos.Add(inactivePhoto);

        await context.SaveChangesAsync();

        // Act
        var response = await _adminClient!.GetAsync("/api/image-recognition/training-dataset");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dataset = await response.Content.ReadFromJsonAsync<TrainingDatasetResponse>();
        dataset.Should().NotBeNull();
        dataset!.TotalProducts.Should().Be(1); // Only active product
        dataset.TotalPhotos.Should().Be(1);
        dataset.Photos.First().ProductSku.Should().Be("ACTIVE-001");
    }

    #endregion

    #region Helper Methods

    private async Task CreateTestModelAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var model = new ModelMetadata
        {
            Version = "v1_20260112",
            TrainedAt = DateTime.UtcNow.AddDays(-5),
            ModelPath = "models/v1_20260112",
            TotalPhotosUsed = 50,
            TotalProductsUsed = 10,
            IsActive = true
        };

        context.ModelMetadata.Add(model);
        await context.SaveChangesAsync();
    }

    private async Task CreateTestProductsWithPhotosAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create 5 products with photos
        for (int i = 1; i <= 5; i++)
        {
            var product = new Product
            {
                SKU = $"TEST-{i:D3}",
                Name = $"Test Product {i}",
                Price = 100m * i,
                IsActive = true
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Add 2 photos per product
            for (int j = 1; j <= 2; j++)
            {
                var photo = new ProductPhoto
                {
                    ProductId = product.Id,
                    FileName = $"photo{j}.jpg",
                    DisplayOrder = j,
                    IsPrimary = j == 1
                };
                context.ProductPhotos.Add(photo);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task<Guid> CreateQueuedJobAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Get admin user ID
        var adminUser = await context.Users.FirstAsync(u => u.Username == "admin");

        var job = new ModelTrainingJob
        {
            InitiatedBy = adminUser.Id,
            Status = "Queued",
            ProgressPercentage = 0,
            CurrentStage = "Queued"
        };

        context.ModelTrainingJobs.Add(job);
        await context.SaveChangesAsync();

        return job.Id;
    }

    private async Task CreateInProgressJobAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminUser = await context.Users.FirstAsync(u => u.Username == "admin");

        var job = new ModelTrainingJob
        {
            InitiatedBy = adminUser.Id,
            Status = "InProgress",
            ProgressPercentage = 45,
            CurrentStage = "Training epoch 9/20",
            StartedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        context.ModelTrainingJobs.Add(job);
        await context.SaveChangesAsync();
    }

    private async Task CreateMockModelFilesAsync(string version)
    {
        // Create mock model directory and files for testing file serving
        var modelsPath = Path.Combine(Directory.GetCurrentDirectory(), "models", version);
        Directory.CreateDirectory(modelsPath);

        // Create mock model.json
        var modelJson = new
        {
            format = "graph-model",
            generatedBy = "mock-test",
            modelTopology = new { },
            weightsManifest = new[]
            {
                new
                {
                    paths = new[] { "group1-shard1of1.bin" },
                    weights = new object[] { }
                }
            }
        };

        var modelJsonPath = Path.Combine(modelsPath, "model.json");
        await File.WriteAllTextAsync(modelJsonPath, System.Text.Json.JsonSerializer.Serialize(modelJson));

        // Create mock binary file
        var binPath = Path.Combine(modelsPath, "group1-shard1of1.bin");
        await File.WriteAllBytesAsync(binPath, new byte[] { 0x00, 0x01, 0x02, 0x03 });
    }

    #endregion
}
