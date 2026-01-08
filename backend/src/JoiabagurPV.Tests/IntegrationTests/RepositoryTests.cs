using FluentAssertions;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JoiabagurPV.Tests.IntegrationTests;

/// <summary>
/// Integration tests for repository functionality.
/// </summary>
[Collection(RepositoryTestCollection.Name)]
public class RepositoryTests
{
    private readonly TestDatabaseFixture _fixture;

    public RepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Repository_ShouldAddAndRetrieveEntity()
    {
        // Arrange
        using var scope = _fixture.ScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var entity = TestDataGenerator.CreateBaseEntity() as TestEntity ?? new TestEntity();

        // Act
        var addedEntity = await repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();

        var retrievedEntity = await repository.GetByIdAsync(addedEntity.Id);

        // Assert
        retrievedEntity.Should().NotBeNull();
        retrievedEntity!.Id.Should().Be(addedEntity.Id);
        retrievedEntity.CreatedAt.Should().Be(addedEntity.CreatedAt);
    }

    [Fact]
    public async Task Repository_ShouldUpdateEntity()
    {
        // Arrange
        using var scope = _fixture.ScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var entity = TestDataGenerator.CreateBaseEntity() as TestEntity ?? new TestEntity();

        var addedEntity = await repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();

        // Act
        var updatedEntity = await repository.UpdateAsync(addedEntity);
        await unitOfWork.SaveChangesAsync();

        var retrievedEntity = await repository.GetByIdAsync(addedEntity.Id);

        // Assert
        retrievedEntity.Should().NotBeNull();
        retrievedEntity!.UpdatedAt.Should().BeAfter(retrievedEntity.CreatedAt);
    }

    [Fact]
    public async Task Repository_ShouldDeleteEntity()
    {
        // Arrange
        using var scope = _fixture.ScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var entity = TestDataGenerator.CreateBaseEntity() as TestEntity ?? new TestEntity();

        var addedEntity = await repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();

        // Act
        var deleted = await repository.DeleteAsync(addedEntity.Id);
        await unitOfWork.SaveChangesAsync();

        var retrievedEntity = await repository.GetByIdAsync(addedEntity.Id);

        // Assert
        deleted.Should().BeTrue();
        retrievedEntity.Should().BeNull();
    }

    [Fact]
    public async Task Repository_ShouldCheckEntityExistence()
    {
        // Arrange
        using var scope = _fixture.ScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var entity = TestDataGenerator.CreateBaseEntity() as TestEntity ?? new TestEntity();

        var addedEntity = await repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync();

        // Act & Assert
        (await repository.ExistsAsync(addedEntity.Id)).Should().BeTrue();
        (await repository.ExistsAsync(Guid.NewGuid())).Should().BeFalse();
    }
}

/// <summary>
/// Concrete implementation of BaseEntity for testing.
/// </summary>
internal class TestEntity : BaseEntity
{
}