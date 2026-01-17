using JoiabagurPV.Application.DTOs.Sales;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Entities;
using JoiabagurPV.Domain.Interfaces.Repositories;
using JoiabagurPV.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace JoiabagurPV.Application.Services;

/// <summary>
/// Service for sales management operations.
/// </summary>
public class SalesService : ISalesService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISalePhotoRepository _salePhotoRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserPointOfSaleRepository _userPointOfSaleRepository;
    private readonly IStockValidationService _stockValidationService;
    private readonly IPaymentMethodValidationService _paymentMethodValidationService;
    private readonly IInventoryService _inventoryService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public SalesService(
        ISaleRepository saleRepository,
        ISalePhotoRepository salePhotoRepository,
        IProductRepository productRepository,
        IUserPointOfSaleRepository userPointOfSaleRepository,
        IStockValidationService stockValidationService,
        IPaymentMethodValidationService paymentMethodValidationService,
        IInventoryService inventoryService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
        _salePhotoRepository = salePhotoRepository ?? throw new ArgumentNullException(nameof(salePhotoRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _userPointOfSaleRepository = userPointOfSaleRepository ?? throw new ArgumentNullException(nameof(userPointOfSaleRepository));
        _stockValidationService = stockValidationService ?? throw new ArgumentNullException(nameof(stockValidationService));
        _paymentMethodValidationService = paymentMethodValidationService ?? throw new ArgumentNullException(nameof(paymentMethodValidationService));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para><b>Transaction Management:</b></para>
    /// <para>This method implements a double stock validation pattern for concurrency safety:</para>
    /// <list type="number">
    /// <item>First validation: Before transaction (fast fail for obvious issues)</item>
    /// <item>Second validation: Inside transaction (prevents race conditions)</item>
    /// </list>
    /// <para>If stock changes between validations, the error message shows the difference.</para>
    /// <para><b>Atomic Operations:</b></para>
    /// <para>Sale, SalePhoto, and InventoryMovement are created in a single transaction.</para>
    /// <para>If any step fails, all changes are rolled back to maintain data integrity.</para>
    /// <para><b>Price Snapshot:</b></para>
    /// <para>The product price at sale time is frozen in Sale.Price to preserve historical accuracy.</para>
    /// </remarks>
    public async Task<CreateSaleResult> CreateSaleAsync(CreateSaleRequest request, Guid userId, bool isAdmin)
    {
        try
        {
            // Validate quantity
            if (request.Quantity <= 0)
            {
                return new CreateSaleResult
                {
                    Success = false,
                    ErrorMessage = "Quantity must be greater than zero."
                };
            }

            // Validate operator is assigned to point of sale (admins can sell from any POS)
            if (!isAdmin)
            {
                var isAssigned = await _userPointOfSaleRepository
                    .GetAll()
                    .AnyAsync(ups => ups.UserId == userId && 
                                    ups.PointOfSaleId == request.PointOfSaleId &&
                                    ups.IsActive);

                if (!isAssigned)
                {
                    return new CreateSaleResult
                    {
                        Success = false,
                        ErrorMessage = "Operator is not assigned to this point of sale."
                    };
                }
            }

            // Validate product exists and is active
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return new CreateSaleResult
                {
                    Success = false,
                    ErrorMessage = $"Product with ID {request.ProductId} not found."
                };
            }

            if (!product.IsActive)
            {
                return new CreateSaleResult
                {
                    Success = false,
                    ErrorMessage = $"Product '{product.Name}' is not active."
                };
            }

            // FIRST stock validation (before transaction)
            var stockValidation = await _stockValidationService.ValidateStockAvailabilityAsync(
                request.ProductId,
                request.PointOfSaleId,
                request.Quantity);

            if (!stockValidation.IsValid)
            {
                return new CreateSaleResult
                {
                    Success = false,
                    ErrorMessage = stockValidation.ErrorMessage ?? "Insufficient stock available."
                };
            }

            // Validate payment method
            try
            {
                await _paymentMethodValidationService.ValidatePaymentMethodAsync(
                    request.PaymentMethodId,
                    request.PointOfSaleId);
            }
            catch (InvalidOperationException ex)
            {
                return new CreateSaleResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }

            // Begin transaction
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // SECOND stock validation (double check for concurrency safety)
                var finalStockValidation = await _stockValidationService.ValidateStockAvailabilityAsync(
                    request.ProductId,
                    request.PointOfSaleId,
                    request.Quantity);

                if (!finalStockValidation.IsValid)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new CreateSaleResult
                    {
                        Success = false,
                        ErrorMessage = $"Stock changed. Available: {finalStockValidation.AvailableQuantity}, Requested: {request.Quantity}"
                    };
                }

                // Create Sale record
                var sale = new Sale
                {
                    ProductId = request.ProductId,
                    PointOfSaleId = request.PointOfSaleId,
                    UserId = userId,
                    PaymentMethodId = request.PaymentMethodId,
                    Price = product.Price, // Price snapshot
                    Quantity = request.Quantity,
                    Notes = request.Notes,
                    SaleDate = DateTime.UtcNow
                };

                sale = await _saleRepository.AddAsync(sale);
                await _unitOfWork.SaveChangesAsync();

                // Handle photo if provided
                // Note: Photo compression will be handled by ImageCompressionService in the controller
                // Here we just save the already-compressed photo
                if (!string.IsNullOrEmpty(request.PhotoBase64))
                {
                    try
                    {
                        var photoBytes = Convert.FromBase64String(request.PhotoBase64);
                        var fileName = $"sale_{sale.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
                        
                        // Save photo to storage
                        using var photoStream = new MemoryStream(photoBytes);
                        var storedFileName = await _fileStorageService.UploadAsync(
                            photoStream, 
                            fileName, 
                            "image/jpeg", 
                            $"sales/{sale.Id}");

                        // Create SalePhoto record
                        var salePhoto = new SalePhoto
                        {
                            SaleId = sale.Id,
                            FilePath = storedFileName,
                            FileName = request.PhotoFileName ?? fileName,
                            FileSize = photoBytes.Length,
                            MimeType = "image/jpeg"
                        };

                        await _salePhotoRepository.AddAsync(salePhoto);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Photo save failed, but don't fail the entire sale
                        // Log the error but continue with the transaction
                        Console.WriteLine($"Failed to save sale photo: {ex.Message}");
                    }
                }

                // Create inventory movement (automatic stock update)
                var movementResult = await _inventoryService.CreateSaleMovementAsync(
                    request.ProductId,
                    request.PointOfSaleId,
                    sale.Id,
                    request.Quantity,
                    userId);

                if (!movementResult.Success)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new CreateSaleResult
                    {
                        Success = false,
                        ErrorMessage = movementResult.ErrorMessage ?? "Failed to create inventory movement."
                    };
                }

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync();

                // Load sale with details for response
                var saleWithDetails = await _saleRepository.GetByIdWithDetailsAsync(sale.Id);

                return new CreateSaleResult
                {
                    Success = true,
                    Sale = MapToDto(saleWithDetails!),
                    WarningMessage = stockValidation.IsLowStock ? stockValidation.WarningMessage : null,
                    IsLowStock = stockValidation.IsLowStock,
                    RemainingStock = movementResult.QuantityAfter
                };
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new CreateSaleResult
            {
                Success = false,
                ErrorMessage = $"An error occurred while creating the sale: {ex.Message}"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SaleDto?> GetSaleByIdAsync(Guid id, Guid userId, bool isAdmin)
    {
        var sale = await _saleRepository.GetByIdWithDetailsAsync(id);
        if (sale == null)
        {
            return null;
        }

        // Check authorization
        if (!isAdmin)
        {
            // Check if operator is assigned to this point of sale
            var isAssigned = await _userPointOfSaleRepository
                .GetAll()
                .AnyAsync(ups => ups.UserId == userId && 
                                ups.PointOfSaleId == sale.PointOfSaleId &&
                                ups.IsActive);

            if (!isAssigned)
            {
                return null; // Not authorized
            }
        }

        return MapToDto(sale);
    }

    /// <inheritdoc/>
    public async Task<SalesHistoryResponse> GetSalesHistoryAsync(
        SalesHistoryFilterRequest request, 
        Guid userId, 
        bool isAdmin)
    {
        List<Sale> sales;
        int totalCount;

        var skip = (request.Page - 1) * request.PageSize;
        var take = Math.Min(request.PageSize, 50);

        if (isAdmin)
        {
            // Admins can see all sales
            (sales, totalCount) = await _saleRepository.GetAllSalesAsync(
                request.StartDate,
                request.EndDate,
                request.PointOfSaleId,
                request.ProductId,
                request.UserId,
                request.PaymentMethodId,
                skip,
                take);
        }
        else
        {
            // Operators can only see sales from their assigned points of sale
            var assignedPosIds = await _userPointOfSaleRepository
                .GetAll()
                .Where(ups => ups.UserId == userId && ups.IsActive)
                .Select(ups => ups.PointOfSaleId)
                .ToListAsync();

            if (!assignedPosIds.Any())
            {
                // No assigned points of sale
                return new SalesHistoryResponse
                {
                    Sales = new List<SaleDto>(),
                    TotalCount = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };
            }

            (sales, totalCount) = await _saleRepository.GetByPointOfSalesAsync(
                assignedPosIds,
                request.StartDate,
                request.EndDate,
                request.ProductId,
                request.UserId,
                request.PaymentMethodId,
                skip,
                take);
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new SalesHistoryResponse
        {
            Sales = sales.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }

    /// <inheritdoc/>
    public async Task<string?> GetSalePhotoPathAsync(Guid saleId)
    {
        var photo = await _salePhotoRepository.GetBySaleIdAsync(saleId);
        return photo?.FilePath;
    }

    /// <inheritdoc/>
    public async Task<(Stream Stream, string ContentType, string FileName)?> GetSalePhotoStreamAsync(Guid saleId)
    {
        var photo = await _salePhotoRepository.GetBySaleIdAsync(saleId);
        if (photo == null || string.IsNullOrEmpty(photo.FilePath))
        {
            return null;
        }

        // Extract folder from the file path
        var folder = $"sales/{saleId}";
        var result = await _fileStorageService.DownloadAsync(photo.FilePath, folder);
        
        if (result == null)
        {
            return null;
        }

        return (result.Value.Stream, result.Value.ContentType, photo.FileName);
    }

    /// <summary>
    /// Maps a Sale entity to a SaleDto.
    /// </summary>
    private static SaleDto MapToDto(Sale sale)
    {
        return new SaleDto
        {
            Id = sale.Id,
            ProductId = sale.ProductId,
            ProductSku = sale.Product?.SKU ?? string.Empty,
            ProductName = sale.Product?.Name ?? string.Empty,
            PointOfSaleId = sale.PointOfSaleId,
            PointOfSaleName = sale.PointOfSale?.Name ?? string.Empty,
            UserId = sale.UserId,
            UserName = sale.User?.Username ?? string.Empty,
            PaymentMethodId = sale.PaymentMethodId,
            PaymentMethodName = sale.PaymentMethod?.Name ?? string.Empty,
            Price = sale.Price,
            Quantity = sale.Quantity,
            Total = sale.GetTotal(),
            Notes = sale.Notes,
            HasPhoto = sale.Photo != null,
            SaleDate = sale.SaleDate,
            CreatedAt = sale.CreatedAt
        };
    }
}
