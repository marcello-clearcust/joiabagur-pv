using JoiabagurPV.Application.DTOs.Components;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for component assignment operations on products.
/// </summary>
public interface IComponentAssignmentService
{
    /// <summary>
    /// Gets all component assignments for a product.
    /// </summary>
    Task<List<ComponentAssignmentDto>> GetByProductIdAsync(Guid productId);

    /// <summary>
    /// Saves (replaces) all component assignments for a product.
    /// </summary>
    Task<List<ComponentAssignmentDto>> SaveAssignmentsAsync(Guid productId, SaveComponentAssignmentsRequest request);

    /// <summary>
    /// Gets a preview of price sync from master for a product.
    /// </summary>
    Task<PriceSyncPreviewDto> GetSyncPreviewAsync(Guid productId);

    /// <summary>
    /// Applies master table prices to all assignments of a product.
    /// </summary>
    Task<List<ComponentAssignmentDto>> ApplySyncFromMasterAsync(Guid productId);

    /// <summary>
    /// Applies a template to a product with merge logic.
    /// </summary>
    Task<ApplyTemplateResultDto> ApplyTemplateAsync(Guid productId, Guid templateId);
}
