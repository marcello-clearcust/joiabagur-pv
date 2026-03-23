using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ProductPhotoEmbedding entity operations.
/// </summary>
public class ProductPhotoEmbeddingRepository : Repository<ProductPhotoEmbedding>, IProductPhotoEmbeddingRepository
{
    private readonly ApplicationDbContext _context;

    public ProductPhotoEmbeddingRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<ProductPhotoEmbedding>> GetAllAsync()
    {
        return await _context.ProductPhotoEmbeddings.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ProductPhotoEmbedding?> GetByPhotoIdAsync(Guid photoId)
    {
        return await _context.ProductPhotoEmbeddings
            .FirstOrDefaultAsync(e => e.ProductPhotoId == photoId);
    }

    /// <inheritdoc/>
    public async Task<List<ProductPhotoEmbedding>> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductPhotoEmbeddings
            .Where(e => e.ProductId == productId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task DeleteByPhotoIdAsync(Guid photoId)
    {
        var embedding = await _context.ProductPhotoEmbeddings
            .FirstOrDefaultAsync(e => e.ProductPhotoId == photoId);

        if (embedding != null)
        {
            _context.ProductPhotoEmbeddings.Remove(embedding);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAllAsync()
    {
        await _context.ProductPhotoEmbeddings.ExecuteDeleteAsync();
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync()
    {
        return await _context.ProductPhotoEmbeddings.CountAsync();
    }

    /// <inheritdoc/>
    public async Task<DateTime?> GetLastUpdatedAsync()
    {
        return await _context.ProductPhotoEmbeddings
            .MaxAsync(e => (DateTime?)e.UpdatedAt);
    }
}
