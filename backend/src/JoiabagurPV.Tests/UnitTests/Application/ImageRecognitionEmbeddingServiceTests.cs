using System.Text.Json;
using FluentAssertions;
using JoiabagurPV.Application.DTOs.ImageRecognition;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Domain.Interfaces.Services;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for the embedding-related methods of ImageRecognitionService.
/// </summary>
public class ImageRecognitionEmbeddingServiceTests
{
    private readonly Mock<IProductPhotoEmbeddingRepository> _embeddingRepoMock;
    private readonly Mock<IModelMetadataRepository> _modelMetadataRepoMock;
    private readonly Mock<IModelTrainingJobRepository> _trainingJobRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IProductPhotoRepository> _productPhotoRepoMock;
    private readonly Mock<IInventoryRepository> _inventoryRepoMock;
    private readonly Mock<IUserPointOfSaleService> _userPosMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ImageRecognitionService>> _loggerMock;
    private readonly ImageRecognitionService _service;

    public ImageRecognitionEmbeddingServiceTests()
    {
        _embeddingRepoMock = new Mock<IProductPhotoEmbeddingRepository>();
        _modelMetadataRepoMock = new Mock<IModelMetadataRepository>();
        _trainingJobRepoMock = new Mock<IModelTrainingJobRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _productPhotoRepoMock = new Mock<IProductPhotoRepository>();
        _inventoryRepoMock = new Mock<IInventoryRepository>();
        _userPosMock = new Mock<IUserPointOfSaleService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ImageRecognitionService>>();

        _service = new ImageRecognitionService(
            _modelMetadataRepoMock.Object,
            _trainingJobRepoMock.Object,
            _productRepoMock.Object,
            _productPhotoRepoMock.Object,
            _embeddingRepoMock.Object,
            _inventoryRepoMock.Object,
            _userPosMock.Object,
            _fileStorageMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region SaveEmbeddingAsync Tests

    [Fact]
    public async Task SaveEmbeddingAsync_WithValidVector_ShouldUpsertAndSave()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var vector = TestDataGenerator.CreateEmbeddingVector(1280);
        var request = new SaveEmbeddingRequest
        {
            PhotoId = photoId,
            ProductId = productId,
            Sku = "SKU001",
            Vector = vector,
        };

        _embeddingRepoMock.Setup(r => r.DeleteByPhotoIdAsync(photoId)).Returns(Task.CompletedTask);
        _embeddingRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductPhotoEmbedding>())).ReturnsAsync((ProductPhotoEmbedding e) => e);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.SaveEmbeddingAsync(request);

        // Assert
        _embeddingRepoMock.Verify(r => r.DeleteByPhotoIdAsync(photoId), Times.Once);
        _embeddingRepoMock.Verify(r => r.AddAsync(It.Is<ProductPhotoEmbedding>(e =>
            e.ProductPhotoId == photoId &&
            e.ProductId == productId &&
            e.ProductSku == "SKU001")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveEmbeddingAsync_WithInvalidVectorLength_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new SaveEmbeddingRequest
        {
            PhotoId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Sku = "SKU001",
            Vector = new float[512], // wrong length
        };

        // Act
        var act = () => _service.SaveEmbeddingAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*1280*");
    }

    [Fact]
    public async Task SaveEmbeddingAsync_WithNullVector_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new SaveEmbeddingRequest
        {
            PhotoId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Sku = "SKU001",
            Vector = null!,
        };

        // Act
        var act = () => _service.SaveEmbeddingAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SaveEmbeddingAsync_ShouldSerializeVectorAsJson()
    {
        // Arrange
        var vector = TestDataGenerator.CreateEmbeddingVector(1280);
        var request = new SaveEmbeddingRequest
        {
            PhotoId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Sku = "SKU001",
            Vector = vector,
        };

        ProductPhotoEmbedding? savedEmbedding = null;
        _embeddingRepoMock.Setup(r => r.DeleteByPhotoIdAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _embeddingRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductPhotoEmbedding>()))
            .Callback<ProductPhotoEmbedding>(e => savedEmbedding = e)
            .ReturnsAsync((ProductPhotoEmbedding e) => e);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.SaveEmbeddingAsync(request);

        // Assert
        savedEmbedding.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<float[]>(savedEmbedding!.EmbeddingVector);
        deserialized.Should().HaveCount(1280);
        deserialized![0].Should().BeApproximately(vector[0], 0.0001f);
    }

    #endregion

    #region DeleteEmbeddingAsync Tests

