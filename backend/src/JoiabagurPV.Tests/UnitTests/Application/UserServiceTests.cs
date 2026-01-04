using FluentAssertions;
using JoiabagurPV.Application.DTOs.Users;
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
/// Unit tests for UserService.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _sut = new UserService(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = TestDataGenerator.CreateUsers(3);
        _userRepositoryMock.Setup(x => x.GetAllAsync(true))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(u => u.Username).Should().BeEquivalentTo(users.Select(u => u.Username));
    }

    [Fact]
    public async Task GetAllAsync_WithIncludeInactiveFalse_ShouldFilterInactiveUsers()
    {
        // Arrange
        var activeUsers = TestDataGenerator.CreateUsers(2, isActive: true);
        _userRepositoryMock.Setup(x => x.GetAllAsync(false))
            .ReturnsAsync(activeUsers);

        // Act
        var result = await _sut.GetAllAsync(includeInactive: false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.IsActive);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Role = "Administrator"
        };

        _userRepositoryMock.Setup(x => x.UsernameExistsAsync(request.Username, null))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email, null))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(request.Username);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.Role.Should().Be("Administrator");
        result.IsActive.Should().BeTrue();

        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Username == request.Username &&
            BCrypt.Net.BCrypt.Verify(request.Password, u.PasswordHash))), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateUsername_ShouldThrowDomainException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "existinguser",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "Operator"
        };

        _userRepositoryMock.Setup(x => x.UsernameExistsAsync(request.Username, null))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El nombre de usuario ya está en uso");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ShouldThrowDomainException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            Role = "Operator"
        };

        _userRepositoryMock.Setup(x => x.UsernameExistsAsync(request.Username, null))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email, null))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El email ya está registrado");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRole_ShouldThrowDomainException()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Password = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe",
            Role = "InvalidRole"
        };

        _userRepositoryMock.Setup(x => x.UsernameExistsAsync(request.Username, null))
            .ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Rol inválido. Use 'Administrator' u 'Operator'");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser();
        var request = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            Role = "Operator",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email, user.Id))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateAsync(user.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Name");
        result.Email.Should().Be("updated@example.com");
        result.Role.Should().Be("Operator");

        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentUser_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Role = "Administrator",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.UpdateAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario no encontrado");
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateEmail_ShouldThrowDomainException()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser();
        var request = new UpdateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "existing@example.com",
            Role = "Administrator",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.EmailExistsAsync(request.Email, user.Id))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.UpdateAsync(user.Id, request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El email ya está registrado");
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ShouldChangePassword()
    {
        // Arrange
        var user = TestDataGenerator.CreateUser();
        var request = new ChangePasswordRequest { NewPassword = "NewSecurePass123!" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Act
        await _sut.ChangePasswordAsync(user.Id, request);

        // Assert
        BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash).Should().BeTrue();
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNonExistentUser_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest { NewPassword = "NewSecurePass123!" };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.ChangePasswordAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Usuario no encontrado");
    }

    #endregion
}
