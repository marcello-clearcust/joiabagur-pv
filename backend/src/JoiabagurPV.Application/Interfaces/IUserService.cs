using JoiabagurPV.Application.DTOs.Users;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for user management operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive users.</param>
    /// <returns>List of user DTOs.</returns>
    Task<List<UserDto>> GetAllAsync(bool includeInactive = true);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user DTO if found.</returns>
    Task<UserDto?> GetByIdAsync(Guid userId);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The create user request.</param>
    /// <returns>The created user DTO.</returns>
    Task<UserDto> CreateAsync(CreateUserRequest request);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userId">The user ID to update.</param>
    /// <param name="request">The update user request.</param>
    /// <returns>The updated user DTO.</returns>
    Task<UserDto> UpdateAsync(Guid userId, UpdateUserRequest request);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="request">The change password request.</param>
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
