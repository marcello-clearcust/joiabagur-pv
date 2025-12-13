# Testing de Archivos y Uploads

[← Volver al índice](../../testing-backend.md)

## Testing de Importación Excel con ClosedXML

```csharp
using ClosedXML.Excel;
using FluentAssertions;
using Moq;
using Xunit;

namespace Joyeria.UnitTests.Services;

public class ExcelImportServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly ExcelImportService _sut;

    public ExcelImportServiceTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _sut = new ExcelImportService(_mockRepo.Object);
    }

    /// <summary>
    /// Crea un archivo Excel de prueba en memoria
    /// </summary>
    private static MemoryStream CreateTestExcel(List<(string Sku, string Name, decimal Price, int Stock)> products)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Productos");

        // Headers
        worksheet.Cell(1, 1).Value = "SKU";
        worksheet.Cell(1, 2).Value = "Nombre";
        worksheet.Cell(1, 3).Value = "Precio";
        worksheet.Cell(1, 4).Value = "Stock";

        // Datos
        for (int i = 0; i < products.Count; i++)
        {
            var row = i + 2;
            worksheet.Cell(row, 1).Value = products[i].Sku;
            worksheet.Cell(row, 2).Value = products[i].Name;
            worksheet.Cell(row, 3).Value = products[i].Price;
            worksheet.Cell(row, 4).Value = products[i].Stock;
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task ImportProducts_WithValidExcel_ShouldImportAllProducts()
    {
        // Arrange
        var testProducts = new List<(string, string, decimal, int)>
        {
            ("ANI-001", "Anillo Oro", 1500m, 10),
            ("COL-001", "Collar Plata", 800m, 5),
            ("PUL-001", "Pulsera", 500m, 20)
        };

        using var excelStream = CreateTestExcel(testProducts);

        // Act
        var result = await _sut.ImportProductsAsync(excelStream);

        // Assert
        result.SuccessCount.Should().Be(3);
        result.ErrorCount.Should().Be(0);
        _mockRepo.Verify(r => r.AddRangeAsync(It.Is<List<Product>>(
            p => p.Count == 3)), Times.Once);
    }

    [Fact]
    public async Task ImportProducts_WithInvalidRows_ShouldReportErrors()
    {
        // Arrange - incluir datos inválidos
        var testProducts = new List<(string, string, decimal, int)>
        {
            ("ANI-001", "Anillo Oro", 1500m, 10),  // Válido
            ("", "Sin SKU", 800m, 5),              // SKU vacío
            ("PUL-001", "", -100m, -1)             // Múltiples errores
        };

        using var excelStream = CreateTestExcel(testProducts);

        // Act
        var result = await _sut.ImportProductsAsync(excelStream);

        // Assert
        result.SuccessCount.Should().Be(1);
        result.ErrorCount.Should().Be(2);
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ImportProducts_WithEmptyFile_ShouldReturnError()
    {
        // Arrange
        using var excelStream = CreateTestExcel(new());

        // Act
        var result = await _sut.ImportProductsAsync(excelStream);

        // Assert
        result.SuccessCount.Should().Be(0);
        result.ErrorCount.Should().Be(0);
        result.Message.Should().Contain("vacío");
    }

    [Fact]
    public async Task ImportProducts_WithDuplicateSkus_ShouldSkipDuplicates()
    {
        // Arrange
        var testProducts = new List<(string, string, decimal, int)>
        {
            ("ANI-001", "Anillo Original", 1500m, 10),
            ("ANI-001", "Anillo Duplicado", 800m, 5) // SKU duplicado
        };

        _mockRepo.Setup(r => r.ExistsBySkuAsync("ANI-001"))
            .ReturnsAsync(true); // Simular que ya existe

        using var excelStream = CreateTestExcel(testProducts);

        // Act
        var result = await _sut.ImportProductsAsync(excelStream);

        // Assert
        result.SuccessCount.Should().Be(0);
        result.ErrorCount.Should().Be(2);
        result.Errors.Should().AllContain("ya existe");
    }
}
```

---

## Testing de Uploads con MockFileSystem

