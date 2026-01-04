using FluentAssertions;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Tests.TestHelpers;
using Xunit;

namespace JoiabagurPV.Tests.UnitTests.Domain;

/// <summary>
/// Unit tests for BaseEntity.
/// </summary>
public class BaseEntityTests : TestBase
{
    [Fact]
    public void BaseEntity_ShouldHaveValidId()
    {
        // Arrange & Act
        var entity = TestDataGenerator.CreateBaseEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void BaseEntity_ShouldHaveCreationTimestamp()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow.AddMilliseconds(-10);
        var entity = new TestEntity();

        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        entity.CreatedAt.Should().BeCloseTo(entity.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void BaseEntity_ShouldInitializeWithCurrentTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddMilliseconds(-10);

        // Act
        var entity = new TestEntity(); // Using a concrete implementation

        // Assert
        entity.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        entity.UpdatedAt.Should().BeOnOrAfter(beforeCreation);
        entity.CreatedAt.Should().BeCloseTo(entity.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }
}

/// <summary>
/// Concrete implementation of BaseEntity for testing.
/// </summary>
internal class TestEntity : BaseEntity
{
}