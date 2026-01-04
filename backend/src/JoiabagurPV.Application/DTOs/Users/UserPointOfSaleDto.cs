namespace JoiabagurPV.Application.DTOs.Users;

/// <summary>
/// DTO for user point of sale assignment.
/// </summary>
public class UserPointOfSaleDto
{
    /// <summary>
    /// The assignment ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The point of sale ID.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// The point of sale name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The point of sale code.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// The assigned user information (when getting by point of sale).
    /// </summary>
    public UserMinimalDto? User { get; set; }

    /// <summary>
    /// When the user was assigned.
    /// </summary>
    public DateTime AssignedAt { get; set; }

    /// <summary>
    /// Whether the assignment is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the user was unassigned (if applicable).
    /// </summary>
    public DateTime? UnassignedAt { get; set; }
}

/// <summary>
/// Minimal user information DTO for nested objects.
/// </summary>
public class UserMinimalDto
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public required string Role { get; set; }
}
