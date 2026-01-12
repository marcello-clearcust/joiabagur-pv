using FluentAssertions;
using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for InventoryService.
/// </summary>
public class InventoryServiceTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<IInventoryMovementRepository> _movementRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IPointOfSaleRepository> _pointOfSaleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<InventoryService>> _loggerMock;
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _movementRepositoryMock = new Mock<IInventoryMovementRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _pointOfSaleRepositoryMock = new Mock<IPointOfSaleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<InventoryService>>();

        _sut = new InventoryService(
            _inventoryRepositoryMock.Object,
            _movementRepositoryMock.Object,
            _productRepositoryMock.Object,
            _pointOfSaleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region AssignProduct Tests

    [Fact]
    public async Task AssignProduct_WithValidData_ShouldCreateAssignment()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var request = new AssignProductRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id))
            .ReturnsAsync(product);
        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync((Inventory?)null);
        _inventoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);

        // Act
        var result = await _sut.AssignProductAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Inventory.Should().NotBeNull();
        result.Inventory!.ProductId.Should().Be(product.Id);
        result.Inventory.PointOfSaleId.Should().Be(pos.Id);
        result.WasReactivated.Should().BeFalse();

        _inventoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Inventory>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignProduct_WithNonExistentProduct_ShouldReturnError()
    {
        // Arrange
        var request = new AssignProductRequest
        {
            ProductId = Guid.NewGuid(),
            PointOfSaleId = Guid.NewGuid()
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(request.ProductId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.AssignProductAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task AssignProduct_WithInactiveProduct_ShouldReturnError()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct(isActive: false);
        var request = new AssignProductRequest
        {
            ProductId = product.Id,
            PointOfSaleId = Guid.NewGuid()
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.AssignProductAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("inactivo");
    }

    [Fact]
    public async Task AssignProduct_WithDuplicateAssignment_ShouldReturnError()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var existingInventory = TestDataGenerator.CreateInventory(productId: product.Id, pointOfSaleId: pos.Id, isActive: true);
        existingInventory.Product = product;
        existingInventory.PointOfSale = pos;
        
        var request = new AssignProductRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id))
            .ReturnsAsync(product);
        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(existingInventory);

        // Act
        var result = await _sut.AssignProductAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ya está asignado");
    }

    [Fact]
    public async Task AssignProduct_ReactivatingPreviousAssignment_ShouldPreserveQuantity()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var existingInventory = TestDataGenerator.CreateInventory(
            productId: product.Id, 
            pointOfSaleId: pos.Id, 
            isActive: false,
            quantity: 50);
        existingInventory.Product = product;
        existingInventory.PointOfSale = pos;
        
        var request = new AssignProductRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id
        };

        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id))
            .ReturnsAsync(product);
        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(existingInventory);
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);

        // Act
        var result = await _sut.AssignProductAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.WasReactivated.Should().BeTrue();
        result.Inventory!.Quantity.Should().Be(50); // Preserved
        existingInventory.IsActive.Should().BeTrue();
    }

    #endregion

    #region UnassignProduct Tests

    [Fact]
    public async Task UnassignProduct_WithZeroQuantity_ShouldSucceed()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id, 
            pointOfSaleId: pos.Id, 
            isActive: true,
            quantity: 0);
        inventory.Product = product;
        inventory.PointOfSale = pos;
        
        var request = new UnassignProductRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id
        };

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);

        // Act
        var result = await _sut.UnassignProductAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        inventory.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UnassignProduct_WithNonZeroQuantity_ShouldReturnError()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id, 
            pointOfSaleId: pos.Id, 
            isActive: true,
            quantity: 10);
        inventory.Product = product;
        inventory.PointOfSale = pos;
        
        var request = new UnassignProductRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id
        };

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.UnassignProductAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("stock");
    }

    [Fact]
    public async Task UnassignProduct_WithNonExistentAssignment_ShouldReturnError()
    {
        // Arrange
        var request = new UnassignProductRequest
        {
            ProductId = Guid.NewGuid(),
            PointOfSaleId = Guid.NewGuid()
        };

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(
            request.ProductId, request.PointOfSaleId))
            .ReturnsAsync((Inventory?)null);

        // Act
        var result = await _sut.UnassignProductAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no está asignado");
    }

    #endregion

    #region AdjustStock Tests

    [Fact]
    public async Task AdjustStock_WithValidIncrease_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id, 
            pointOfSaleId: pos.Id, 
            isActive: true,
            quantity: 10);
        inventory.Product = product;
        inventory.PointOfSale = pos;
        
        var request = new StockAdjustmentRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id,
            QuantityChange = 5,
            Reason = "Test adjustment"
        };

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);
        _movementRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement m) => m);

        // Act
        var result = await _sut.AdjustStockAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.QuantityBefore.Should().Be(10);
        result.QuantityAfter.Should().Be(15);
        inventory.Quantity.Should().Be(15);

        _movementRepositoryMock.Verify(x => x.AddAsync(It.Is<InventoryMovement>(m => 
            m.MovementType == MovementType.Adjustment &&
            m.QuantityChange == 5 &&
            m.QuantityBefore == 10 &&
            m.QuantityAfter == 15
        )), Times.Once);
    }

    [Fact]
    public async Task AdjustStock_WithValidDecrease_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id, 
            pointOfSaleId: pos.Id, 
            isActive: true,
            quantity: 10);
        inventory.Product = product;
        inventory.PointOfSale = pos;
        
        var request = new StockAdjustmentRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id,
            QuantityChange = -5,
            Reason = "Test adjustment"
        };

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);
        _movementRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement m) => m);

        // Act
        var result = await _sut.AdjustStockAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.QuantityBefore.Should().Be(10);
        result.QuantityAfter.Should().Be(5);
    }

    [Fact]
    public async Task AdjustStock_ResultingInNegative_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id, 
            pointOfSaleId: pos.Id, 
            isActive: true,
            quantity: 5);
        inventory.Product = product;
        inventory.PointOfSale = pos;
        
        var request = new StockAdjustmentRequest
        {
            ProductId = product.Id,
            PointOfSaleId = pos.Id,
            QuantityChange = -10,
            Reason = "Test adjustment"
        };

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.AdjustStockAsync(request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("negativo");
    }

    [Fact]
    public async Task AdjustStock_WithoutReason_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new StockAdjustmentRequest
        {
            ProductId = Guid.NewGuid(),
            PointOfSaleId = Guid.NewGuid(),
            QuantityChange = 5,
            Reason = "" // Empty reason
        };

        // Act
        var result = await _sut.AdjustStockAsync(request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("motivo");
    }

    [Fact]
    public async Task AdjustStock_WithUnassignedProduct_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new StockAdjustmentRequest
        {
            ProductId = Guid.NewGuid(),
            PointOfSaleId = Guid.NewGuid(),
            QuantityChange = 5,
            Reason = "Test adjustment"
        };

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(
            request.ProductId, request.PointOfSaleId))
            .ReturnsAsync((Inventory?)null);

        // Act
        var result = await _sut.AdjustStockAsync(request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no está asignado");
    }

    #endregion

    #region Stock Query Tests

    [Fact]
    public async Task GetStockByPointOfSale_ShouldReturnPaginatedResult()
    {
        // Arrange
        var posId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventories = new List<Inventory>
        {
            TestDataGenerator.CreateInventory(productId: product.Id, pointOfSaleId: posId, quantity: 10)
        };
        inventories[0].Product = product;
        inventories[0].PointOfSale = pos;

        _inventoryRepositoryMock.Setup(x => x.FindByPointOfSaleAsync(posId, true))
            .ReturnsAsync(inventories);

        // Act
        var result = await _sut.GetStockByPointOfSaleAsync(posId, 1, 50);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetAssignedProducts_ShouldReturnActiveInventories()
    {
        // Arrange
        var posId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventories = new List<Inventory>
        {
            TestDataGenerator.CreateInventory(productId: product.Id, pointOfSaleId: posId, isActive: true)
        };
        inventories[0].Product = product;
        inventories[0].PointOfSale = pos;

        _inventoryRepositoryMock.Setup(x => x.FindByPointOfSaleAsync(posId, true))
            .ReturnsAsync(inventories);

        // Act
        var result = await _sut.GetAssignedProductsAsync(posId);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(i => i.IsActive);
    }

    #endregion

    #region CreateSaleMovement Tests

    [Fact]
    public async Task CreateSaleMovement_WithSufficientStock_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var saleId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id,
            pointOfSaleId: pos.Id,
            isActive: true,
            quantity: 20);
        inventory.Product = product;
        inventory.PointOfSale = pos;

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);
        _movementRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement m) => m);

        // Act
        var result = await _sut.CreateSaleMovementAsync(product.Id, pos.Id, saleId, 5, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.QuantityBefore.Should().Be(20);
        result.QuantityAfter.Should().Be(15);
        inventory.Quantity.Should().Be(15);

        _movementRepositoryMock.Verify(x => x.AddAsync(It.Is<InventoryMovement>(m =>
            m.MovementType == MovementType.Sale &&
            m.SaleId == saleId &&
            m.QuantityChange == -5 &&
            m.QuantityBefore == 20 &&
            m.QuantityAfter == 15 &&
            m.UserId == userId
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateSaleMovement_WithInsufficientStock_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var saleId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id,
            pointOfSaleId: pos.Id,
            isActive: true,
            quantity: 3);
        inventory.Product = product;
        inventory.PointOfSale = pos;

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.CreateSaleMovementAsync(product.Id, pos.Id, saleId, 5, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Stock insuficiente");
        result.ErrorMessage.Should().Contain("Disponible: 3");
        result.ErrorMessage.Should().Contain("Solicitado: 5");

        // Verify no changes were made
        _inventoryRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Inventory>()), Times.Never);
        _movementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<InventoryMovement>()), Times.Never);
    }

    [Fact]
    public async Task CreateSaleMovement_WithUnassignedProduct_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(productId, posId))
            .ReturnsAsync((Inventory?)null);

        // Act
        var result = await _sut.CreateSaleMovementAsync(productId, posId, saleId, 5, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no está asignado");
    }

    [Fact]
    public async Task CreateSaleMovement_WithInactiveInventory_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var saleId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id,
            pointOfSaleId: pos.Id,
            isActive: false,
            quantity: 20);
        inventory.Product = product;
        inventory.PointOfSale = pos;

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);

        // Act
        var result = await _sut.CreateSaleMovementAsync(product.Id, pos.Id, saleId, 5, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no está asignado");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task CreateSaleMovement_WithInvalidQuantity_ShouldReturnError(int quantity)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var posId = Guid.NewGuid();

        // Act
        var result = await _sut.CreateSaleMovementAsync(productId, posId, saleId, quantity, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("debe ser mayor que cero");
    }

    [Fact]
    public async Task CreateSaleMovement_TransactionRollback_OnException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var saleId = Guid.NewGuid();
        var product = TestDataGenerator.CreateProduct();
        var pos = TestDataGenerator.CreatePointOfSale();
        var inventory = TestDataGenerator.CreateInventory(
            productId: product.Id,
            pointOfSaleId: pos.Id,
            isActive: true,
            quantity: 20);
        inventory.Product = product;
        inventory.PointOfSale = pos;

        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(inventory);
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.CreateSaleMovementAsync(product.Id, pos.Id, saleId, 5, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error al crear el movimiento de venta");
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }

    #endregion
}

