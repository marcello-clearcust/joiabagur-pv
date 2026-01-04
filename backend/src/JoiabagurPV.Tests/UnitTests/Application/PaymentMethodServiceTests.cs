using FluentAssertions;
using JoiabagurPV.Application.DTOs.PaymentMethods;
using JoiabagurPV.Application.Services;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace JoiabagurPV.Tests.UnitTests.Application;

public class PaymentMethodServiceTests
{
    private readonly Mock<IPaymentMethodRepository> _paymentMethodRepositoryMock;
    private readonly Mock<IPointOfSalePaymentMethodRepository> _posPaymentMethodRepositoryMock;
    private readonly Mock<IPointOfSaleRepository> _pointOfSaleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<PaymentMethodService>> _loggerMock;
    private readonly PaymentMethodService _service;

    public PaymentMethodServiceTests()
    {
        _paymentMethodRepositoryMock = new Mock<IPaymentMethodRepository>();
        _posPaymentMethodRepositoryMock = new Mock<IPointOfSalePaymentMethodRepository>();
        _pointOfSaleRepositoryMock = new Mock<IPointOfSaleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<PaymentMethodService>>();

        _service = new PaymentMethodService(
            _paymentMethodRepositoryMock.Object,
            _posPaymentMethodRepositoryMock.Object,
            _pointOfSaleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPaymentMethods()
    {
        // Arrange
        var paymentMethods = new List<PaymentMethod>
        {
            new() { Id = Guid.NewGuid(), Code = "CASH", Name = "Efectivo", IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "BIZUM", Name = "Bizum", IsActive = true }
        };
        _paymentMethodRepositoryMock.Setup(r => r.GetAllAsync(true)).ReturnsAsync(paymentMethods);

        // Act
        var result = await _service.GetAllAsync(true);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(pm => pm.Code == "CASH");
        result.Should().Contain(pm => pm.Code == "BIZUM");
    }

    [Fact]
    public async Task GetAllAsync_WhenIncludeInactiveFalse_ShouldReturnOnlyActive()
    {
        // Arrange
        var activePaymentMethods = new List<PaymentMethod>
        {
            new() { Id = Guid.NewGuid(), Code = "CASH", Name = "Efectivo", IsActive = true }
        };
        _paymentMethodRepositoryMock.Setup(r => r.GetAllAsync(false)).ReturnsAsync(activePaymentMethods);

        // Act
        var result = await _service.GetAllAsync(false);

        // Assert
        result.Should().HaveCount(1);
        result.First().Code.Should().Be("CASH");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnPaymentMethod()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentMethod = new PaymentMethod { Id = id, Code = "CASH", Name = "Efectivo", IsActive = true };
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(paymentMethod);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("CASH");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((PaymentMethod?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreatePaymentMethod()
    {
        // Arrange
        var request = new CreatePaymentMethodRequest
        {
            Code = "NEWMETHOD",
            Name = "New Method",
            Description = "Description"
        };
        _paymentMethodRepositoryMock.Setup(r => r.CodeExistsAsync(request.Code, null)).ReturnsAsync(false);
        _paymentMethodRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PaymentMethod>()))
            .ReturnsAsync((PaymentMethod pm) => pm);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("NEWMETHOD");
        result.Name.Should().Be("New Method");
        result.IsActive.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCode_ShouldThrowDomainException()
    {
        // Arrange
        var request = new CreatePaymentMethodRequest
        {
            Code = "CASH",
            Name = "Duplicate Cash"
        };
        _paymentMethodRepositoryMock.Setup(r => r.CodeExistsAsync(request.Code, null)).ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*en uso*");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdatePaymentMethod()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingMethod = new PaymentMethod { Id = id, Code = "CASH", Name = "Old Name", IsActive = true };
        var request = new UpdatePaymentMethodRequest
        {
            Name = "New Name",
            Description = "Updated description"
        };
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingMethod);
        _paymentMethodRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<PaymentMethod>()))
            .ReturnsAsync((PaymentMethod pm) => pm);

        // Act
        var result = await _service.UpdateAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdatePaymentMethodRequest { Name = "Name" };
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((PaymentMethod?)null);

        // Act
        var act = () => _service.UpdateAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*no encontrado*");
    }

    #endregion

    #region ChangeStatusAsync Tests

    [Fact]
    public async Task ChangeStatusAsync_ShouldUpdateStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var paymentMethod = new PaymentMethod { Id = id, Code = "CASH", Name = "Cash", IsActive = true };
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(paymentMethod);
        _paymentMethodRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<PaymentMethod>()))
            .ReturnsAsync((PaymentMethod pm) => pm);

        // Act
        var result = await _service.ChangeStatusAsync(id, false);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region AssignToPointOfSaleAsync Tests

    [Fact]
    public async Task AssignToPointOfSaleAsync_WithValidData_ShouldCreateAssignment()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var paymentMethod = new PaymentMethod { Id = paymentMethodId, Code = "CASH", Name = "Cash", IsActive = true };
        
        _pointOfSaleRepositoryMock.Setup(r => r.ExistsAsync(pointOfSaleId)).ReturnsAsync(true);
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(paymentMethodId)).ReturnsAsync(paymentMethod);
        _posPaymentMethodRepositoryMock.Setup(r => r.GetAssignmentAsync(pointOfSaleId, paymentMethodId))
            .ReturnsAsync((PointOfSalePaymentMethod?)null);
        _posPaymentMethodRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PointOfSalePaymentMethod>()))
            .ReturnsAsync((PointOfSalePaymentMethod pm) => pm);

        // Act
        var result = await _service.AssignToPointOfSaleAsync(pointOfSaleId, paymentMethodId);

        // Assert
        result.Should().NotBeNull();
        result.PointOfSaleId.Should().Be(pointOfSaleId);
        result.PaymentMethodId.Should().Be(paymentMethodId);
        result.IsActive.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignToPointOfSaleAsync_WhenPointOfSaleNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        _pointOfSaleRepositoryMock.Setup(r => r.ExistsAsync(pointOfSaleId)).ReturnsAsync(false);

        // Act
        var act = () => _service.AssignToPointOfSaleAsync(pointOfSaleId, paymentMethodId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Punto de venta no encontrado*");
    }

    [Fact]
    public async Task AssignToPointOfSaleAsync_WhenPaymentMethodNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        _pointOfSaleRepositoryMock.Setup(r => r.ExistsAsync(pointOfSaleId)).ReturnsAsync(true);
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(paymentMethodId)).ReturnsAsync((PaymentMethod?)null);

        // Act
        var act = () => _service.AssignToPointOfSaleAsync(pointOfSaleId, paymentMethodId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Método de pago no encontrado*");
    }

    [Fact]
    public async Task AssignToPointOfSaleAsync_WhenAlreadyAssigned_ShouldThrowDomainException()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var paymentMethod = new PaymentMethod { Id = paymentMethodId, Code = "CASH", Name = "Cash", IsActive = true };
        var existingAssignment = new PointOfSalePaymentMethod
        {
            PointOfSaleId = pointOfSaleId,
            PaymentMethodId = paymentMethodId,
            IsActive = true,
            PaymentMethod = paymentMethod
        };

        _pointOfSaleRepositoryMock.Setup(r => r.ExistsAsync(pointOfSaleId)).ReturnsAsync(true);
        _paymentMethodRepositoryMock.Setup(r => r.GetByIdAsync(paymentMethodId)).ReturnsAsync(paymentMethod);
        _posPaymentMethodRepositoryMock.Setup(r => r.GetAssignmentAsync(pointOfSaleId, paymentMethodId))
            .ReturnsAsync(existingAssignment);

        // Act
        var act = () => _service.AssignToPointOfSaleAsync(pointOfSaleId, paymentMethodId);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*ya está asignado*");
    }

    #endregion

    #region IsPaymentMethodAvailableAsync Tests

    [Fact]
    public async Task IsPaymentMethodAvailableAsync_WhenActiveAndAssigned_ShouldReturnTrue()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        _posPaymentMethodRepositoryMock.Setup(r => r.IsActiveForPointOfSaleAsync(pointOfSaleId, paymentMethodId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IsPaymentMethodAvailableAsync(pointOfSaleId, paymentMethodId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPaymentMethodAvailableAsync_WhenNotAssigned_ShouldReturnFalse()
    {
        // Arrange
        var pointOfSaleId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        _posPaymentMethodRepositoryMock.Setup(r => r.IsActiveForPointOfSaleAsync(pointOfSaleId, paymentMethodId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IsPaymentMethodAvailableAsync(pointOfSaleId, paymentMethodId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
