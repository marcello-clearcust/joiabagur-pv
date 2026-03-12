using JoiabagurPV.Application.DTOs.Sales;

namespace JoiabagurPV.Application.Interfaces;

/// <summary>
/// Service interface for sales management operations.
/// </summary>
public interface ISalesService
{
    /// <summary>
    /// Creates a new sale with transaction-based inventory updates.
    /// Validates stock, payment method, and operator access before creating the sale.
    /// </summary>
    /// <param name="request">The sale creation request.</param>
    /// <param name="userId">The ID of the user creating the sale.</param>
    /// <param name="isAdmin">Whether the user is an administrator (admins can sell from any POS).</param>
    /// <returns>The result of the sale creation.</returns>
    Task<CreateSaleResult> CreateSaleAsync(CreateSaleRequest request, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets a sale by ID with full details.
    /// Admins can view all sales, operators can only view sales from their assigned points of sale.
    /// </summary>
    /// <param name="id">The sale ID.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="isAdmin">Whether the requesting user is an admin.</param>
    /// <returns>The sale DTO if found and authorized, null otherwise.</returns>
    Task<SaleDto?> GetSaleByIdAsync(Guid id, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets sales history with filtering and pagination.
    /// Admins see all sales, operators see only sales from their assigned points of sale.
    /// </summary>
    /// <param name="request">The filter and pagination request.</param>
    /// <param name="userId">The ID of the requesting user.</param>
    /// <param name="isAdmin">Whether the requesting user is an admin.</param>
    /// <returns>Paginated sales history.</returns>
    Task<SalesHistoryResponse> GetSalesHistoryAsync(SalesHistoryFilterRequest request, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets the photo file path for a sale.
    /// </summary>
    /// <param name="saleId">The sale ID.</param>
    /// <returns>The file path if photo exists, null otherwise.</returns>
    Task<string?> GetSalePhotoPathAsync(Guid saleId);

    /// <summary>
    /// Gets the photo file stream for a sale.
    /// </summary>
    /// <param name="saleId">The sale ID.</param>
    /// <returns>The file stream and content type if photo exists, null otherwise.</returns>
    Task<(Stream Stream, string ContentType, string FileName)?> GetSalePhotoStreamAsync(Guid saleId);

    /// <summary>
    /// Creates multiple sales atomically in a single transaction.
    /// All lines must use the same point of sale and payment method.
    /// </summary>
    Task<CreateBulkSalesResult> CreateBulkSalesAsync(CreateBulkSalesRequest request, Guid userId, bool isAdmin);

    /// <summary>
    /// Gets a paginated sales report with extended filters and global aggregates.
    /// Admins see all POS, operators see only assigned POS.
    /// </summary>
    Task<SalesReportResponse> GetSalesReportAsync(SalesReportFilterRequest request, Guid userId, bool isAdmin);

    /// <summary>
    /// Exports the sales report as an Excel file (MemoryStream).
    /// Throws InvalidOperationException if the filtered result exceeds 10,000 rows.
    /// </summary>
    Task<(MemoryStream Stream, int TotalCount)> ExportSalesReportAsync(SalesReportFilterRequest request, Guid userId, bool isAdmin);
}
