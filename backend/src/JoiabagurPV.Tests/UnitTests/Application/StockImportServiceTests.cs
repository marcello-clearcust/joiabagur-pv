using ClosedXML.Excel;
using FluentAssertions;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for StockImportService.
/// </summary>
public class StockImportServiceTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly Mock<IInventoryMovementRepository> _movementRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IPointOfSaleRepository> _pointOfSaleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExcelTemplateService> _templateServiceMock;
    private readonly Mock<ILogger<StockImportService>> _loggerMock;
    private readonly StockImportService _sut;

    public StockImportServiceTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _movementRepositoryMock = new Mock<IInventoryMovementRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _pointOfSaleRepositoryMock = new Mock<IPointOfSaleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _templateServiceMock = new Mock<IExcelTemplateService>();
        _loggerMock = new Mock<ILogger<StockImportService>>();

        _sut = new StockImportService(
            _inventoryRepositoryMock.Object,
            _movementRepositoryMock.Object,
            _productRepositoryMock.Object,
            _pointOfSaleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _templateServiceMock.Object,
            _loggerMock.Object);
    }

    #region Helper Methods

    private static MemoryStream CreateExcelStream(params (string sku, int quantity)[] rows)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stock Import");
        
        worksheet.Cell(1, 1).Value = "SKU";
        worksheet.Cell(1, 2).Value = "Quantity";
        
        for (int i = 0; i < rows.Length; i++)
        {
            worksheet.Cell(i + 2, 1).Value = rows[i].sku;
            worksheet.Cell(i + 2, 2).Value = rows[i].quantity;
        }
        
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateExcelStreamWithMissingColumns()
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Stock Import");
        
        worksheet.Cell(1, 1).Value = "WrongColumn";
        worksheet.Cell(2, 1).Value = "TEST-001";
        
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    #endregion

    #region GenerateTemplate Tests

    [Fact]
    public void GenerateTemplate_ShouldCallTemplateService()
    {
        // Arrange
        var templateStream = new MemoryStream();
        _templateServiceMock.Setup(x => x.GenerateTemplate(It.IsAny<ExcelTemplateConfig>()))
            .Returns(templateStream);

        // Act
        var result = _sut.GenerateTemplate();

        // Assert
        result.Should().NotBeNull();
        _templateServiceMock.Verify(x => x.GenerateTemplate(It.Is<ExcelTemplateConfig>(c =>
            c.Columns.Count == 2 &&
            c.Columns.Any(col => col.Name == "SKU") &&
            c.Columns.Any(col => col.Name == "Quantity")
        )), Times.Once);
    }

    #endregion

    #region ValidateAsync Tests

    [Fact]
    public async Task ValidateAsync_WithValidData_ShouldSucceed()
    {
        // Arrange
        var pos = TestDataGenerator.CreatePointOfSale();
        var product = TestDataGenerator.CreateProduct(sku: "TEST-001");
        
        using var stream = CreateExcelStream(("TEST-001", 10));

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase) 
            { 
                { "TEST-001", product } 
            });

        // Act
        var result = await _sut.ValidateAsync(stream, pos.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.TotalRows.Should().Be(1);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_WithNonExistentPos_ShouldReturnError()
    {
        // Arrange
        var posId = Guid.NewGuid();
        using var stream = CreateExcelStream(("TEST-001", 10));

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(posId))
            .ReturnsAsync((PointOfSale?)null);

        // Act
        var result = await _sut.ValidateAsync(stream, posId);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Field == "PointOfSaleId");
    }

    [Fact]
    public async Task ValidateAsync_WithMissingColumns_ShouldReturnError()
    {
        // Arrange
        var pos = TestDataGenerator.CreatePointOfSale();
        using var stream = CreateExcelStreamWithMissingColumns();

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);

        // Act
        var result = await _sut.ValidateAsync(stream, pos.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("columna requerida"));
    }

    [Fact]
    public async Task ValidateAsync_WithNonExistentSku_ShouldReturnError()
    {
        // Arrange
        var pos = TestDataGenerator.CreatePointOfSale();
        using var stream = CreateExcelStream(("NONEXISTENT", 10));

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase));

        // Act
        var result = await _sut.ValidateAsync(stream, pos.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.Field == "SKU" && e.Message.Contains("no encontrado"));
    }

    [Fact]
    public async Task ValidateAsync_WithDuplicateSku_ShouldReturnError()
    {
        // Arrange
        var pos = TestDataGenerator.CreatePointOfSale();
        var product = TestDataGenerator.CreateProduct(sku: "TEST-001");
        using var stream = CreateExcelStream(("TEST-001", 10), ("TEST-001", 20)); // Duplicate

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase) 
            { 
                { "TEST-001", product } 
            });

        // Act
        var result = await _sut.ValidateAsync(stream, pos.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("duplicado"));
    }

    [Fact]
    public async Task ValidateAsync_WithNegativeQuantity_ShouldReturnError()
    {
        // Arrange
        var pos = TestDataGenerator.CreatePointOfSale();
        var product = TestDataGenerator.CreateProduct(sku: "TEST-001");
        using var stream = CreateExcelStream(("TEST-001", -5)); // Negative

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase) 
            { 
                { "TEST-001", product } 
            });

        // Act
        var result = await _sut.ValidateAsync(stream, pos.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => 
            e.Field == "Quantity" && e.Message.Contains("0 o mayor"));
    }

    [Fact]
    public async Task ValidateAsync_WithInactiveProduct_ShouldReturnError()
    {
        // Arrange
        var pos = TestDataGenerator.CreatePointOfSale();
        var product = TestDataGenerator.CreateProduct(sku: "TEST-001", isActive: false);
        using var stream = CreateExcelStream(("TEST-001", 10));

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase) 
            { 
                { "TEST-001", product } 
            });

        // Act
        var result = await _sut.ValidateAsync(stream, pos.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("inactivo"));
    }

    #endregion

    #region ImportAsync Tests

    [Fact]
    public async Task ImportAsync_WithValidData_ShouldCreateImplicitAssignment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pos = TestDataGenerator.CreatePointOfSale();
        var product = TestDataGenerator.CreateProduct(sku: "TEST-001");
        using var stream = CreateExcelStream(("TEST-001", 10));

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase) 
            { 
                { "TEST-001", product } 
            });
        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync((Inventory?)null);
        _inventoryRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => { i.Id = Guid.NewGuid(); return i; });
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);
        _movementRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement m) => m);

        // Act
        var result = await _sut.ImportAsync(stream, pos.Id, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.AssignmentsCreatedCount.Should().Be(1);
        result.StockUpdatedCount.Should().Be(1);
        
        _inventoryRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Inventory>()), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WithExistingInventory_ShouldAddToQuantity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pos = TestDataGenerator.CreatePointOfSale();
        var product = TestDataGenerator.CreateProduct(sku: "TEST-001");
        var existingInventory = TestDataGenerator.CreateInventory(
            productId: product.Id, 
            pointOfSaleId: pos.Id,
            quantity: 5,
            isActive: true);
        
        using var stream = CreateExcelStream(("TEST-001", 10));

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);
        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase) 
            { 
                { "TEST-001", product } 
            });
        _inventoryRepositoryMock.Setup(x => x.FindByProductAndPointOfSaleAsync(product.Id, pos.Id))
            .ReturnsAsync(existingInventory);
        _inventoryRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Inventory>()))
            .ReturnsAsync((Inventory i) => i);
        _movementRepositoryMock.Setup(x => x.AddAsync(It.IsAny<InventoryMovement>()))
            .ReturnsAsync((InventoryMovement m) => m);

        // Act
        var result = await _sut.ImportAsync(stream, pos.Id, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.AssignmentsCreatedCount.Should().Be(0); // Already assigned
        result.StockUpdatedCount.Should().Be(1);
        existingInventory.Quantity.Should().Be(15); // 5 + 10
    }

    [Fact]
    public async Task ImportAsync_WithInactivePos_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pos = TestDataGenerator.CreatePointOfSale(isActive: false);
        using var stream = CreateExcelStream(("TEST-001", 10));

        _pointOfSaleRepositoryMock.Setup(x => x.GetByIdAsync(pos.Id))
            .ReturnsAsync(pos);

        // Act
        var result = await _sut.ImportAsync(stream, pos.Id, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("inactivo"));
    }

    #endregion
}

