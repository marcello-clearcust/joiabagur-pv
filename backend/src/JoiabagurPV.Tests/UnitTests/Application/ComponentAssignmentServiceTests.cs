using FluentAssertions;
using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

public class ComponentAssignmentServiceTests
{
    private readonly Mock<IProductComponentAssignmentRepository> _assignRepoMock;
    private readonly Mock<IProductComponentRepository> _compRepoMock;
    private readonly Mock<IProductRepository> _prodRepoMock;
    private readonly Mock<IComponentTemplateRepository> _tmplRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<ComponentAssignmentService>> _loggerMock;
    private readonly ComponentAssignmentService _sut;

    public ComponentAssignmentServiceTests()
    {
        _assignRepoMock = new Mock<IProductComponentAssignmentRepository>();
        _compRepoMock = new Mock<IProductComponentRepository>();
        _prodRepoMock = new Mock<IProductRepository>();
        _tmplRepoMock = new Mock<IComponentTemplateRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ComponentAssignmentService>>();
        _sut = new ComponentAssignmentService(
            _assignRepoMock.Object, _compRepoMock.Object, _prodRepoMock.Object,
            _tmplRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    #region SaveAssignmentsAsync

    [Fact]
    public async Task SaveAssignmentsAsync_WithValidData_ShouldReplaceAssignments()
    {
        var productId = Guid.NewGuid();
        var componentId = Guid.NewGuid();
        _prodRepoMock.Setup(r => r.ExistsAsync(productId)).ReturnsAsync(true);
        _compRepoMock.Setup(r => r.ExistsAsync(componentId)).ReturnsAsync(true);
        _assignRepoMock.Setup(r => r.GetByProductIdAsync(productId))
            .ReturnsAsync(new List<ProductComponentAssignment>
            {
                new() { Id = Guid.NewGuid(), ComponentId = componentId, Quantity = 1, CostPrice = 10, SalePrice = 20, DisplayOrder = 0,
                    Component = new ProductComponent { Description = "Oro", CostPrice = 10, SalePrice = 20 } }
            });

        var request = new SaveComponentAssignmentsRequest
        {
            Assignments = new()
            {
                new() { ComponentId = componentId, Quantity = 3, CostPrice = 150, SalePrice = 200, DisplayOrder = 0 }
            }
        };

        var result = await _sut.SaveAssignmentsAsync(productId, request);

        result.Should().HaveCount(1);
        _assignRepoMock.Verify(r => r.RemoveAllByProductIdAsync(productId), Times.Once);
        _assignRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ProductComponentAssignment>>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveAssignmentsAsync_WithDuplicateComponents_ShouldThrow()
    {
        var productId = Guid.NewGuid();
        var compId = Guid.NewGuid();
        _prodRepoMock.Setup(r => r.ExistsAsync(productId)).ReturnsAsync(true);

        var request = new SaveComponentAssignmentsRequest
        {
            Assignments = new()
            {
                new() { ComponentId = compId, Quantity = 1, CostPrice = 10, SalePrice = 20 },
                new() { ComponentId = compId, Quantity = 2, CostPrice = 10, SalePrice = 20 }
            }
        };

        var act = () => _sut.SaveAssignmentsAsync(productId, request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*mismo componente*");
    }

    [Fact]
    public async Task SaveAssignmentsAsync_WithNonExistentProduct_ShouldThrow()
    {
        _prodRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var act = () => _sut.SaveAssignmentsAsync(Guid.NewGuid(), new SaveComponentAssignmentsRequest());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SaveAssignmentsAsync_WithZeroQuantity_ShouldThrow()
    {
        var productId = Guid.NewGuid();
        _prodRepoMock.Setup(r => r.ExistsAsync(productId)).ReturnsAsync(true);

        var request = new SaveComponentAssignmentsRequest
        {
            Assignments = new() { new() { ComponentId = Guid.NewGuid(), Quantity = 0, CostPrice = 10, SalePrice = 20 } }
        };

        var act = () => _sut.SaveAssignmentsAsync(productId, request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*cantidad*");
    }

    #endregion

    #region ApplySyncFromMasterAsync

    [Fact]
    public async Task ApplySyncFromMasterAsync_ShouldUpdatePricesFromMaster()
    {
        var productId = Guid.NewGuid();
        var comp = new ProductComponent { Id = Guid.NewGuid(), Description = "Oro", CostPrice = 160, SalePrice = 210 };
        var assignment = new ProductComponentAssignment
        {
            Id = Guid.NewGuid(), ProductId = productId, ComponentId = comp.Id,
            CostPrice = 150, SalePrice = 200, Quantity = 1, Component = comp
        };

        _assignRepoMock.Setup(r => r.GetByProductIdAsync(productId)).ReturnsAsync(new List<ProductComponentAssignment> { assignment });
        _assignRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ProductComponentAssignment>()))
            .ReturnsAsync((ProductComponentAssignment a) => a);

        var result = await _sut.ApplySyncFromMasterAsync(productId);

        assignment.CostPrice.Should().Be(160);
        assignment.SalePrice.Should().Be(210);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApplySyncFromMasterAsync_SkipsComponentsWithoutMasterPrices()
    {
        var productId = Guid.NewGuid();
        var comp = new ProductComponent { Id = Guid.NewGuid(), Description = "Diamante", CostPrice = null, SalePrice = null };
        var assignment = new ProductComponentAssignment
        {
            Id = Guid.NewGuid(), ProductId = productId, ComponentId = comp.Id,
            CostPrice = 100, SalePrice = 150, Quantity = 1, Component = comp
        };

        _assignRepoMock.Setup(r => r.GetByProductIdAsync(productId)).ReturnsAsync(new List<ProductComponentAssignment> { assignment });

        await _sut.ApplySyncFromMasterAsync(productId);

        assignment.CostPrice.Should().Be(100);
        assignment.SalePrice.Should().Be(150);
    }

    #endregion

    #region ApplyTemplateAsync

    [Fact]
    public async Task ApplyTemplateAsync_ShouldMergeAndSkipExisting()
    {
        var productId = Guid.NewGuid();
        var existingCompId = Guid.NewGuid();
        var newCompId = Guid.NewGuid();

        _prodRepoMock.Setup(r => r.ExistsAsync(productId)).ReturnsAsync(true);

        var existingComp = new ProductComponent { Id = existingCompId, Description = "Oro", CostPrice = 150, SalePrice = 200 };
        var newComp = new ProductComponent { Id = newCompId, Description = "Plata", CostPrice = 12, SalePrice = 22 };

        var template = new ComponentTemplate
        {
            Id = Guid.NewGuid(), Name = "Anillo",
            Items = new List<ComponentTemplateItem>
            {
                new() { ComponentId = existingCompId, Quantity = 3, Component = existingComp },
                new() { ComponentId = newCompId, Quantity = 1, Component = newComp }
            }
        };

        _tmplRepoMock.Setup(r => r.GetWithItemsAsync(template.Id)).ReturnsAsync(template);
        _assignRepoMock.Setup(r => r.GetByProductIdAsync(productId)).ReturnsAsync(new List<ProductComponentAssignment>
        {
            new() { ComponentId = existingCompId, Quantity = 2, CostPrice = 140, SalePrice = 190, DisplayOrder = 0,
                Component = existingComp }
        });

        var result = await _sut.ApplyTemplateAsync(productId, template.Id);

        result.AddedComponents.Should().Contain("Plata");
        result.SkippedComponents.Should().Contain("Oro");
        _assignRepoMock.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<ProductComponentAssignment>>(
            a => a.Count() == 1 && a.First().ComponentId == newCompId)), Times.Once);
    }

    #endregion
}
