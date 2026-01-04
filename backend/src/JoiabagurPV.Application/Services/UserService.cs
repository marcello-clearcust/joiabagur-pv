using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Domain.Exceptions;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for user management operations.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<List<UserDto>> GetAllAsync(bool includeInactive = true)
    {
        var users = await _userRepository.GetAllAsync(includeInactive);
        return users.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<UserDto?> GetByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? MapToDto(user) : null;
    }

    /// <inheritdoc/>
    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        // Validate username uniqueness
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            throw new DomainException("El nombre de usuario ya está en uso");
        }

        // Validate email uniqueness if provided
        if (!string.IsNullOrEmpty(request.Email) && await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new DomainException("El email ya está registrado");
        }

        // Parse and validate role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new DomainException("Rol inválido. Use 'Administrator' u 'Operator'");
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = role,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Username} created successfully", user.Username);

        return MapToDto(user);
    }

    /// <inheritdoc/>
    public async Task<UserDto> UpdateAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new DomainException("Usuario no encontrado");
        }

        // Validate email uniqueness if changed and provided
        if (!string.IsNullOrEmpty(request.Email) && 
            request.Email != user.Email &&
            await _userRepository.EmailExistsAsync(request.Email, userId))
        {
            throw new DomainException("El email ya está registrado");
        }

        // Parse and validate role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new DomainException("Rol inválido. Use 'Administrator' u 'Operator'");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Role = role;
        user.IsActive = request.IsActive;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Username} updated successfully", user.Username);

        return MapToDto(user);
    }

    /// <inheritdoc/>
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new DomainException("Usuario no encontrado");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Password changed for user {Username}", user.Username);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
