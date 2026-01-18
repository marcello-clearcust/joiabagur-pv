using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Enums;
using JoiabagurPV.Infrastructure.Data;

namespace JoiabagurPV.Tests.TestHelpers.Mothers;

/// <summary>
/// Mother Object for creating Return entities in integration tests.
/// Uses Bogus via TestDataGenerator for realistic default values
/// and handles database persistence.
/// </summary>
public class ReturnMother
{
    private readonly ApplicationDbContext _context;
    private readonly Return _return;
    private readonly List<(Guid SaleId, int Quantity, decimal UnitPrice)> _saleAssociations = new();
    private bool _includePhoto = false;

    public ReturnMother(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        // Start with a Bogus-generated entity with realistic values
        _return = TestDataGenerator.CreateReturn();
    }

    /// <summary>
    /// Sets the product for the return.
    /// </summary>
    public ReturnMother WithProduct(Guid productId)
    {
        _return.ProductId = productId;
        return this;
    }

    /// <summary>
    /// Sets the point of sale for the return.
    /// </summary>
    public ReturnMother WithPointOfSale(Guid pointOfSaleId)
    {
        _return.PointOfSaleId = pointOfSaleId;
        return this;
    }

    /// <summary>
    /// Sets the user who created the return.
    /// </summary>
    public ReturnMother WithUser(Guid userId)
    {
        _return.UserId = userId;
        return this;
    }

    /// <summary>
    /// Sets the quantity being returned.
    /// </summary>
    public ReturnMother WithQuantity(int quantity)
    {
        _return.Quantity = quantity;
        return this;
    }

    /// <summary>
    /// Sets the return category.
    /// </summary>
    public ReturnMother WithCategory(ReturnCategory category)
    {
        _return.Category = category;
        return this;
    }

    /// <summary>
    /// Sets the optional reason for the return.
    /// </summary>
    public ReturnMother WithReason(string reason)
    {
        _return.Reason = reason;
        return this;
    }

    /// <summary>
    /// Sets the return date.
    /// </summary>
    public ReturnMother WithReturnDate(DateTime returnDate)
    {
        _return.ReturnDate = returnDate;
        return this;
    }

    /// <summary>
    /// Adds a sale association to the return.
    /// </summary>
    public ReturnMother WithSaleAssociation(Guid saleId, int quantity, decimal unitPrice)
    {
        _saleAssociations.Add((saleId, quantity, unitPrice));
        return this;
    }

    /// <summary>
    /// Includes a photo for the return.
    /// </summary>
    public ReturnMother WithPhoto()
    {
        _includePhoto = true;
        return this;
    }

    /// <summary>
    /// Creates and persists the return to the database.
    /// </summary>
    public async Task<Return> CreateAsync()
    {
        // Validate required foreign keys are set
        if (_return.ProductId == Guid.Empty)
            throw new InvalidOperationException("ProductId is required. Call WithProduct().");
        if (_return.PointOfSaleId == Guid.Empty)
            throw new InvalidOperationException("PointOfSaleId is required. Call WithPointOfSale().");
        if (_return.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required. Call WithUser().");

        _context.Returns.Add(_return);

        // Add sale associations
        foreach (var (saleId, quantity, unitPrice) in _saleAssociations)
        {
            var returnSale = TestDataGenerator.CreateReturnSale(
                returnId: _return.Id,
                saleId: saleId,
                quantity: quantity,
                unitPrice: unitPrice);
            _context.ReturnSales.Add(returnSale);
        }

        // Add photo if requested
        if (_includePhoto)
        {
            var photo = new ReturnPhoto
            {
                Id = Guid.NewGuid(),
                ReturnId = _return.Id,
                FilePath = $"/uploads/returns/{_return.Id}.jpg",
                FileName = $"{_return.Id}.jpg",
                FileSize = 1024,
                MimeType = "image/jpeg",
                CreatedAt = DateTime.UtcNow
            };
            _context.ReturnPhotos.Add(photo);
        }

        await _context.SaveChangesAsync();
        return _return;
    }
}
