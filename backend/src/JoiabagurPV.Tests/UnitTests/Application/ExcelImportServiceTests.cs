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
/// Unit tests for ExcelImportService.
/// </summary>
public class ExcelImportServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICollectionRepository> _collectionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IExcelTemplateService> _templateServiceMock;
    private readonly Mock<ILogger<ExcelImportService>> _loggerMock;
    private readonly ExcelImportService _sut;

    public ExcelImportServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _collectionRepositoryMock = new Mock<ICollectionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _templateServiceMock = new Mock<IExcelTemplateService>();
        _loggerMock = new Mock<ILogger<ExcelImportService>>();

        _sut = new ExcelImportService(
            _productRepositoryMock.Object,
            _collectionRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _templateServiceMock.Object,
            _loggerMock.Object);
    }

    #region Validation Tests

    [Fact]
    public async Task ValidateAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Gold Ring", "Beautiful gold ring", 299.99m, ""),
            ("JOY-002", "Silver Necklace", "Elegant silver necklace", 199.99m, "Summer")
        });

        // Act
        var result = await _sut.ValidateAsync(stream);

        // Assert
        result.Success.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.TotalRows.Should().Be(2);
    }

    [Fact]
    public async Task ValidateAsync_WithMissingRequiredColumns_ShouldReturnErrors()
    {
        // Arrange
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Products");
        worksheet.Cell(1, 1).Value = "Name"; // Missing SKU and Price
        worksheet.Cell(1, 2).Value = "Description";
        worksheet.Cell(2, 1).Value = "Gold Ring";
        worksheet.Cell(2, 2).Value = "Description";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        // Act
        var result = await _sut.ValidateAsync(stream);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == "SKU");
        result.Errors.Should().Contain(e => e.Field == "Price");
    }

    [Fact]
    public async Task ValidateAsync_WithEmptySku_ShouldReturnError()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("", "Gold Ring", "Description", 299.99m, ""),
        });

        // Act
        var result = await _sut.ValidateAsync(stream);

        // Assert
        // Empty SKU rows are skipped during parsing, so no error
        result.TotalRows.Should().Be(0);
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyName_ShouldReturnError()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "", "Description", 299.99m, ""),
        });

        // Act
        var result = await _sut.ValidateAsync(stream);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == "Name" && e.RowNumber == 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task ValidateAsync_WithInvalidPrice_ShouldReturnError(decimal price)
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Gold Ring", "Description", price, ""),
        });

        // Act
        var result = await _sut.ValidateAsync(stream);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == "Price" && e.RowNumber == 2);
    }

    [Fact]
    public async Task ValidateAsync_WithDuplicateSkuInFile_ShouldReturnError()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Gold Ring", "Description", 299.99m, ""),
            ("JOY-001", "Silver Ring", "Another description", 199.99m, ""),
        });

        // Act
        var result = await _sut.ValidateAsync(stream);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("SKU duplicado en el archivo"));
    }

    #endregion

    #region Import Tests

    [Fact]
    public async Task ImportAsync_WithNewProducts_ShouldCreateProducts()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Gold Ring", "Beautiful gold ring", 299.99m, ""),
            ("JOY-002", "Silver Necklace", "Elegant silver necklace", 199.99m, ""),
        });

        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>());
        _collectionRepositoryMock.Setup(x => x.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Collection>());

        // Act
        var result = await _sut.ImportAsync(stream);

        // Assert
        result.Success.Should().BeTrue();
        result.CreatedCount.Should().Be(2);
        result.UpdatedCount.Should().Be(0);

        _productRepositoryMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Product>>(p => p.Count() == 2)), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WithExistingProducts_ShouldUpdateProducts()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Updated Gold Ring", "Updated description", 399.99m, ""),
        });

        var existingProduct = TestDataGenerator.CreateProduct(sku: "JOY-001", name: "Gold Ring", price: 299.99m);
        var existingProducts = new Dictionary<string, Product> { { "JOY-001", existingProduct } };

        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(existingProducts);
        _collectionRepositoryMock.Setup(x => x.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Collection>());

        // Act
        var result = await _sut.ImportAsync(stream);

        // Assert
        result.Success.Should().BeTrue();
        result.CreatedCount.Should().Be(0);
        result.UpdatedCount.Should().Be(1);

        existingProduct.Name.Should().Be("Updated Gold Ring");
        existingProduct.Price.Should().Be(399.99m);

        _productRepositoryMock.Verify(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<Product>>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WithMixedNewAndExisting_ShouldCreateAndUpdate()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Updated Gold Ring", "Updated description", 399.99m, ""),
            ("JOY-NEW", "New Diamond Ring", "New description", 999.99m, ""),
        });

        var existingProduct = TestDataGenerator.CreateProduct(sku: "JOY-001");
        var existingProducts = new Dictionary<string, Product> { { "JOY-001", existingProduct } };

        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(existingProducts);
        _collectionRepositoryMock.Setup(x => x.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Collection>());

        // Act
        var result = await _sut.ImportAsync(stream);

        // Assert
        result.Success.Should().BeTrue();
        result.CreatedCount.Should().Be(1);
        result.UpdatedCount.Should().Be(1);
    }

    [Fact]
    public async Task ImportAsync_WithNewCollection_ShouldCreateCollection()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Gold Ring", "Description", 299.99m, "Summer Collection"),
        });

        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>());
        _collectionRepositoryMock.Setup(x => x.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Collection>());

        // Act
        var result = await _sut.ImportAsync(stream);

        // Assert
        result.Success.Should().BeTrue();
        result.CollectionsCreatedCount.Should().Be(1);

        _collectionRepositoryMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Collection>>(c => 
            c.Any(col => col.Name == "Summer Collection"))), Times.Once);
    }

    [Fact]
    public async Task ImportAsync_WithExistingCollection_ShouldUseExistingCollection()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Gold Ring", "Description", 299.99m, "Summer"),
        });

        var existingCollection = TestDataGenerator.CreateCollection(name: "Summer");
        var existingCollections = new Dictionary<string, Collection> { { "summer", existingCollection } };

        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>());
        _collectionRepositoryMock.Setup(x => x.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(existingCollections);

        // Act
        var result = await _sut.ImportAsync(stream);

        // Assert
        result.Success.Should().BeTrue();
        result.CollectionsCreatedCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportAsync_WithValidationErrors_ShouldNotImport()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "", "Description", 299.99m, ""), // Empty name
        });

        // Act
        var result = await _sut.ImportAsync(stream);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();

        _productRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<Product>>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task ImportAsync_WithTransactionError_ShouldRollback()
    {
        // Arrange
        using var stream = CreateExcelStream(new[]
        {
            ("JOY-001", "Gold Ring", "Description", 299.99m, ""),
        });

        _productRepositoryMock.Setup(x => x.GetBySkusAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Product>());
        _collectionRepositoryMock.Setup(x => x.GetByNamesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, Collection>());
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.ImportAsync(stream);

        // Assert
        result.Success.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static MemoryStream CreateExcelStream(IEnumerable<(string Sku, string Name, string Description, decimal Price, string Collection)> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Products");

        // Headers
        worksheet.Cell(1, 1).Value = "SKU";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Price";
        worksheet.Cell(1, 5).Value = "Collection";

        // Data
        var rowNum = 2;
        foreach (var row in rows)
        {
            worksheet.Cell(rowNum, 1).Value = row.Sku;
            worksheet.Cell(rowNum, 2).Value = row.Name;
            worksheet.Cell(rowNum, 3).Value = row.Description;
            worksheet.Cell(rowNum, 4).Value = row.Price;
            worksheet.Cell(rowNum, 5).Value = row.Collection;
            rowNum++;
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    #endregion
}




