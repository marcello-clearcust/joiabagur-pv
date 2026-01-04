namespace JoiabagurPV.Application.DTOs.Auth;

/// <summary>
/// Response DTO for current user information.
/// </summary>
public class CurrentUserResponse
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user's username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The user's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// The user's email (if provided).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// The user's role.
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// List of assigned point of sales (for operators).
    /// </summary>
    public List<AssignedPointOfSaleDto> AssignedPointOfSales { get; set; } = new();
}

/// <summary>
/// DTO for assigned point of sale information.
/// </summary>
public class AssignedPointOfSaleDto
{
    /// <summary>
    /// The point of sale ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The point of sale name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The point of sale code.
    /// </summary>
    public required string Code { get; set; }
}
