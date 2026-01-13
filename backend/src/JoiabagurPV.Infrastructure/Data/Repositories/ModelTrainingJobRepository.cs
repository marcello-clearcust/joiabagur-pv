using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ModelTrainingJob entity operations.
/// </summary>
public class ModelTrainingJobRepository : Repository<ModelTrainingJob>, IModelTrainingJobRepository
{
    private readonly ApplicationDbContext _context;

    public ModelTrainingJobRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<ModelTrainingJob?> GetLatestJobAsync()
    {
        return await _context.ModelTrainingJobs
            .Include(j => j.InitiatedByUser)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ModelTrainingJob>> GetRecentJobsAsync(int limit = 10)
    {
        return await _context.ModelTrainingJobs
            .Include(j => j.InitiatedByUser)
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsJobInProgressAsync()
    {
        return await _context.ModelTrainingJobs
            .AnyAsync(j => j.Status == "InProgress" || j.Status == "Queued");
    }

    /// <inheritdoc/>
    public async Task<ModelTrainingJob?> GetInProgressJobAsync()
    {
        return await _context.ModelTrainingJobs
            .Include(j => j.InitiatedByUser)
            .Where(j => j.Status == "InProgress" || j.Status == "Queued")
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
