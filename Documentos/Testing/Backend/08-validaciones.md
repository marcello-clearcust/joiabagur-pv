# Testing de Validaciones

[← Volver al índice](../../testing-backend.md)

## Testing con FluentValidation.TestHelper

```csharp
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Joyeria.UnitTests.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    #region SKU Validations

    [Fact]
    public void Sku_WhenEmpty_ShouldHaveError()
    {
        var model = new CreateProductDto { Sku = "" };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage("El SKU es requerido");
    }

    [Fact]
    public void Sku_WhenTooShort_ShouldHaveError()
    {
        var model = new CreateProductDto { Sku = "AB" };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage("El SKU debe tener al menos 5 caracteres");
    }

    [Fact]
    public void Sku_WhenValid_ShouldNotHaveError()
    {
        var model = new CreateProductDto { Sku = "ANI-001" };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldNotHaveValidationErrorFor(x => x.Sku);
    }

    [Theory]
    [InlineData("ANI-001")]
    [InlineData("COL-123")]
    [InlineData("PUL-999-A")]
    public void Sku_WithValidFormat_ShouldNotHaveError(string sku)
    {
        var model = new CreateProductDto { Sku = sku };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldNotHaveValidationErrorFor(x => x.Sku);
    }

    #endregion

    #region Price Validations

    [Fact]
    public void Price_WhenZero_ShouldHaveError()
    {
        var model = new CreateProductDto { Price = 0 };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("El precio debe ser mayor a cero");
    }

    [Fact]
    public void Price_WhenNegative_ShouldHaveError()
    {
        var model = new CreateProductDto { Price = -100 };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(9999.99)]
    public void Price_WhenPositive_ShouldNotHaveError(decimal price)
    {
        var model = new CreateProductDto { Price = price };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    #endregion

    #region Stock Validations

    [Fact]
    public void Stock_WhenNegative_ShouldHaveError()
    {
        var model = new CreateProductDto { Stock = -1 };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldHaveValidationErrorFor(x => x.Stock)
            .WithErrorMessage("El stock no puede ser negativo");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Stock_WhenZeroOrPositive_ShouldNotHaveError(int stock)
    {
        var model = new CreateProductDto { Stock = stock };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldNotHaveValidationErrorFor(x => x.Stock);
    }

    #endregion

    #region Complete Model Validation

    [Fact]
    public void Validate_WithAllFieldsValid_ShouldBeValid()
    {
        var model = new CreateProductDto
        {
            Sku = "ANI-001",
            Name = "Anillo de Oro",
            Price = 1500.00m,
            Stock = 10,
            Category = "Anillos"
        };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldHaveAllErrors()
    {
        var model = new CreateProductDto
        {
            Sku = "",
            Name = "",
            Price = -100,
            Stock = -5
        };
        
        var result = _validator.TestValidate(model);
        
        result.ShouldHaveValidationErrorFor(x => x.Sku);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Price);
        result.ShouldHaveValidationErrorFor(x => x.Stock);
    }

    #endregion
}
```

---

## Testing de DataAnnotations

```csharp
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Xunit;

namespace Joyeria.UnitTests.Validators;

public class ProductDtoDataAnnotationsTests
{
    private List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
        return validationResults;
    }

    [Fact]
    public void Validate_WithValidData_ShouldHaveNoErrors()
    {
        var dto = new CreateProductDto
        {
            Sku = "ANI-001",
            Name = "Anillo de Oro",
            Price = 1500.00m,
            Stock = 10
        };

        var results = ValidateModel(dto);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptySku_ShouldHaveError()
    {
        var dto = new CreateProductDto
        {
            Sku = "",
            Name = "Anillo",
            Price = 100
        };

        var results = ValidateModel(dto);

        results.Should().ContainSingle(r => 
            r.MemberNames.Contains("Sku"));
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveError()
    {
        var dto = new CreateProductDto
        {
            Sku = "ANI-001",
            Name = new string('A', 201), // Max 200 chars
            Price = 100
        };

        var results = ValidateModel(dto);

        results.Should().ContainSingle(r => 
            r.MemberNames.Contains("Name"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_WithInvalidPrice_ShouldHaveError(decimal price)
    {
        var dto = new CreateProductDto
        {
            Sku = "ANI-001",
            Name = "Anillo",
            Price = price
        };

        var results = ValidateModel(dto);

        results.Should().Contain(r => 
            r.MemberNames.Contains("Price"));
    }
}
```

---

## Testing de Validaciones Asíncronas

```csharp
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace Joyeria.UnitTests.Validators;

public class CreateProductAsyncValidatorTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly CreateProductValidator _validator;

    public CreateProductAsyncValidatorTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _validator = new CreateProductValidator(_mockRepo.Object);
    }

    [Fact]
    public async Task Sku_WhenAlreadyExists_ShouldHaveError()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsBySkuAsync("ANI-001"))
            .ReturnsAsync(true);

        var model = new CreateProductDto { Sku = "ANI-001" };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage("Ya existe un producto con este SKU");
    }

    [Fact]
    public async Task Sku_WhenUnique_ShouldNotHaveError()
    {
        // Arrange
        _mockRepo.Setup(r => r.ExistsBySkuAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var model = new CreateProductDto { Sku = "NEW-001" };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Sku);
    }

    [Fact]
    public async Task Category_WhenNotExists_ShouldHaveError()
    {
        // Arrange
        _mockRepo.Setup(r => r.CategoryExistsAsync("Inexistente"))
            .ReturnsAsync(false);

        var model = new CreateProductDto 
        { 
            Sku = "ANI-001",
            Category = "Inexistente" 
        };

        // Act
        var result = await _validator.TestValidateAsync(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("La categoría no existe");
    }
}
```

---

## Test de Integración para Validación de Modelo

```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Joyeria.IntegrationTests.Api;

[Collection("Api")]
public class ProductValidationIntegrationTests
{
    private readonly HttpClient _client;

    public ProductValidationIntegrationTests(ApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithEmptySku_ShouldReturnBadRequest()
    {
        var product = new CreateProductDto
        {
            Sku = "",
            Name = "Producto",
            Price = 100
        };

        var response = await _client.PostAsJsonAsync("/api/products", product);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errors = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        errors!.Errors.Should().ContainKey("Sku");
    }

    [Fact]
    public async Task CreateProduct_WithMultipleErrors_ShouldReturnAllErrors()
    {
        var product = new CreateProductDto
        {
            Sku = "",
            Name = "",
            Price = -100
        };

        var response = await _client.PostAsJsonAsync("/api/products", product);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errors = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        errors!.Errors.Should().ContainKeys("Sku", "Name", "Price");
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturnCreated()
    {
        var product = new CreateProductDto
        {
            Sku = "TEST-VALID-001",
            Name = "Producto Válido",
            Price = 100.00m,
            Stock = 10
        };

        var response = await _client.PostAsJsonAsync("/api/products", product);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}

// Modelo para deserializar errores de validación
public class ValidationErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}
```

---

## Paquetes Necesarios

```xml
<!-- FluentValidation y su helper de testing -->
<PackageReference Include="FluentValidation" Version="11.10.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.10.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

<!-- En el proyecto de tests -->
<PackageReference Include="FluentValidation.TestHelper" Version="11.10.0" />
```

