# Testcontainers + PostgreSQL

[← Volver al índice](../../testing-backend.md)

## Fixture Básica para Base de Datos

```csharp
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Joyeria.Infrastructure.Data;
using Xunit;

namespace Joyeria.IntegrationTests.Fixtures;

/// <summary>
/// Fixture que levanta un contenedor PostgreSQL para tests de integración
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    public JoyeriaDbContext DbContext { get; private set; } = null!;
    public string ConnectionString => _postgresContainer.GetConnectionString();

    public DatabaseFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("joyeria_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Iniciar contenedor
        await _postgresContainer.StartAsync();

        // Crear DbContext
        var options = new DbContextOptionsBuilder<JoyeriaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        DbContext = new JoyeriaDbContext(options);

        // Aplicar migraciones
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    /// <summary>
    /// Limpia todas las tablas entre tests
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        // Usar Respawn para limpiar datos manteniendo estructura
        var respawner = await Respawn.Respawner.CreateAsync(
            ConnectionString,
            new Respawn.RespawnerOptions
            {
                DbAdapter = Respawn.DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            }
        );

        await respawner.ResetAsync(ConnectionString);
    }
}

/// <summary>
/// Colección para compartir el fixture entre tests
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
```

---

## Configuración Avanzada del Contenedor

```csharp
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Joyeria.Infrastructure.Data;
using Xunit;

namespace Joyeria.IntegrationTests.Fixtures;

/// <summary>
/// Fixture avanzada con configuración completa de PostgreSQL
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private Respawn.Respawner? _respawner;
    
    public JoyeriaDbContext DbContext { get; private set; } = null!;
    public string ConnectionString => _postgresContainer.GetConnectionString();

    public DatabaseFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("joyeria_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            // Configuración de recursos (opcional para CI)
            .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "trust")
            // Configuración de performance para tests
            .WithCommand(
                "-c", "fsync=off",
                "-c", "synchronous_commit=off",
                "-c", "full_page_writes=off"
            )
            // Timeout de espera
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilPortIsAvailable(5432)
            )
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Iniciar contenedor
        await _postgresContainer.StartAsync();

        // Crear DbContext con opciones optimizadas para tests
        var options = new DbContextOptionsBuilder<JoyeriaDbContext>()
            .UseNpgsql(ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
            })
            .EnableSensitiveDataLogging() // Útil para debugging
            .EnableDetailedErrors()
            .Options;

        DbContext = new JoyeriaDbContext(options);

        // Aplicar migraciones
        await DbContext.Database.MigrateAsync();

        // Configurar Respawner para limpieza
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        _respawner = await Respawn.Respawner.CreateAsync(connection, new Respawn.RespawnerOptions
        {
            DbAdapter = Respawn.DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            // Tablas que NO se deben limpiar (ej: datos de configuración)
            TablesToIgnore = new Respawn.Graph.Table[]
            {
                "__EFMigrationsHistory"
            }
        });

        // Seed inicial de datos
        await SeedInitialDataAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    /// <summary>
    /// Limpia todas las tablas entre tests (muy rápido)
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
        
        // Re-seed datos base después de limpiar
        await SeedInitialDataAsync();
    }

    /// <summary>
    /// Seed de datos iniciales necesarios para todos los tests
    /// </summary>
    private async Task SeedInitialDataAsync()
    {
        // Verificar si ya existen datos
        if (await DbContext.Users.AnyAsync())
            return;

        // Usuario admin para tests de autenticación
        var adminUser = new User
        {
            Id = "admin-test-id",
            Username = "admin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            FullName = "Admin Test",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Usuario operador para tests de roles
        var operatorUser = new User
        {
            Id = "operator-test-id",
            Username = "operador",
            Email = "operador@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("operador123"),
            FullName = "Operador Test",
            Role = "Operador",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Punto de venta para tests
        var pointOfSale = new PointOfSale
        {
            Id = 1,
            Name = "Tienda Principal Test",
            Address = "Calle Test 123",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Users.AddRange(adminUser, operatorUser);
        DbContext.PointsOfSale.Add(pointOfSale);
        await DbContext.SaveChangesAsync();

        // Limpiar el tracker para evitar conflictos
        DbContext.ChangeTracker.Clear();
    }

    /// <summary>
    /// Crear un nuevo DbContext para tests que necesitan contextos separados
    /// </summary>
    public JoyeriaDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<JoyeriaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new JoyeriaDbContext(options);
    }
}
```

---

