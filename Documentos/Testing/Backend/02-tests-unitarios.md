# Tests Unitarios

[← Volver al índice](../../testing-backend.md)

## Test Unitario Básico

```csharp
using FluentAssertions;
using Moq;
using Xunit;
using Joyeria.Core.Entities;
using Joyeria.Core.Interfaces;
using Joyeria.Core.DTOs;
using Joyeria.API.Services;

namespace Joyeria.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _sut; // System Under Test

    public ProductServiceTests()
    {
        // Arrange común para todos los tests (equivalente a [SetUp])
        _mockRepo = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _sut = new ProductService(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProductBySku_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var expectedProduct = new Product
        {
            Id = 1,
            Sku = "ANI-001",
            Name = "Anillo Oro 18k",
            Price = 1500.00m,
            Stock = 5
        };

        _mockRepo
            .Setup(r => r.GetBySkuAsync("ANI-001"))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _sut.GetProductBySkuAsync("ANI-001");

        // Assert
        result.Should().NotBeNull();
        result!.Sku.Should().Be("ANI-001");
        result.Name.Should().Contain("Anillo");
        result.Price.Should().BePositive();
    }

    [Fact]
    public async Task GetProductBySku_WhenProductNotExists_ShouldReturnNull()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetBySkuAsync(It.IsAny<string>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _sut.GetProductBySkuAsync("NO-EXISTE");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductBySku_ShouldCallRepositoryOnce()
    {
        // Arrange
        _mockRepo
            .Setup(r => r.GetBySkuAsync(It.IsAny<string>()))
            .ReturnsAsync(new Product());

        // Act
        await _sut.GetProductBySkuAsync("TEST-001");

        // Assert - Verificar que el repositorio fue llamado exactamente una vez
        _mockRepo.Verify(r => r.GetBySkuAsync("TEST-001"), Times.Once);
    }
}
```

---

## Tests Parametrizados con [Theory]

```csharp
using FluentAssertions;
using Xunit;

namespace Joyeria.UnitTests.Services;

public class ProductValidationTests
{
    private readonly ProductValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void ValidateSku_WithInvalidSku_ShouldReturnFalse(string? invalidSku)
    {
        // Act
        var result = _validator.IsValidSku(invalidSku);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("ANI-001", true)]
    [InlineData("COL-123", true)]
    [InlineData("PUL-999", true)]
    [InlineData("AB", false)]        // Muy corto
    [InlineData("", false)]          // Vacío
    public void ValidateSku_WithVariousInputs_ShouldReturnExpectedResult(
        string sku, 
        bool expectedResult)
    {
        // Act
        var result = _validator.IsValidSku(sku);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ValidatePrice_WithNonPositivePrice_ShouldReturnFalse(decimal price)
    {
        // Act
        var result = _validator.IsValidPrice(price);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(9999.99)]
    public void ValidatePrice_WithPositivePrice_ShouldReturnTrue(decimal price)
    {
        // Act
        var result = _validator.IsValidPrice(price);

        // Assert
        result.Should().BeTrue();
    }
}
```

---

## Tests con Excepciones

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace Joyeria.UnitTests.Services;

