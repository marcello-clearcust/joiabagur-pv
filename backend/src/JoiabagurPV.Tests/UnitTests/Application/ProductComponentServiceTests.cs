using FluentAssertions;
using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

public class ProductComponentServiceTests
{
    private readonly Mock<IProductComponentRepository> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<ProductComponentService>> _loggerMock;
    private readonly ProductComponentService _sut;

    public ProductComponentServiceTests()
    {
        _repoMock = new Mock<IProductComponentRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductComponentService>>();
        _sut = new ProductComponentService(_repoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateComponent()
    {
        var request = new CreateComponentRequest { Description = "Oro 18k", CostPrice = 150m, SalePrice = 200m };
        _repoMock.Setup(r => r.DescriptionExistsAsync("Oro 18k", null)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<ProductComponent>())).ReturnsAsync((ProductComponent c) => c);

        var result = await _sut.CreateAsync(request);

        result.Description.Should().Be("Oro 18k");
        result.CostPrice.Should().Be(150m);
        result.IsActive.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithoutPrices_ShouldCreateComponentWithNullPrices()
    {
        var request = new CreateComponentRequest { Description = "Diamante" };
        _repoMock.Setup(r => r.DescriptionExistsAsync("Diamante", null)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<ProductComponent>())).ReturnsAsync((ProductComponent c) => c);

        var result = await _sut.CreateAsync(request);

        result.CostPrice.Should().BeNull();
        result.SalePrice.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateDescription_ShouldThrow()
    {
        var request = new CreateComponentRequest { Description = "Oro 18k" };
        _repoMock.Setup(r => r.DescriptionExistsAsync("Oro 18k", null)).ReturnsAsync(true);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ya existe*");
    }

    [Fact]
    public async Task CreateAsync_WithEmptyDescription_ShouldThrow()
    {
        var request = new CreateComponentRequest { Description = "" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*obligatoria*");
    }

    [Fact]
    public async Task CreateAsync_WithDescriptionOver35Chars_ShouldThrow()
    {
        var request = new CreateComponentRequest { Description = new string('A', 36) };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*35*");
    }

    [Fact]
    public async Task CreateAsync_WithNegativeCostPrice_ShouldThrow()
    {
        var request = new CreateComponentRequest { Description = "Test", CostPrice = -1m };
        _repoMock.Setup(r => r.DescriptionExistsAsync("Test", null)).ReturnsAsync(false);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*coste*");
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateComponent()
    {
        var id = Guid.NewGuid();
        var existing = new ProductComponent { Id = id, Description = "Old", IsActive = true };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.DescriptionExistsAsync("New", id)).ReturnsAsync(false);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ProductComponent>())).ReturnsAsync((ProductComponent c) => c);

        var result = await _sut.UpdateAsync(id, new UpdateComponentRequest { Description = "New", CostPrice = 10m, SalePrice = 20m, IsActive = true });

        result.Description.Should().Be("New");
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldThrow()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductComponent?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UpdateComponentRequest { Description = "X", IsActive = true });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region SearchAsync

    [Fact]
    public async Task SearchAsync_WithShortQuery_ShouldReturnEmpty()
    {
        var result = await _sut.SearchAsync("O");

        result.Should().BeEmpty();
        _repoMock.Verify(r => r.SearchActiveAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ShouldReturnResults()
    {
        var components = new List<ProductComponent> { new() { Description = "Oro 18k", IsActive = true } };
        _repoMock.Setup(r => r.SearchActiveAsync("Or", 20)).ReturnsAsync(components);

        var result = await _sut.SearchAsync("Or");

        result.Should().HaveCount(1);
        result[0].Description.Should().Be("Oro 18k");
    }

    #endregion
}