## Ejecución de Scripts SQL Personalizados

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    // ... código anterior ...

    /// <summary>
    /// Ejecutar script SQL directo (útil para datos complejos)
    /// </summary>
    public async Task ExecuteSqlAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Ejecutar script SQL desde archivo
    /// </summary>
    public async Task ExecuteSqlFileAsync(string filePath)
    {
        var sql = await File.ReadAllTextAsync(filePath);
        await ExecuteSqlAsync(sql);
    }

    /// <summary>
    /// Insertar datos masivos para tests de rendimiento
    /// </summary>
    public async Task BulkInsertProductsAsync(int count)
    {
        var products = TestDataBuilder.CreateProducts(count);
        
        // Usar EF Core bulk insert
        await DbContext.Products.AddRangeAsync(products);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();
    }
}
```

---

## Compartir Contenedor Entre Tests (Optimización)

```csharp
using Xunit;

namespace Joyeria.IntegrationTests.Fixtures;

/// <summary>
/// Colección que comparte un único contenedor entre todos los tests
/// Reduce tiempo de inicio significativamente
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Esta clase no tiene código
    // Solo sirve para definir la colección
}

/// <summary>
/// Clase base para tests que necesitan la BD
/// </summary>
public abstract class DatabaseTestBase : IAsyncLifetime
{
    protected readonly DatabaseFixture Fixture;
    protected JoyeriaDbContext DbContext => Fixture.DbContext;

    protected DatabaseTestBase(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;
}

// Uso en tests
[Collection("Database")]
public class ProductRepositoryTests : DatabaseTestBase
{
    private readonly ProductRepository _repository;

    public ProductRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
        _repository = new ProductRepository(DbContext);
    }

    [Fact]
    public async Task Add_ShouldPersistProduct()
    {
        // El fixture ya está limpio y con seed data
        var product = TestDataBuilder.CreateProduct();
        await _repository.AddAsync(product);
        await DbContext.SaveChangesAsync();
        // ...
    }
}
```

---

## Test de Integración de Repositorio

```csharp
using FluentAssertions;
using Xunit;
using Joyeria.IntegrationTests.Fixtures;
using Joyeria.Infrastructure.Repositories;
using Joyeria.UnitTests.Helpers;

namespace Joyeria.IntegrationTests.Repositories;

[Collection("Database")]
public class ProductRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProductRepository(_fixture.DbContext);
    }

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Add_ShouldPersistProduct()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct();

        // Act
        await _repository.AddAsync(product);
        await _fixture.DbContext.SaveChangesAsync();

        // Assert
        var savedProduct = await _repository.GetByIdAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Sku.Should().Be(product.Sku);
        savedProduct.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetBySku_WhenExists_ShouldReturnProduct()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct();
        await _repository.AddAsync(product);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync(product.Sku);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetBySku_WhenNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySkuAsync("NO-EXISTE-999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldModifyExistingProduct()
    {
        // Arrange
        var product = TestDataBuilder.CreateProduct();
        await _repository.AddAsync(product);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        product.Name = "Nombre Actualizado";
        product.Price = 9999.99m;
        _repository.Update(product);
        await _fixture.DbContext.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(product.Id);
        updated!.Name.Should().Be("Nombre Actualizado");
        updated.Price.Should().Be(9999.99m);
    }
}
```

---

## Fixture para Tests de API

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Joyeria.Infrastructure.Data;

namespace Joyeria.IntegrationTests.Fixtures;

/// <summary>
/// Factory personalizada para tests de integración de API
/// </summary>
public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;

    public ApiFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("joyeria_api_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover el DbContext original
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<JoyeriaDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Añadir DbContext con conexión al contenedor de test
            services.AddDbContext<JoyeriaDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Crear la base de datos y aplicar migraciones
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JoyeriaDbContext>();
            db.Database.Migrate();
        });

        builder.UseEnvironment("Testing");
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
}
```

---

## Test de Integración de Controller

```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Joyeria.IntegrationTests.Fixtures;
using Joyeria.Core.DTOs;

namespace Joyeria.IntegrationTests.Api;

[Collection("Api")]
public class ProductsControllerTests
{
    private readonly HttpClient _client;

    public ProductsControllerTests(ApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOkWithList()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var newProduct = new CreateProductDto
        {
            Sku = "TEST-001",
            Name = "Producto de Test",
            Price = 100.00m,
            Stock = 10,
            Category = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        created.Should().NotBeNull();
        created!.Sku.Should().Be("TEST-001");
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ShouldReturnConflict()
    {
        // Arrange
        var product = new CreateProductDto
        {
            Sku = "DUP-001",
            Name = "Producto Original",
            Price = 100.00m
        };

        // Crear el primero
        await _client.PostAsJsonAsync("/api/products", product);

        // Act - Intentar crear duplicado
        var response = await _client.PostAsJsonAsync("/api/products", product);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
```

