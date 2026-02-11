using FluentAssertions;
using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

public class ComponentTemplateServiceTests
{
    private readonly Mock<IComponentTemplateRepository> _tmplRepoMock;
    private readonly Mock<IProductComponentRepository> _compRepoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<ComponentTemplateService>> _loggerMock;
    private readonly ComponentTemplateService _sut;

    public ComponentTemplateServiceTests()
    {
        _tmplRepoMock = new Mock<IComponentTemplateRepository>();
        _compRepoMock = new Mock<IProductComponentRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ComponentTemplateService>>();
        _sut = new ComponentTemplateService(_tmplRepoMock.Object, _compRepoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateTemplate()
    {
        var request = new SaveComponentTemplateRequest
        {
            Name = "Anillo oro",
            Description = "Template for gold rings",
            Items = new() { new() { ComponentId = Guid.NewGuid(), Quantity = 3 } }
        };

        _tmplRepoMock.Setup(r => r.AddAsync(It.IsAny<ComponentTemplate>())).ReturnsAsync((ComponentTemplate t) => t);
        _tmplRepoMock.Setup(r => r.GetWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync(
            new ComponentTemplate
            {
                Name = "Anillo oro", Description = "Template for gold rings",
                Items = new List<ComponentTemplateItem>
                {
                    new() { ComponentId = request.Items[0].ComponentId, Quantity = 3,
                        Component = new ProductComponent { Description = "Oro" } }
                }
            });

        var result = await _sut.CreateAsync(request);

        result.Name.Should().Be("Anillo oro");
        result.Items.Should().HaveCount(1);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ShouldThrow()
    {
        var request = new SaveComponentTemplateRequest { Name = "", Items = new() };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*obligatorio*");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateComponents_ShouldThrow()
    {
        var compId = Guid.NewGuid();
        var request = new SaveComponentTemplateRequest
        {
            Name = "Test",
            Items = new() { new() { ComponentId = compId, Quantity = 1 }, new() { ComponentId = compId, Quantity = 2 } }
        };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*repetir*");
    }

    [Fact]
    public async Task CreateAsync_WithZeroQuantity_ShouldThrow()
    {
        var request = new SaveComponentTemplateRequest
        {
            Name = "Test",
            Items = new() { new() { ComponentId = Guid.NewGuid(), Quantity = 0 } }
        };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*cantidad*");
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldThrow()
    {
        _tmplRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDelete()
    {
        var id = Guid.NewGuid();
        _tmplRepoMock.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _tmplRepoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        await _sut.DeleteAsync(id);

        _tmplRepoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTemplates()
    {
        _tmplRepoMock.Setup(r => r.GetAllWithItemsAsync()).ReturnsAsync(new List<ComponentTemplate>
        {
            new() { Name = "T1", Items = new List<ComponentTemplateItem>() },
            new() { Name = "T2", Items = new List<ComponentTemplateItem>() }
        });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }
}
