using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ProductPhoto entity operations.
/// </summary>
public class ProductPhotoRepository : Repository<ProductPhoto>, IProductPhotoRepository
{
    private readonly ApplicationDbContext _context;

    public ProductPhotoRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<ProductPhoto>> GetByProductIdAsync(Guid productId)
    {
        return await _context.ProductPhotos
            .Where(pp => pp.ProductId == productId)
            .OrderBy(pp => pp.DisplayOrder)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<ProductPhoto?> GetPrimaryPhotoAsync(Guid productId)
    {
        return await _context.ProductPhotos
            .FirstOrDefaultAsync(pp => pp.ProductId == productId && pp.IsPrimary);
    }

    /// <inheritdoc/>
    public async Task UpdateDisplayOrderAsync(Guid productId, Dictionary<Guid, int> photoOrders)
    {
        var photos = await _context.ProductPhotos
            .Where(pp => pp.ProductId == productId && photoOrders.Keys.Contains(pp.Id))
            .ToListAsync();

        foreach (var photo in photos)
        {
            if (photoOrders.TryGetValue(photo.Id, out var newOrder))
            {
                photo.DisplayOrder = newOrder;
            }
        }
    }

    /// <inheritdoc/>
    public async Task SetPrimaryPhotoAsync(Guid photoId)
    {
        var photo = await _context.ProductPhotos
            .FirstOrDefaultAsync(pp => pp.Id == photoId);

        if (photo == null)
        {
            return;
        }

        // Unmark any existing primary photo for this product
        var existingPrimary = await _context.ProductPhotos
            .Where(pp => pp.ProductId == photo.ProductId && pp.IsPrimary && pp.Id != photoId)
            .ToListAsync();

        foreach (var existing in existingPrimary)
        {
            existing.IsPrimary = false;
        }

        // Mark the new photo as primary
        photo.IsPrimary = true;
    }

    /// <inheritdoc/>
    public async Task<int> GetNextDisplayOrderAsync(Guid productId)
    {
        var maxOrder = await _context.ProductPhotos
            .Where(pp => pp.ProductId == productId)
            .MaxAsync(pp => (int?)pp.DisplayOrder);

        return (maxOrder ?? -1) + 1;
    }
}




