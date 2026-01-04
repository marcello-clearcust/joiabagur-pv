using JoiabagurPV.Domain.Entities;

namespace JoiabagurPV.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The email to search for.</param>
    /// <returns>The user if found, null otherwise.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks if a username is already in use.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check.</param>
    /// <returns>True if username exists, false otherwise.</returns>
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null);

    /// <summary>
    /// Checks if an email is already in use.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check.</param>
    /// <returns>True if email exists, false otherwise.</returns>
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);

    /// <summary>
    /// Gets a user with their point of sale assignments.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user with assignments, or null if not found.</returns>
    Task<User?> GetWithAssignmentsAsync(Guid userId);

    /// <summary>
    /// Gets all users with optional filtering.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive users.</param>
    /// <returns>List of users.</returns>
    Task<List<User>> GetAllAsync(bool includeInactive = true);
}
