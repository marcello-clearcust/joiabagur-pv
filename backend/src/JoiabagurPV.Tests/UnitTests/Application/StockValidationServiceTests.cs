using FluentAssertions;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for StockValidationService.
/// </summary>
public class StockValidationServiceTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<ILogger<StockValidationService>> _loggerMock;
    private readonly StockValidationService _sut;

    public StockValidationServiceTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _loggerMock = new Mock<ILogger<StockValidationService>>();

        _sut = new StockValidationService(
            _inventoryRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ValidateStockAvailability_WithSufficientStock_ShouldReturnValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();
        var inventory = TestDataGenerator.CreateInventory(
            productId: productId,
            pointOfSaleId: posId,
            isActive: true,
            quantity: 100);

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, 50);

        // Assert
        result.IsValid.Should().BeTrue();
        result.AvailableQuantity.Should().Be(100);
        result.RequestedQuantity.Should().Be(50);
        result.ErrorMessage.Should().BeNullOrEmpty();
        result.IsLowStock.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateStockAvailability_WithUnassignedProduct_ShouldReturnInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync((Inventory?)null);

        // Act
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, 10);

        // Assert
        result.IsValid.Should().BeFalse();
        result.AvailableQuantity.Should().Be(0);
        result.ErrorMessage.Should().Contain("no está asignado");
    }

    [Fact]
    public async Task ValidateStockAvailability_WithInactiveInventory_ShouldReturnInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();
        var inventory = TestDataGenerator.CreateInventory(
            productId: productId,
            pointOfSaleId: posId,
            isActive: false,
            quantity: 100);

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, 10);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no está asignado");
    }

    [Fact]
    public async Task ValidateStockAvailability_WithInsufficientStock_ShouldReturnInvalid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();
        var inventory = TestDataGenerator.CreateInventory(
            productId: productId,
            pointOfSaleId: posId,
            isActive: true,
            quantity: 5);

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, 10);

        // Assert
        result.IsValid.Should().BeFalse();
        result.AvailableQuantity.Should().Be(5);
        result.RequestedQuantity.Should().Be(10);
        result.ErrorMessage.Should().Contain("Stock insuficiente");
        result.ErrorMessage.Should().Contain("Disponible: 5");
        result.ErrorMessage.Should().Contain("Solicitado: 10");
    }

    [Fact]
    public async Task ValidateStockAvailability_WithLowStockAfterSale_ShouldReturnWarning()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();
        var inventory = TestDataGenerator.CreateInventory(
            productId: productId,
            pointOfSaleId: posId,
            isActive: true,
            quantity: 10);

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync(inventory);

        // Act - Request 8 units, leaving 2 (below 10% threshold of 10, which is max(1, 5) = 5)
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, 8);

        // Assert
        result.IsValid.Should().BeTrue();
        result.IsLowStock.Should().BeTrue();
        result.WarningMessage.Should().Contain("Stock bajo");
        result.WarningMessage.Should().Contain("2 unidades");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ValidateStockAvailability_WithInvalidQuantity_ShouldReturnError(int quantity)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();

        // Act
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, quantity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("debe ser mayor que cero");
    }

    [Fact]
    public async Task ValidateStockAvailability_WithExactStockMatch_ShouldReturnValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();
        var inventory = TestDataGenerator.CreateInventory(
            productId: productId,
            pointOfSaleId: posId,
            isActive: true,
            quantity: 10);

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, 10);

        // Assert
        result.IsValid.Should().BeTrue();
        result.AvailableQuantity.Should().Be(10);
        result.RequestedQuantity.Should().Be(10);
        result.IsLowStock.Should().BeFalse(); // Remaining = 0, which doesn't trigger low stock warning
    }

    [Fact]
    public async Task ValidateStockAvailability_WithMultiUnitValidation_ShouldWork()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();
        var inventory = TestDataGenerator.CreateInventory(
            productId: productId,
            pointOfSaleId: posId,
            isActive: true,
            quantity: 100);

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.ValidateStockAvailabilityAsync(productId, posId, 25);

        // Assert
        result.IsValid.Should().BeTrue();
        result.AvailableQuantity.Should().Be(100);
        result.RequestedQuantity.Should().Be(25);
    }
}
