# Mockear Entity Framework Core

[← Volver al índice](../../testing-backend.md)

## Mockear DbContext Completo

```csharp
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace Joyeria.UnitTests.Helpers;

/// <summary>
/// Helper para crear mocks de DbContext
/// </summary>
public static class DbContextMockHelper
{
    /// <summary>
    /// Crear un mock de DbContext con datos predefinidos
    /// </summary>
    public static Mock<JoyeriaDbContext> CreateMockContext(
        List<Product>? products = null,
        List<Sale>? sales = null,
        List<User>? users = null)
    {
        var mockContext = new Mock<JoyeriaDbContext>(
            new DbContextOptionsBuilder<JoyeriaDbContext>().Options
        );

        // Configurar DbSets con datos
        if (products != null)
            mockContext.Setup(c => c.Products).ReturnsDbSet(products);
        else
            mockContext.Setup(c => c.Products).ReturnsDbSet(new List<Product>());

        if (sales != null)
            mockContext.Setup(c => c.Sales).ReturnsDbSet(sales);
        else
            mockContext.Setup(c => c.Sales).ReturnsDbSet(new List<Sale>());

        if (users != null)
            mockContext.Setup(c => c.Users).ReturnsDbSet(users);
        else
            mockContext.Setup(c => c.Users).ReturnsDbSet(new List<User>());

        // Configurar SaveChangesAsync
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return mockContext;
    }
}
```

---

## Mockear DbSet con Queries LINQ

```csharp
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Joyeria.UnitTests.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task GetActiveProducts_ShouldFilterCorrectly()
    {
        // Arrange - Crear datos de prueba
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Activo 1", IsActive = true },
            new() { Id = 2, Name = "Inactivo", IsActive = false },
            new() { Id = 3, Name = "Activo 2", IsActive = true }
        };

        // Crear mock del DbContext
        var mockContext = DbContextMockHelper.CreateMockContext(products: products);
        var service = new ProductService(mockContext.Object);

        // Act
        var result = await service.GetActiveProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task SearchProducts_ShouldFilterByNameAndCategory()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Anillo Oro", Category = "Anillos", IsActive = true },
            new() { Id = 2, Name = "Collar Plata", Category = "Collares", IsActive = true },
            new() { Id = 3, Name = "Anillo Plata", Category = "Anillos", IsActive = true },
            new() { Id = 4, Name = "Anillo Inactivo", Category = "Anillos", IsActive = false }
        };

        var mockContext = DbContextMockHelper.CreateMockContext(products: products);
        var service = new ProductService(mockContext.Object);

        // Act
        var result = await service.SearchProductsAsync("Anillo", "Anillos");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => 
            p.Name.Contains("Anillo") && 
            p.Category == "Anillos" && 
            p.IsActive);
    }
}
```

---

## Mockear Transacciones

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace Joyeria.UnitTests.Services;

public class SaleServiceTransactionTests
{
    [Fact]
    public async Task CreateSale_ShouldUseTransaction()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Producto", Stock = 10, Price = 100 }
        };
        var sales = new List<Sale>();

        var mockContext = DbContextMockHelper.CreateMockContext(
            products: products, 
            sales: sales
        );

        // Mock de la transacción
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

        var service = new SaleService(mockContext.Object);

        // Act
        await service.CreateSaleWithTransactionAsync(new CreateSaleDto 
        { 
            ProductId = 1, 
            Quantity = 2 
        });

        // Assert - Verificar que se usó la transacción
        mockDatabase.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSale_OnError_ShouldRollback()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Producto", Stock = 0, Price = 100 } // Sin stock
        };

        var mockContext = DbContextMockHelper.CreateMockContext(products: products);

        var mockTransaction = new Mock<IDbContextTransaction>();
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);

        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

        var service = new SaleService(mockContext.Object);

        // Act & Assert
        await service.Invoking(s => s.CreateSaleWithTransactionAsync(
            new CreateSaleDto { ProductId = 1, Quantity = 1 }))
            .Should().ThrowAsync<InvalidOperationException>();

        // Verificar rollback
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

## Mockear Includes y Navigation Properties

```csharp
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace Joyeria.UnitTests.Services;

public class SaleReportServiceTests
{
    [Fact]
    public async Task GetSalesWithProducts_ShouldIncludeProductData()
    {
        // Arrange - Crear datos con relaciones
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Anillo Oro", Price = 1500 },
            new() { Id = 2, Name = "Collar Plata", Price = 800 }
        };

        var sales = new List<Sale>
        {
            new() 
            { 
                Id = 1, 
                ProductId = 1, 
                Product = products[0], // Navigation property
                Quantity = 2,
                UnitPrice = 1500,
                CreatedAt = DateTime.UtcNow
            },
            new() 
            { 
                Id = 2, 
                ProductId = 2, 
                Product = products[1],
                Quantity = 1,
                UnitPrice = 800,
                CreatedAt = DateTime.UtcNow
            }
        };

        var mockContext = new Mock<JoyeriaDbContext>(
            new DbContextOptionsBuilder<JoyeriaDbContext>().Options
        );
        mockContext.Setup(c => c.Sales).ReturnsDbSet(sales);

        var service = new SaleReportService(mockContext.Object);

        // Act
        var result = await service.GetSalesWithProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Product.Should().NotBeNull();
        result.First().Product!.Name.Should().Be("Anillo Oro");
    }
}
```

---

## In-Memory Database (Alternativa para Tests Simples)

```csharp
using Microsoft.EntityFrameworkCore;

namespace Joyeria.UnitTests.Helpers;

/// <summary>
/// Factory para crear DbContext en memoria (más simple que mocks)
/// Útil para tests que no necesitan comportamiento específico
/// </summary>
public static class InMemoryDbContextFactory
{
    public static JoyeriaDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<JoyeriaDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new JoyeriaDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static JoyeriaDbContext CreateWithSeedData()
    {
        var context = Create();
        
        context.Products.AddRange(
            new Product { Id = 1, Sku = "ANI-001", Name = "Anillo Test", Price = 100, Stock = 10 },
            new Product { Id = 2, Sku = "COL-001", Name = "Collar Test", Price = 200, Stock = 5 }
        );
        
        context.SaveChanges();
        return context;
    }
}

// Uso en tests
public class ProductServiceInMemoryTests
{
    [Fact]
    public async Task UpdateStock_ShouldPersistChanges()
    {
        // Arrange
        using var context = InMemoryDbContextFactory.CreateWithSeedData();
        var service = new ProductService(context);

        // Act
        await service.UpdateStockAsync(1, 5);

        // Assert - Verificar en un nuevo contexto
        using var verifyContext = InMemoryDbContextFactory.Create(context.Database.GetDbConnection().Database);
        var product = await verifyContext.Products.FindAsync(1);
        product!.Stock.Should().Be(5);
    }
}
```

---

## Paquete Recomendado: Moq.EntityFrameworkCore

```xml
<PackageReference Include="Moq.EntityFrameworkCore" Version="8.0.1" />
```

Este paquete añade el método de extensión `ReturnsDbSet()` que simplifica enormemente el mocking de DbSets.