```csharp
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Joyeria.UnitTests.Services;

public class FileUploadServiceTests
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly FileUploadService _sut;

    public FileUploadServiceTests()
    {
        _mockFileSystem = new MockFileSystem();
        _sut = new FileUploadService(_mockFileSystem);
    }

    [Fact]
    public async Task SaveProductImage_WithValidImage_ShouldSaveToCorrectPath()
    {
        // Arrange
        var imageBytes = CreateTestImageBytes();
        var productSku = "ANI-001";
        var expectedPath = $"/uploads/products/{productSku}.jpg";

        // Act
        var result = await _sut.SaveProductImageAsync(productSku, imageBytes, "image/jpeg");

        // Assert
        result.Should().Be(expectedPath);
        _mockFileSystem.FileExists(expectedPath).Should().BeTrue();
        
        var savedContent = _mockFileSystem.File.ReadAllBytes(expectedPath);
        savedContent.Should().BeEquivalentTo(imageBytes);
    }

    [Fact]
    public async Task SaveProductImage_WithUnsupportedFormat_ShouldThrowException()
    {
        // Arrange
        var imageBytes = new byte[] { 1, 2, 3 };

        // Act & Assert
        await _sut.Invoking(s => s.SaveProductImageAsync("ANI-001", imageBytes, "application/pdf"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*formato*no*soportado*");
    }

    [Fact]
    public async Task SaveProductImage_WithTooLargeFile_ShouldThrowException()
    {
        // Arrange - archivo de 6MB (límite es 5MB)
        var largeImage = new byte[6 * 1024 * 1024];

        // Act & Assert
        await _sut.Invoking(s => s.SaveProductImageAsync("ANI-001", largeImage, "image/jpeg"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*excede*tamaño*máximo*");
    }

    [Fact]
    public async Task DeleteProductImage_WhenExists_ShouldDeleteFile()
    {
        // Arrange
        var imagePath = "/uploads/products/ANI-001.jpg";
        _mockFileSystem.AddFile(imagePath, new MockFileData(new byte[] { 1, 2, 3 }));

        // Act
        var result = await _sut.DeleteProductImageAsync("ANI-001");

        // Assert
        result.Should().BeTrue();
        _mockFileSystem.FileExists(imagePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProductImage_WhenNotExists_ShouldReturnFalse()
    {
        // Act
        var result = await _sut.DeleteProductImageAsync("NO-EXISTE");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    public async Task SaveProductImage_ShouldUseCorrectExtension(string contentType, string expectedExtension)
    {
        // Arrange
        var imageBytes = CreateTestImageBytes();

        // Act
        var result = await _sut.SaveProductImageAsync("TEST-001", imageBytes, contentType);

        // Assert
        result.Should().EndWith(expectedExtension);
    }

    private static byte[] CreateTestImageBytes()
    {
        // Crear bytes mínimos de un JPEG válido (header)
        return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
    }
}
```

---

## Testing de Uploads de API (Integración)

```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Joyeria.IntegrationTests.Api;

[Collection("Api")]
public class FileUploadControllerTests
{
    private readonly HttpClient _client;

    public FileUploadControllerTests(ApiFixture fixture)
    {
        _client = fixture.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task UploadProductImage_WithValidImage_ShouldReturnOk()
    {
        // Arrange
        var imageContent = CreateTestImageContent();
        using var content = new MultipartFormDataContent();
        content.Add(imageContent, "file", "test-image.jpg");

        // Act
        var response = await _client.PostAsync("/api/products/ANI-001/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<UploadResultDto>();
        result!.Url.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UploadProductImage_WithInvalidFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var textContent = new StringContent("not an image");
        using var content = new MultipartFormDataContent();
        content.Add(textContent, "file", "test.txt");

        // Act
        var response = await _client.PostAsync("/api/products/ANI-001/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadProductImage_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var unauthorizedClient = new HttpClient();
        var imageContent = CreateTestImageContent();
        using var content = new MultipartFormDataContent();
        content.Add(imageContent, "file", "test.jpg");

        // Act
        var response = await unauthorizedClient.PostAsync(
            "http://localhost/api/products/ANI-001/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static ByteArrayContent CreateTestImageContent()
    {
        // Crear un JPEG mínimo válido
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        var content = new ByteArrayContent(jpegBytes);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        return content;
    }
}
```

