using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for User entity.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower());
    }

    /// <inheritdoc/>
    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.Username.ToLower() == username.ToLower());

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.Email != null && u.Email.ToLower() == email.ToLower());

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc/>
    public async Task<User?> GetWithAssignmentsAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.PointOfSaleAssignments.Where(a => a.IsActive))
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <inheritdoc/>
    public async Task<List<User>> GetAllAsync(bool includeInactive = true)
    {
        var query = _context.Users.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        return await query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();
    }
}
