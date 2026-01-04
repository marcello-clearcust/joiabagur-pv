using FluentAssertions;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

/// <summary>
/// Unit tests for UserPointOfSaleService.
/// </summary>
public class UserPointOfSaleServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserPointOfSaleRepository> _userPointOfSaleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UserPointOfSaleService>> _loggerMock;
    private readonly UserPointOfSaleService _sut;

    public UserPointOfSaleServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userPointOfSaleRepositoryMock = new Mock<IUserPointOfSaleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UserPointOfSaleService>>();

        _sut = new UserPointOfSaleService(
            _userRepositoryMock.Object,
            _userPointOfSaleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region GetUserAssignmentsAsync Tests

    [Fact]
    public async Task GetUserAssignmentsAsync_ShouldReturnAssignments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var assignments = new List<UserPointOfSale>
        {
            TestDataGenerator.CreateUserPointOfSale(userId, Guid.NewGuid(), true),
            TestDataGenerator.CreateUserPointOfSale(userId, Guid.NewGuid(), true)
        };

        _userPointOfSaleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, false))
            .ReturnsAsync(assignments);

        // Act
        var result = await _sut.GetUserAssignmentsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.IsActive);
    }

    [Fact]
    public async Task GetUserAssignmentsAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userPointOfSaleRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, false))
            .ReturnsAsync(new List<UserPointOfSale>());

        // Act
        var result = await _sut.GetUserAssignmentsAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AssignAsync Tests

    [Fact]
    public async Task AssignAsync_WithValidOperator_ShouldCreateAssignment()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Operator);
        var pointOfSaleId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _userPointOfSaleRepositoryMock.Setup(x => x.GetAssignmentAsync(user.Id, pointOfSaleId))
            .ReturnsAsync((UserPointOfSale?)null);

        // Act
        var result = await _sut.AssignAsync(user.Id, pointOfSaleId);

        // Assert
        result.Should().NotBeNull();
        result.PointOfSaleId.Should().Be(pointOfSaleId);
        result.IsActive.Should().BeTrue();

        _userPointOfSaleRepositoryMock.Verify(x => x.AddAsync(It.Is<UserPointOfSale>(a =>
            a.UserId == user.Id &&
            a.PointOfSaleId == pointOfSaleId &&
            a.IsActive)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_WithAdminUser_ShouldThrowDomainException()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Administrator);
        var pointOfSaleId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.AssignAsync(user.Id, pointOfSaleId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Los administradores tienen acceso a todos los puntos de venta y no requieren asignación");
    }

    [Fact]
    public async Task AssignAsync_WithNonExistentUser_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pointOfSaleId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.AssignAsync(userId, pointOfSaleId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario no encontrado");
    }

    [Fact]
    public async Task AssignAsync_WithExistingActiveAssignment_ShouldThrowDomainException()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Operator);
        var pointOfSaleId = Guid.NewGuid();
        var existingAssignment = TestDataGenerator.CreateUserPointOfSale(user.Id, pointOfSaleId, true);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _userPointOfSaleRepositoryMock.Setup(x => x.GetAssignmentAsync(user.Id, pointOfSaleId))
            .ReturnsAsync(existingAssignment);

        // Act
        var act = () => _sut.AssignAsync(user.Id, pointOfSaleId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El operador ya está asignado a este punto de venta");
    }

    [Fact]
    public async Task AssignAsync_WithInactiveExistingAssignment_ShouldReactivate()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser(UserRole.Operator);
        var pointOfSaleId = Guid.NewGuid();
        var existingAssignment = TestDataGenerator.CreateUserPointOfSale(user.Id, pointOfSaleId, false);
        existingAssignment.UnassignedAt = DateTime.UtcNow.AddDays(-1);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _userPointOfSaleRepositoryMock.Setup(x => x.GetAssignmentAsync(user.Id, pointOfSaleId))
            .ReturnsAsync(existingAssignment);

        // Act
        var result = await _sut.AssignAsync(user.Id, pointOfSaleId);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        existingAssignment.IsActive.Should().BeTrue();
        existingAssignment.UnassignedAt.Should().BeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region UnassignAsync Tests

    [Fact]
    public async Task UnassignAsync_WithValidAssignment_ShouldDeactivate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pointOfSaleId = Guid.NewGuid();
        var assignment = TestDataGenerator.CreateUserPointOfSale(userId, pointOfSaleId, true);

        _userPointOfSaleRepositoryMock.Setup(x => x.GetActiveAssignmentAsync(userId, pointOfSaleId))
            .ReturnsAsync(assignment);
        _userPointOfSaleRepositoryMock.Setup(x => x.CountActiveAssignmentsAsync(userId))
            .ReturnsAsync(2); // User has more than one assignment

        // Act
        await _sut.UnassignAsync(userId, pointOfSaleId);

        // Assert
        assignment.IsActive.Should().BeFalse();
        assignment.UnassignedAt.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UnassignAsync_WithNoActiveAssignment_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pointOfSaleId = Guid.NewGuid();

        _userPointOfSaleRepositoryMock.Setup(x => x.GetActiveAssignmentAsync(userId, pointOfSaleId))
            .ReturnsAsync((UserPointOfSale?)null);

        // Act
        var act = () => _sut.UnassignAsync(userId, pointOfSaleId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El operador ya está desasignado de este punto de venta");
    }

    [Fact]
    public async Task UnassignAsync_WithLastActiveAssignment_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pointOfSaleId = Guid.NewGuid();
        var assignment = TestDataGenerator.CreateUserPointOfSale(userId, pointOfSaleId, true);

        _userPointOfSaleRepositoryMock.Setup(x => x.GetActiveAssignmentAsync(userId, pointOfSaleId))
            .ReturnsAsync(assignment);
        _userPointOfSaleRepositoryMock.Setup(x => x.CountActiveAssignmentsAsync(userId))
            .ReturnsAsync(1); // Only one active assignment

        // Act
        var act = () => _sut.UnassignAsync(userId, pointOfSaleId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Un operador debe tener al menos un punto de venta asignado");
    }

    #endregion

    #region HasAccessAsync Tests

    [Fact]
    public async Task HasAccessAsync_WithActiveAssignment_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pointOfSaleId = Guid.NewGuid();

        _userPointOfSaleRepositoryMock.Setup(x => x.HasAccessAsync(userId, pointOfSaleId))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.HasAccessAsync(userId, pointOfSaleId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAccessAsync_WithNoAssignment_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pointOfSaleId = Guid.NewGuid();

        _userPointOfSaleRepositoryMock.Setup(x => x.HasAccessAsync(userId, pointOfSaleId))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.HasAccessAsync(userId, pointOfSaleId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAssignedPointOfSaleIdsAsync Tests

    [Fact]
    public async Task GetAssignedPointOfSaleIdsAsync_ShouldReturnIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        _userPointOfSaleRepositoryMock.Setup(x => x.GetAssignedPointOfSaleIdsAsync(userId))
            .ReturnsAsync(expectedIds);

        // Act
        var result = await _sut.GetAssignedPointOfSaleIdsAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedIds);
    }

    #endregion
}
