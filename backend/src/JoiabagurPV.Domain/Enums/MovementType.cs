namespace JoiabagurPV.Domain.Enums;

/// <summary>
/// Types of inventory movements.
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Stock decrease due to a sale.
    /// </summary>
    Sale = 1,

    /// <summary>
    /// Stock increase due to a return.
    /// </summary>
    Return = 2,

    /// <summary>
    /// Manual stock adjustment (increase or decrease).
    /// </summary>
    Adjustment = 3,

    /// <summary>
    /// Stock addition via Excel import.
    /// </summary>
    Import = 4
}

