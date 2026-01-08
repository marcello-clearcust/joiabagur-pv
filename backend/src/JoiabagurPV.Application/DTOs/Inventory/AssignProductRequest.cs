namespace JoiabagurPV.Application.DTOs.Inventory;

/// <summary>
/// Request to assign a product to a point of sale.
/// </summary>
public class AssignProductRequest
{
    /// <summary>
    /// The product ID to assign.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The point of sale ID to assign to.
    /// </summary>
    public Guid PointOfSaleId { get; set; }
}

/// <summary>
/// Request to assign multiple products to a point of sale.
/// </summary>
public class BulkAssignProductsRequest
{
    /// <summary>
    /// The product IDs to assign.
    /// </summary>
    public List<Guid> ProductIds { get; set; } = new();

    /// <summary>
    /// The point of sale ID to assign to.
    /// </summary>
    public Guid PointOfSaleId { get; set; }
}

/// <summary>
/// Request to unassign a product from a point of sale.
/// </summary>
public class UnassignProductRequest
{
    /// <summary>
    /// The product ID to unassign.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The point of sale ID to unassign from.
    /// </summary>
    public Guid PointOfSaleId { get; set; }
}

/// <summary>
/// Result of an assignment operation.
/// </summary>
public class AssignmentResult
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The created or updated inventory record.
    /// </summary>
    public InventoryDto? Inventory { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether this was a reactivation of a previously unassigned product.
    /// </summary>
    public bool WasReactivated { get; set; }
}

/// <summary>
/// Result of a bulk assignment operation.
/// </summary>
public class BulkAssignmentResult
{
    /// <summary>
    /// Whether all assignments were successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of products successfully assigned.
    /// </summary>
    public int AssignedCount { get; set; }

    /// <summary>
    /// Number of products that were reactivated.
    /// </summary>
    public int ReactivatedCount { get; set; }

    /// <summary>
    /// Number of products that failed to assign.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Number of products already assigned (skipped).
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// The created or updated inventory records.
    /// </summary>
    public List<InventoryDto> Inventories { get; set; } = new();

    /// <summary>
    /// Error details for failed assignments.
    /// </summary>
    public List<AssignmentError> Errors { get; set; } = new();

    /// <summary>
    /// Summary message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Error details for a failed assignment.
/// </summary>
public class AssignmentError
{
    /// <summary>
    /// The product ID that failed.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