---

## Testing de Almacenamiento en la Nube (S3/Azure Blob)

```csharp
using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Moq;
using Xunit;

namespace Joyeria.UnitTests.Services;

public class S3StorageServiceTests
{
    private readonly Mock<IAmazonS3> _mockS3Client;
    private readonly S3StorageService _sut;

    public S3StorageServiceTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _sut = new S3StorageService(_mockS3Client.Object, "test-bucket");
    }

    [Fact]
    public async Task UploadFile_ShouldCallS3WithCorrectParameters()
    {
        // Arrange
        var fileBytes = new byte[] { 1, 2, 3 };
        var fileName = "products/ANI-001.jpg";

        _mockS3Client.Setup(s => s.PutObjectAsync(
            It.IsAny<PutObjectRequest>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _sut.UploadAsync(fileName, fileBytes, "image/jpeg");

        // Assert
        _mockS3Client.Verify(s => s.PutObjectAsync(
            It.Is<PutObjectRequest>(r =>
                r.BucketName == "test-bucket" &&
                r.Key == fileName &&
                r.ContentType == "image/jpeg"),
            It.IsAny<CancellationToken>()), 
            Times.Once);

        result.Should().Contain(fileName);
    }

    [Fact]
    public async Task DeleteFile_ShouldCallS3DeleteObject()
    {
        // Arrange
        var fileName = "products/ANI-001.jpg";

        _mockS3Client.Setup(s => s.DeleteObjectAsync(
            It.IsAny<DeleteObjectRequest>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteObjectResponse());

        // Act
        await _sut.DeleteAsync(fileName);

        // Assert
        _mockS3Client.Verify(s => s.DeleteObjectAsync(
            It.Is<DeleteObjectRequest>(r =>
                r.BucketName == "test-bucket" &&
                r.Key == fileName),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSignedUrl_ShouldReturnValidUrl()
    {
        // Arrange
        var fileName = "products/ANI-001.jpg";
        var expectedUrl = "https://test-bucket.s3.amazonaws.com/products/ANI-001.jpg?signed";

        _mockS3Client.Setup(s => s.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Returns(expectedUrl);

        // Act
        var result = await _sut.GetSignedUrlAsync(fileName, TimeSpan.FromHours(1));

        // Assert
        result.Should().Be(expectedUrl);
    }
}
```

---

## Paquetes Necesarios

```xml
<!-- Para manipular Excel -->
<PackageReference Include="ClosedXML" Version="0.104.1" />

<!-- Para mockear filesystem -->
<PackageReference Include="System.IO.Abstractions" Version="21.0.29" />
<PackageReference Include="System.IO.Abstractions.TestingHelpers" Version="21.0.29" />

<!-- Para tests con S3 (opcional) -->
<PackageReference Include="AWSSDK.S3" Version="3.7.400" />
<PackageReference Include="Moq" Version="4.20.72" />
```

---

## Configuración del Servicio con IFileSystem

```csharp
// Startup/Program.cs
public static void ConfigureServices(IServiceCollection services)
{
    // Registrar IFileSystem para poder mockear en tests
    services.AddSingleton<IFileSystem, FileSystem>();
    services.AddScoped<IFileUploadService, FileUploadService>();
}

// FileUploadService.cs
public class FileUploadService : IFileUploadService
{
    private readonly IFileSystem _fileSystem;
    private readonly string _uploadPath;

    public FileUploadService(IFileSystem fileSystem, IConfiguration config)
    {
        _fileSystem = fileSystem;
        _uploadPath = config["Storage:LocalPath"] ?? "/uploads";
    }

    public async Task<string> SaveProductImageAsync(string sku, byte[] imageBytes, string contentType)
    {
        var extension = GetExtension(contentType);
        var filePath = _fileSystem.Path.Combine(_uploadPath, "products", $"{sku}{extension}");
        
        // Crear directorio si no existe
        var directory = _fileSystem.Path.GetDirectoryName(filePath);
        if (!_fileSystem.Directory.Exists(directory))
            _fileSystem.Directory.CreateDirectory(directory!);
        
        await _fileSystem.File.WriteAllBytesAsync(filePath, imageBytes);
        
        return filePath;
    }
}
```

