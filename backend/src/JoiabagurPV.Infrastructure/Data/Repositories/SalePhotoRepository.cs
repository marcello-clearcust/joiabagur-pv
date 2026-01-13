using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for SalePhoto entity operations.
/// </summary>
public class SalePhotoRepository : Repository<SalePhoto>, ISalePhotoRepository
{
    private readonly ApplicationDbContext _context;

    public SalePhotoRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<SalePhoto?> GetBySaleIdAsync(Guid saleId)
    {
        return await _context.SalePhotos
            .FirstOrDefaultAsync(p => p.SaleId == saleId);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsBySaleIdAsync(Guid saleId)
    {
        return await _context.SalePhotos
            .AnyAsync(p => p.SaleId == saleId);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteBySaleIdAsync(Guid saleId)
    {
        var photo = await GetBySaleIdAsync(saleId);
        if (photo == null)
            return false;

        _context.SalePhotos.Remove(photo);
        return true;
    }
}
