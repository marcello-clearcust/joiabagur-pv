namespace JoiabagurPV.Domain.Enums;

/// <summary>
/// Defines the roles available in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator with full system access.
    /// </summary>
    Administrator = 1,

    /// <summary>
    /// Operator with access restricted to assigned points of sale.
    /// </summary>
    Operator = 2
}
