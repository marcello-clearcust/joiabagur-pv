using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for ReturnSale entity operations.
/// </summary>
public class ReturnSaleRepository : Repository<ReturnSale>, IReturnSaleRepository
{
    private readonly ApplicationDbContext _context;

    public ReturnSaleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<int> GetReturnedQuantityForSaleAsync(Guid saleId)
    {
        return await _context.ReturnSales
            .Where(rs => rs.SaleId == saleId)
            .SumAsync(rs => rs.Quantity);
    }

    /// <inheritdoc/>
    public async Task<List<ReturnSale>> GetBySaleIdAsync(Guid saleId)
    {
        return await _context.ReturnSales
            .Include(rs => rs.Return)
            .Where(rs => rs.SaleId == saleId)
            .OrderByDescending(rs => rs.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ReturnSale>> GetByReturnIdWithSaleDetailsAsync(Guid returnId)
    {
        return await _context.ReturnSales
            .Include(rs => rs.Sale)
                .ThenInclude(s => s.PaymentMethod)
            .Where(rs => rs.ReturnId == returnId)
            .OrderBy(rs => rs.Sale.SaleDate)
            .ToListAsync();
    }
}
