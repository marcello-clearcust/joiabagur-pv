using JoiabagurPV.Application.DTOs.Returns;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for returns management operations.
/// </summary>
public interface IReturnService
{
    /// <summary>
    /// Creates a new return with transaction-based inventory updates.
    /// Validates eligible sales, quantities, and operator access before creating the return.
    /// </summary>
    /// <param name="request">The return creation request.</param>
    /// <param name="userId">The ID of the user creating the return.</param>
    /// <param name="isAdmin">Whether the user is an administrator (admins can create returns from any POS).</param>
    /// <returns>The result of the return creation.</returns>
    Task<CreateReturnResult> CreateReturnAsync(CreateReturnRequest request, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets a return by ID with full details.
    /// Admins can view all returns, operators can only view returns from their assigned points of sale.
    /// </summary>
    /// <param name="id">The return ID.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="isAdmin">Whether the requesting user is an admin.</param>
    /// <returns>The return DTO if found and authorized, null otherwise.</returns>
    Task<ReturnDto?> GetReturnByIdAsync(Guid id, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets returns history with filtering and pagination.
    /// Admins see all returns, operators see only returns from their assigned points of sale.
    /// </summary>
    /// <param name="request">The filter and pagination request.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="isAdmin">Whether the requesting user is an admin.</param>
    /// <returns>Paginated returns history.</returns>
    Task<ReturnsHistoryResponse> GetReturnsHistoryAsync(ReturnsHistoryFilterRequest request, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets sales eligible for return based on product and point of sale.
    /// Returns only sales within the 30-day return window that have remaining returnable quantity.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="isAdmin">Whether the requesting user is an admin.</param>
    /// <returns>List of eligible sales with available quantities.</returns>
    Task<EligibleSalesResponse?> GetEligibleSalesAsync(Guid productId, Guid pointOfSaleId, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets the photo file path for a return.
    /// </summary>
    /// <param name="returnId">The return ID.</param>
    /// <returns>The file path if photo exists, null otherwise.</returns>
    Task<string?> GetReturnPhotoPathAsync(Guid returnId);

    /// <summary>
    /// Gets the photo file stream for a return.
    /// </summary>
    /// <param name="returnId">The return ID.</param>
    /// <returns>The file stream and content type if photo exists, null otherwise.</returns>
    Task<(Stream Stream, string ContentType, string FileName)?> GetReturnPhotoStreamAsync(Guid returnId);
}