    [Fact]
    public async Task DeleteEmbeddingAsync_ShouldDeleteAndSave()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        _embeddingRepoMock.Setup(r => r.DeleteByPhotoIdAsync(photoId)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.DeleteEmbeddingAsync(photoId);

        // Assert
        _embeddingRepoMock.Verify(r => r.DeleteByPhotoIdAsync(photoId), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region DeleteAllEmbeddingsAsync Tests

    [Fact]
    public async Task DeleteAllEmbeddingsAsync_ShouldDeleteAll()
    {
        // Arrange
        _embeddingRepoMock.Setup(r => r.DeleteAllAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAllEmbeddingsAsync();

        // Assert
        _embeddingRepoMock.Verify(r => r.DeleteAllAsync(), Times.Once);
    }

    #endregion

    #region GetAllEmbeddingsAsync Tests

    [Fact]
    public async Task GetAllEmbeddingsAsync_WithData_ShouldReturnDeserializedVectors()
    {
        // Arrange
        var photoId1 = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var vector1 = TestDataGenerator.CreateEmbeddingVector(1280);
        var updatedAt = DateTime.UtcNow.AddHours(-1);

        var embeddings = new List<ProductPhotoEmbedding>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductPhotoId = photoId1,
                ProductId = productId1,
                ProductSku = "SKU001",
                EmbeddingVector = JsonSerializer.Serialize(vector1),
                UpdatedAt = updatedAt,
            }
        };

        _embeddingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(embeddings);

        // Act
        var result = await _service.GetAllEmbeddingsAsync();

        // Assert
        result.Count.Should().Be(1);
        result.Embeddings.Should().HaveCount(1);
        result.Embeddings[0].PhotoId.Should().Be(photoId1);
        result.Embeddings[0].ProductId.Should().Be(productId1);
        result.Embeddings[0].Sku.Should().Be("SKU001");
        result.Embeddings[0].Vector.Should().HaveCount(1280);
        result.LastUpdated.Should().Be(updatedAt);
    }

    [Fact]
    public async Task GetAllEmbeddingsAsync_WithNoData_ShouldReturnEmptyResponse()
    {
        // Arrange
        _embeddingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ProductPhotoEmbedding>());

        // Act
        var result = await _service.GetAllEmbeddingsAsync();

        // Assert
        result.Count.Should().Be(0);
        result.Embeddings.Should().BeEmpty();
        result.LastUpdated.Should().BeNull();
    }

    [Fact]
    public async Task GetAllEmbeddingsAsync_ShouldReturnMaxUpdatedAt()
    {
        // Arrange
        var earlier = DateTime.UtcNow.AddHours(-2);
        var later = DateTime.UtcNow.AddHours(-1);

        var embeddings = new List<ProductPhotoEmbedding>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProductPhotoId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductSku = "SKU001",
                EmbeddingVector = JsonSerializer.Serialize(new float[1280]),
                UpdatedAt = earlier,
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProductPhotoId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductSku = "SKU002",
                EmbeddingVector = JsonSerializer.Serialize(new float[1280]),
                UpdatedAt = later,
            }
        };

        _embeddingRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(embeddings);

        // Act
        var result = await _service.GetAllEmbeddingsAsync();

        // Assert
        result.LastUpdated.Should().Be(later);
        result.Count.Should().Be(2);
    }

    #endregion

    #region GetEmbeddingsStatusAsync Tests

    [Fact]
    public async Task GetEmbeddingsStatusAsync_ShouldReturnCountAndLastUpdated()
    {
        // Arrange
        var lastUpdated = DateTime.UtcNow.AddHours(-1);
        _embeddingRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(42);
        _embeddingRepoMock.Setup(r => r.GetLastUpdatedAsync()).ReturnsAsync(lastUpdated);

        // Act
        var result = await _service.GetEmbeddingsStatusAsync();

        // Assert
        result.Count.Should().Be(42);
        result.LastUpdated.Should().Be(lastUpdated);
    }

    [Fact]
    public async Task GetEmbeddingsStatusAsync_WhenEmpty_ShouldReturnZeroCountAndNullTimestamp()
    {
        // Arrange
        _embeddingRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(0);
        _embeddingRepoMock.Setup(r => r.GetLastUpdatedAsync()).ReturnsAsync((DateTime?)null);

        // Act
        var result = await _service.GetEmbeddingsStatusAsync();

        // Assert
        result.Count.Should().Be(0);
        result.LastUpdated.Should().BeNull();
    }

    #endregion
}