public class SaleServiceTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SaleService _sut;

    public SaleServiceTests()
    {
        _mockSaleRepo = new Mock<ISaleRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _sut = new SaleService(
            _mockSaleRepo.Object,
            _mockProductRepo.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task CreateSale_WithInsufficientStock_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var product = new Product { Id = 1, Stock = 0 };
        _mockProductRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var saleDto = new CreateSaleDto { ProductId = 1, Quantity = 1 };

        // Act & Assert
        await _sut.Invoking(s => s.CreateSaleAsync(saleDto))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*stock*insuficiente*");
    }

    [Fact]
    public async Task CreateSale_WithNonExistentProduct_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockProductRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);

        var saleDto = new CreateSaleDto { ProductId = 999, Quantity = 1 };

        // Act & Assert
        await _sut.Invoking(s => s.CreateSaleAsync(saleDto))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*producto*no*encontrado*");
    }

    [Fact]
    public async Task CreateSale_WithValidData_ShouldDecrementStock()
    {
        // Arrange
        var product = new Product { Id = 1, Stock = 10 };
        _mockProductRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var saleDto = new CreateSaleDto { ProductId = 1, Quantity = 3 };

        // Act
        await _sut.CreateSaleAsync(saleDto);

        // Assert
        product.Stock.Should().Be(7);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
```

---

## Generador de Datos con Bogus

```csharp
using Bogus;
using Joyeria.Core.Entities;

namespace Joyeria.UnitTests.Helpers;

/// <summary>
/// Builder para generar datos de prueba consistentes y realistas
/// </summary>
public static class TestDataBuilder
{
    private static int _productIdCounter = 1;
    private static int _saleIdCounter = 1;

    // Generador de productos
    private static readonly Faker<Product> ProductFaker = new Faker<Product>("es")
        .RuleFor(p => p.Id, _ => _productIdCounter++)
        .RuleFor(p => p.Sku, f => $"{f.Random.String2(3).ToUpper()}-{f.Random.Number(100, 999)}")
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Price, f => f.Finance.Amount(50, 5000))
        .RuleFor(p => p.Cost, (f, p) => p.Price * f.Random.Decimal(0.3m, 0.6m))
        .RuleFor(p => p.Stock, f => f.Random.Number(0, 100))
        .RuleFor(p => p.Category, f => f.PickRandom("Anillos", "Collares", "Pulseras", "Aretes", "Relojes"))
        .RuleFor(p => p.Material, f => f.PickRandom("Oro 18k", "Oro 14k", "Plata 925", "Acero", "Platino"))
        .RuleFor(p => p.IsActive, f => f.Random.Bool(0.9f))
        .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
        .RuleFor(p => p.UpdatedAt, f => f.Date.Recent(30));

    // Generador de ventas
    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>("es")
        .RuleFor(s => s.Id, _ => _saleIdCounter++)
        .RuleFor(s => s.Quantity, f => f.Random.Number(1, 5))
        .RuleFor(s => s.UnitPrice, f => f.Finance.Amount(50, 5000))
        .RuleFor(s => s.TotalPrice, (f, s) => s.Quantity * s.UnitPrice)
        .RuleFor(s => s.PaymentMethod, f => f.PickRandom("Efectivo", "Tarjeta", "Transferencia"))
        .RuleFor(s => s.CreatedAt, f => f.Date.Recent(30));

    // Generador de usuarios
    private static readonly Faker<User> UserFaker = new Faker<User>("es")
        .RuleFor(u => u.Id, f => f.Random.Guid().ToString())
        .RuleFor(u => u.Username, f => f.Internet.UserName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.FullName, f => f.Name.FullName())
        .RuleFor(u => u.Role, f => f.PickRandom("Admin", "Operador"))
        .RuleFor(u => u.IsActive, f => f.Random.Bool(0.9f))
        .RuleFor(u => u.CreatedAt, f => f.Date.Past(2));

    // Métodos públicos
    public static Product CreateProduct() => ProductFaker.Generate();
    public static List<Product> CreateProducts(int count) => ProductFaker.Generate(count);

    public static Sale CreateSale() => SaleFaker.Generate();
    public static List<Sale> CreateSales(int count) => SaleFaker.Generate(count);

    public static User CreateUser() => UserFaker.Generate();
    public static User CreateAdminUser() => UserFaker.Clone()
        .RuleFor(u => u.Role, "Admin")
        .Generate();
    public static User CreateOperatorUser() => UserFaker.Clone()
        .RuleFor(u => u.Role, "Operador")
        .Generate();

    // Métodos para crear entidades con datos específicos
    public static Product CreateProductWithStock(int stock) => ProductFaker.Clone()
        .RuleFor(p => p.Stock, stock)
        .Generate();

    public static Product CreateProductWithPrice(decimal price) => ProductFaker.Clone()
        .RuleFor(p => p.Price, price)
        .Generate();

    public static Sale CreateSaleForProduct(Product product) => SaleFaker.Clone()
        .RuleFor(s => s.ProductId, product.Id)
        .RuleFor(s => s.UnitPrice, product.Price)
        .Generate();

    // Reset counters (útil entre tests)
    public static void ResetCounters()
    {
        _productIdCounter = 1;
        _saleIdCounter = 1;
    }
}
```

---

## Uso de TestDataBuilder en Tests

```csharp
using FluentAssertions;
using Moq;
using Xunit;
using Joyeria.UnitTests.Helpers;

namespace Joyeria.UnitTests.Services;

public class InventoryServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _sut = new InventoryService(_mockRepo.Object);
        TestDataBuilder.ResetCounters();
    }

    [Fact]
    public async Task GetLowStockProducts_ShouldReturnOnlyProductsWithLowStock()
    {
        // Arrange
        var products = new List<Product>
        {
            TestDataBuilder.CreateProductWithStock(2),  // Low stock
            TestDataBuilder.CreateProductWithStock(50), // Normal stock
            TestDataBuilder.CreateProductWithStock(0),  // Out of stock
            TestDataBuilder.CreateProductWithStock(5),  // Low stock threshold
        };

        _mockRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetLowStockProductsAsync(threshold: 5);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(p => p.Stock <= 5);
    }

    [Fact]
    public async Task CalculateTotalInventoryValue_ShouldSumAllProductValues()
    {
        // Arrange
        var products = TestDataBuilder.CreateProducts(10);
        _mockRepo
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        var expectedTotal = products.Sum(p => p.Price * p.Stock);

        // Act
        var result = await _sut.CalculateTotalInventoryValueAsync();

        // Assert
        result.Should().Be(expectedTotal);
    }
}
```

---

## Comandos para Ejecutar Tests

```bash
# Todos los tests
dotnet test backend/Joyeria.sln

# Solo tests unitarios
dotnet test backend/tests/Joyeria.UnitTests/Joyeria.UnitTests.csproj

# Filtrar por nombre de test
dotnet test --filter "FullyQualifiedName~ProductService"

# Filtrar por categoría/trait
dotnet test --filter "Category=Unit"

# Modo verboso
dotnet test --verbosity detailed
```

