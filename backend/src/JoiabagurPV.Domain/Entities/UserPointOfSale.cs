namespace JoiabagurPV.Domain.Entities;

/// <summary>
/// Represents an assignment between an operator user and a point of sale.
/// Uses soft-delete pattern to preserve assignment history.
/// </summary>
public class UserPointOfSale : BaseEntity
{
    /// <summary>
    /// The assigned user's ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for the assigned user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The assigned point of sale's ID.
    /// </summary>
    public Guid PointOfSaleId { get; set; }

    /// <summary>
    /// Navigation property for the assigned point of sale.
    /// </summary>
    public PointOfSale PointOfSale { get; set; } = null!;

    /// <summary>
    /// Whether this assignment is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the user was assigned to the point of sale.
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user was unassigned from the point of sale.
    /// Null if assignment is still active.
    /// </summary>
    public DateTime? UnassignedAt { get; set; }
}
