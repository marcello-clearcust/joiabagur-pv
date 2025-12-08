# Configuración del Stack de Testing

[← Volver al índice](../../testing-backend.md)

## Stack Tecnológico

| Componente | Tecnología | Versión |
|------------|------------|---------|
| **Framework de Testing** | xUnit | 2.9.x |
| **Mocking** | Moq | 4.20.x |
| **Assertions** | FluentAssertions | 7.x |
| **CI/CD** | GitHub Actions | - |
| **Tests de Integración** | Testcontainers | 4.x |
| **Generación de Datos** | Bogus | 35.x |

---

## ¿Por qué esta Combinación?

### xUnit
- ✅ Estándar de facto en .NET moderno
- ✅ Documentación abundante y comunidad activa
- ✅ Paralelización nativa de tests
- ✅ Microsoft lo usa internamente para ASP.NET Core
- ✅ Template oficial: `dotnet new xunit`

### Moq
- ✅ Librería de mocking más popular en .NET
- ✅ Miles de ejemplos y tutoriales disponibles
- ✅ Sintaxis explícita y clara
- ✅ Soporte completo para async/await

### FluentAssertions
- ✅ Mensajes de error muy descriptivos
- ✅ Sintaxis legible: `result.Should().BeTrue()`
- ✅ Facilita el debugging cuando un test falla
- ✅ Extensible para tipos personalizados

### GitHub Actions
- ✅ 2,000 minutos/mes gratis (repos privados)
- ✅ Ilimitado para repositorios públicos
- ✅ Integración nativa con GitHub
- ✅ Curva de aprendizaje baja

---

## Paquetes NuGet

### Proyecto de Tests Unitarios

```xml
<!-- Joyeria.UnitTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- Framework de testing -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    
    <!-- Mocking -->
    <PackageReference Include="Moq" Version="4.20.72" />
    
    <!-- Assertions legibles -->
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    
    <!-- Generación de datos fake -->
    <PackageReference Include="Bogus" Version="35.6.1" />
  </ItemGroup>

  <ItemGroup>
    <!-- Referencias a proyectos a testear -->
    <ProjectReference Include="..\..\Joyeria.API\Joyeria.API.csproj" />
    <ProjectReference Include="..\..\Joyeria.Core\Joyeria.Core.csproj" />
    <ProjectReference Include="..\..\Joyeria.Infrastructure\Joyeria.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

### Proyecto de Tests de Integración

```xml
<!-- Joyeria.IntegrationTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- Framework de testing -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    
    <!-- Mocking -->
    <PackageReference Include="Moq" Version="4.20.72" />
    
    <!-- Assertions legibles -->
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    
    <!-- Generación de datos fake -->
    <PackageReference Include="Bogus" Version="35.6.1" />
    
    <!-- Tests de integración con API -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />
    
    <!-- Tests de integración con PostgreSQL (Docker) -->
    <PackageReference Include="Testcontainers.PostgreSql" Version="4.1.0" />
    
    <!-- Limpieza de BD entre tests -->
    <PackageReference Include="Respawn" Version="6.2.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Joyeria.API\Joyeria.API.csproj" />
  </ItemGroup>

</Project>
```

---

## Estructura de Proyecto

```
backend/
├── Joyeria.API/
│   ├── Controllers/
│   ├── Program.cs
│   └── Joyeria.API.csproj
├── Joyeria.Core/
│   ├── Entities/
│   ├── Interfaces/
│   ├── DTOs/
│   └── Joyeria.Core.csproj
├── Joyeria.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   └── Joyeria.Infrastructure.csproj
│
└── tests/
    ├── Joyeria.UnitTests/
    │   ├── Services/
    │   │   ├── ProductServiceTests.cs
    │   │   ├── SaleServiceTests.cs
    │   │   ├── InventoryServiceTests.cs
    │   │   └── AuthServiceTests.cs
    │   ├── Validators/
    │   │   ├── ProductValidatorTests.cs
    │   │   └── SaleValidatorTests.cs
    │   ├── Helpers/
    │   │   ├── TestDataBuilder.cs
    │   │   └── MockHelpers.cs
    │   └── Joyeria.UnitTests.csproj
    │
    └── Joyeria.IntegrationTests/
        ├── Api/
        │   ├── ProductsControllerTests.cs
        │   ├── SalesControllerTests.cs
        │   ├── InventoryControllerTests.cs
        │   └── AuthControllerTests.cs
        ├── Repositories/
        │   ├── ProductRepositoryTests.cs
        │   └── SaleRepositoryTests.cs
        ├── Fixtures/
        │   ├── DatabaseFixture.cs
        │   ├── ApiFixture.cs
        │   └── TestDatabaseCollection.cs
        └── Joyeria.IntegrationTests.csproj
```

---

## Crear Proyectos de Test

```bash
# Crear proyecto de tests unitarios
cd backend/tests
dotnet new xunit -n Joyeria.UnitTests
cd Joyeria.UnitTests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Bogus
dotnet add reference ../../Joyeria.Core/Joyeria.Core.csproj
dotnet add reference ../../Joyeria.Infrastructure/Joyeria.Infrastructure.csproj

# Crear proyecto de tests de integración
cd backend/tests
dotnet new xunit -n Joyeria.IntegrationTests
cd Joyeria.IntegrationTests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Bogus
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet add package Respawn
dotnet add reference ../../Joyeria.API/Joyeria.API.csproj
```

---

## Convenciones de Nomenclatura

### Nombres de Tests

```
Método_Escenario_ResultadoEsperado
```

**Ejemplos:**
- `GetProductBySku_WhenProductExists_ShouldReturnProduct`
- `CreateSale_WithInsufficientStock_ShouldThrowException`
- `ValidatePrice_WithNegativeValue_ShouldReturnFalse`
- `Login_WithValidCredentials_ShouldReturnToken`

### Estructura AAA

```csharp
[Fact]
public async Task NombreDelTest()
{
    // Arrange - Preparar datos y mocks
    
    // Act - Ejecutar la acción a testear
    
    // Assert - Verificar resultados
}
```

### Traits/Categorías

```csharp
[Fact]
[Trait("Category", "Unit")]
[Trait("Feature", "Products")]
public async Task ProductTest() { }

[Fact]
[Trait("Category", "Integration")]
[Trait("Feature", "Sales")]
public async Task SalesIntegrationTest() { }
```

---

## Recursos de Aprendizaje

### Documentación Oficial
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [FluentAssertions Documentation](https://fluentassertions.com/introduction)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Testcontainers .NET](https://dotnet.testcontainers.org/)

### Tutoriales Recomendados
- [Testing in .NET (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/core/testing/)
- [Unit Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

