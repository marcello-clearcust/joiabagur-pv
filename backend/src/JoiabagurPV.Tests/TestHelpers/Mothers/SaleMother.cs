using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Infrastructure.Data;

namespace JoiabagurPV.Tests.TestHelpers.Mothers;

/// <summary>
/// Mother Object for creating Sale entities in integration tests.
/// Uses Bogus via TestDataGenerator for realistic default values
/// and handles database persistence.
/// </summary>
public class SaleMother
{
    private readonly ApplicationDbContext _context;
    private readonly Sale _sale;
    private bool _includePhoto = false;

    public SaleMother(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        // Start with a Bogus-generated entity with realistic values
        _sale = TestDataGenerator.CreateSale();
    }

    /// <summary>
    /// Sets the product for the sale.
    /// </summary>
    public SaleMother WithProduct(Guid productId)
    {
        _sale.ProductId = productId;
        return this;
    }

    /// <summary>
    /// Sets the point of sale for the sale.
    /// </summary>
    public SaleMother WithPointOfSale(Guid pointOfSaleId)
    {
        _sale.PointOfSaleId = pointOfSaleId;
        return this;
    }

    /// <summary>
    /// Sets the payment method for the sale.
    /// </summary>
    public SaleMother WithPaymentMethod(Guid paymentMethodId)
    {
        _sale.PaymentMethodId = paymentMethodId;
        return this;
    }

    /// <summary>
    /// Sets the user who made the sale.
    /// </summary>
    public SaleMother WithUser(Guid userId)
    {
        _sale.UserId = userId;
        return this;
    }

    /// <summary>
    /// Sets the quantity sold.
    /// </summary>
    public SaleMother WithQuantity(int quantity)
    {
        _sale.Quantity = quantity;
        return this;
    }

    /// <summary>
    /// Sets the price per unit.
    /// </summary>
    public SaleMother WithPrice(decimal price)
    {
        _sale.Price = price;
        return this;
    }

    /// <summary>
    /// Alias for WithPrice for compatibility.
    /// </summary>
    public SaleMother WithUnitPrice(decimal price)
    {
        _sale.Price = price;
        return this;
    }

    /// <summary>
    /// Sets the sale date.
    /// </summary>
    public SaleMother WithSaleDate(DateTime saleDate)
    {
        _sale.SaleDate = saleDate;
        return this;
    }

    /// <summary>
    /// Sets optional notes for the sale.
    /// </summary>
    public SaleMother WithNotes(string notes)
    {
        _sale.Notes = notes;
        return this;
    }

    /// <summary>
    /// Includes a photo for the sale.
    /// </summary>
    public SaleMother WithPhoto()
    {
        _includePhoto = true;
        return this;
    }

    /// <summary>
    /// Creates and persists the sale to the database.
    /// </summary>
    public async Task<Sale> CreateAsync()
    {
        // Validate required foreign keys are set
        if (_sale.ProductId == Guid.Empty)
            throw new InvalidOperationException("ProductId is required. Call WithProduct().");
        if (_sale.PointOfSaleId == Guid.Empty)
            throw new InvalidOperationException("PointOfSaleId is required. Call WithPointOfSale().");
        if (_sale.PaymentMethodId == Guid.Empty)
            throw new InvalidOperationException("PaymentMethodId is required. Call WithPaymentMethod().");
        if (_sale.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required. Call WithUser().");

        _context.Sales.Add(_sale);

        if (_includePhoto)
        {
            var photo = new SalePhoto
            {
                Id = Guid.NewGuid(),
                SaleId = _sale.Id,
                FilePath = $"/uploads/sales/{_sale.Id}.jpg",
                FileName = $"{_sale.Id}.jpg",
                FileSize = 1024,
                MimeType = "image/jpeg",
                CreatedAt = DateTime.UtcNow
            };
            _context.SalePhotos.Add(photo);
        }

        await _context.SaveChangesAsync();
        return _sale;
    }
}
