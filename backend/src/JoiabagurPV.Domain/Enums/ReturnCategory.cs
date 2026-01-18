namespace JoiabagurPV.Domain.Enums;

/// <summary>
/// Categories for product returns.
/// Used for analytics and pattern detection.
/// </summary>
public enum ReturnCategory
{
    /// <summary>
    /// Product is defective.
    /// </summary>
    Defectuoso = 1,

    /// <summary>
    /// Wrong size for customer.
    /// </summary>
    TamañoIncorrecto = 2,

    /// <summary>
    /// Customer not satisfied with product.
    /// </summary>
    NoSatisfecho = 3,

    /// <summary>
    /// Other reason (requires reason text).
    /// </summary>
    Otro = 4
}
