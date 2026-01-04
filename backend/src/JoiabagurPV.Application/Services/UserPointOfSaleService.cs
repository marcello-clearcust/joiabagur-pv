using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for user point of sale assignment operations.
/// </summary>
public class UserPointOfSaleService : IUserPointOfSaleService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPointOfSaleRepository _userPointOfSaleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserPointOfSaleService> _logger;

    public UserPointOfSaleService(
        IUserRepository userRepository,
        IUserPointOfSaleRepository userPointOfSaleRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserPointOfSaleService> logger)
    {
        _userRepository = userRepository;
        _userPointOfSaleRepository = userPointOfSaleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<UserPointOfSaleDto>> GetUserAssignmentsAsync(Guid userId)
    {
        var assignments = await _userPointOfSaleRepository.GetByUserIdAsync(userId);
        
        // Note: In a full implementation, we'd join with PointOfSale to get name/code
        return assignments.Select(a => new UserPointOfSaleDto
        {
            Id = a.Id,
            UserId = a.UserId,
            PointOfSaleId = a.PointOfSaleId,
            Name = null, // Will be populated when PointOfSale entity exists
            Code = null,
            AssignedAt = a.AssignedAt,
            IsActive = a.IsActive,
            UnassignedAt = a.UnassignedAt,
            User = null
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<UserPointOfSaleDto>> GetByPointOfSaleAsync(Guid pointOfSaleId)
    {
        var assignments = await _userPointOfSaleRepository.GetByPointOfSaleAsync(pointOfSaleId);
        
        return assignments.Select(a => new UserPointOfSaleDto
        {
            Id = a.Id,
            UserId = a.UserId,
            PointOfSaleId = a.PointOfSaleId,
            Name = null,
            Code = null,
            AssignedAt = a.AssignedAt,
            IsActive = a.IsActive,
            UnassignedAt = a.UnassignedAt,
            User = a.User != null ? new UserMinimalDto
            {
                Id = a.User.Id,
                Username = a.User.Username,
                FirstName = a.User.FirstName,
                LastName = a.User.LastName,
                Email = a.User.Email,
                Role = a.User.Role.ToString()
            } : null
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<UserPointOfSaleDto> AssignAsync(Guid userId, Guid pointOfSaleId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new DomainException("Usuario no encontrado");
        }

        if (user.Role == UserRole.Administrator)
        {
            throw new DomainException("Los administradores tienen acceso a todos los puntos de venta y no requieren asignación");
        }

        // Check for existing assignment
        var existingAssignment = await _userPointOfSaleRepository.GetAssignmentAsync(userId, pointOfSaleId);

        if (existingAssignment != null)
        {
            if (existingAssignment.IsActive)
            {
                throw new DomainException("El operador ya está asignado a este punto de venta");
            }

            // Reactivate existing assignment
            existingAssignment.IsActive = true;
            existingAssignment.AssignedAt = DateTime.UtcNow;
            existingAssignment.UnassignedAt = null;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} re-assigned to point of sale {PointOfSaleId}", userId, pointOfSaleId);

            return new UserPointOfSaleDto
            {
                PointOfSaleId = pointOfSaleId,
                AssignedAt = existingAssignment.AssignedAt,
                IsActive = true
            };
        }

        // Create new assignment
        var assignment = new UserPointOfSale
        {
            UserId = userId,
            PointOfSaleId = pointOfSaleId,
            IsActive = true,
            AssignedAt = DateTime.UtcNow
        };

        await _userPointOfSaleRepository.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserId} assigned to point of sale {PointOfSaleId}", userId, pointOfSaleId);

        return new UserPointOfSaleDto
        {
            PointOfSaleId = pointOfSaleId,
            AssignedAt = assignment.AssignedAt,
            IsActive = true
        };
    }

    /// <inheritdoc/>
    public async Task UnassignAsync(Guid userId, Guid pointOfSaleId)
    {
        var assignment = await _userPointOfSaleRepository.GetActiveAssignmentAsync(userId, pointOfSaleId);
        
        if (assignment == null)
        {
            throw new DomainException("El operador ya está desasignado de este punto de venta");
        }

        // Check if this is the last active assignment
        var activeCount = await _userPointOfSaleRepository.CountActiveAssignmentsAsync(userId);
        if (activeCount <= 1)
        {
            throw new DomainException("Un operador debe tener al menos un punto de venta asignado");
        }

        assignment.IsActive = false;
        assignment.UnassignedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserId} unassigned from point of sale {PointOfSaleId}", userId, pointOfSaleId);
    }

    /// <inheritdoc/>
    public async Task<bool> HasAccessAsync(Guid userId, Guid pointOfSaleId)
    {
        return await _userPointOfSaleRepository.HasAccessAsync(userId, pointOfSaleId);
    }

    /// <inheritdoc/>
    public async Task<List<Guid>> GetAssignedPointOfSaleIdsAsync(Guid userId)
    {
        return await _userPointOfSaleRepository.GetAssignedPointOfSaleIdsAsync(userId);
    }
}
