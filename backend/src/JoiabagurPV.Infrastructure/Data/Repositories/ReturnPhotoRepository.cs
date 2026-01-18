using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ReturnPhoto entity operations.
/// </summary>
public class ReturnPhotoRepository : Repository<ReturnPhoto>, IReturnPhotoRepository
{
    private readonly ApplicationDbContext _context;

    public ReturnPhotoRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<ReturnPhoto?> GetByReturnIdAsync(Guid returnId)
    {
        return await _context.ReturnPhotos
            .FirstOrDefaultAsync(p => p.ReturnId == returnId);
    }
}
