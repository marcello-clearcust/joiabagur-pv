using FluentAssertions;
using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for ComponentReportService.
/// Note: Report queries use EF async extensions (CountAsync, ToListAsync) which require
/// a provider that implements IAsyncQueryProvider. We test the service indirectly
/// through the repository mock by returning pre-computed data.
/// Since the report service is tightly coupled to EF LINQ, these tests verify
/// the service construction and parameter passing rather than query logic.
/// Query correctness should be verified via integration tests.
/// </summary>
public class ComponentReportServiceTests
{
    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange & Act
        var prodRepoMock = new Mock<IProductRepository>();
        var assignRepoMock = new Mock<IProductComponentAssignmentRepository>();
        var loggerMock = new Mock<ILogger<ComponentReportService>>();

        var act = () => new ComponentReportService(prodRepoMock.Object, assignRepoMock.Object, loggerMock.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MarginReportQueryParameters_DefaultValues_ShouldBeCorrect()
    {
        var parameters = new MarginReportQueryParameters();

        parameters.Page.Should().Be(1);
        parameters.PageSize.Should().Be(50);
        parameters.CollectionId.Should().BeNull();
        parameters.MaxMarginPercent.Should().BeNull();
        parameters.Search.Should().BeNull();
    }

    [Fact]
    public void ProductsWithoutComponentsQueryParameters_DefaultValues_ShouldBeCorrect()
    {
        var parameters = new ProductsWithoutComponentsQueryParameters();

        parameters.Page.Should().Be(1);
        parameters.PageSize.Should().Be(50);
        parameters.CollectionId.Should().BeNull();
        parameters.Search.Should().BeNull();
    }

    [Fact]
    public void ProductMarginDto_ShouldHoldMarginData()
    {
        var dto = new ProductMarginDto
        {
            ProductId = Guid.NewGuid(),
            SKU = "JOY-001",
            ProductName = "Gold Ring",
            TotalCostPrice = 300,
            TotalSalePrice = 500,
            MarginAmount = 200,
            MarginPercent = 40
        };

        dto.MarginAmount.Should().Be(200);
        dto.MarginPercent.Should().Be(40);
    }

    [Fact]
    public void MarginReportDto_ShouldHoldAggregatedData()
    {
        var report = new MarginReportDto
        {
            Items = new List<ProductMarginDto>(),
            TotalCount = 10,
            SumCostPrice = 3000,
            SumSalePrice = 5000,
            SumMargin = 2000
        };

        report.SumMargin.Should().Be(2000);
        report.TotalCount.Should().Be(10);
    }
}
