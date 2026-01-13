using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ModelMetadata entity operations.
/// </summary>
public class ModelMetadataRepository : Repository<ModelMetadata>, IModelMetadataRepository
{
    private readonly ApplicationDbContext _context;

    public ModelMetadataRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<ModelMetadata?> GetActiveModelAsync()
    {
        return await _context.ModelMetadata
            .FirstOrDefaultAsync(m => m.IsActive);
    }

    /// <inheritdoc/>
    public async Task<ModelMetadata?> GetByVersionAsync(string version)
    {
        return await _context.ModelMetadata
            .FirstOrDefaultAsync(m => m.Version == version);
    }

    /// <inheritdoc/>
    public async Task<List<ModelMetadata>> GetAllVersionsAsync()
    {
        return await _context.ModelMetadata
            .OrderByDescending(m => m.TrainedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task DeactivateCurrentModelAsync()
    {
        var activeModel = await GetActiveModelAsync();
        if (activeModel != null)
        {
            activeModel.IsActive = false;
            await UpdateAsync(activeModel);
        }
    }

    /// <inheritdoc/>
    public async Task ActivateModelAsync(Guid modelId)
    {
        // Deactivate current model
        await DeactivateCurrentModelAsync();

        // Activate the specified model
        var model = await GetByIdAsync(modelId);
        if (model != null)
        {
            model.IsActive = true;
            await UpdateAsync(model);
        }
    }
}
