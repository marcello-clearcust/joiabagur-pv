using FluentAssertions;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Tests.TestHelpers;
using Xunit;

namespace JoiabagurPV.Tests.UnitTests.Domain;

/// <summary>
/// Unit tests for Product domain entity validations.
/// </summary>
public class ProductEntityTests
{
    [Fact]
    public void Product_IsPriceValid_WithPositivePrice_ShouldReturnTrue()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct(price: 100.00m);

        // Act
        var result = product.IsPriceValid();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Product_IsPriceValid_WithZeroOrNegativePrice_ShouldReturnFalse(decimal price)
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct(price: price);

        // Act
        var result = product.IsPriceValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Product_IsSkuValid_WithValidSku_ShouldReturnTrue()
    {
        // Arrange
        var product = TestDataGenerator.CreateProduct(sku: "JOY-001");

        // Act
        var result = product.IsSkuValid();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Product_IsSkuValid_WithEmptyOrWhitespaceSku_ShouldReturnFalse(string? sku)
    {
        // Arrange
        var product = new Product
        {
            SKU = sku!,
            Name = "Test Product",
            Price = 100.00m
        };

        // Act
        var result = product.IsSkuValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Product_ShouldHaveDefaultIsActiveTrue()
    {
        // Arrange & Act
        var product = new Product
        {
            SKU = "JOY-001",
            Name = "Test Product",
            Price = 100.00m
        };

        // Assert
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_ShouldInitializePhotosCollection()
    {
        // Arrange & Act
        var product = new Product
        {
            SKU = "JOY-001",
            Name = "Test Product",
            Price = 100.00m
        };

        // Assert
        product.Photos.Should().NotBeNull();
        product.Photos.Should().BeEmpty();
    }

    [Fact]
    public void Collection_ShouldInitializeProductsCollection()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Name = "Summer Collection"
        };

        // Assert
        collection.Products.Should().NotBeNull();
        collection.Products.Should().BeEmpty();
    }

    [Fact]
    public void ProductPhoto_ShouldHaveDefaultDisplayOrderZero()
    {
        // Arrange & Act
        var photo = new ProductPhoto
        {
            ProductId = Guid.NewGuid(),
            FileName = "test.jpg"
        };

        // Assert
        photo.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void ProductPhoto_ShouldHaveDefaultIsPrimaryFalse()
    {
        // Arrange & Act
        var photo = new ProductPhoto
        {
            ProductId = Guid.NewGuid(),
            FileName = "test.jpg"
        };

        // Assert
        photo.IsPrimary.Should().BeFalse();
    }
}



