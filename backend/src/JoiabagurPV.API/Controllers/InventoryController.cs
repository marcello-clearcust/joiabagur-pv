using FluentValidation;
using JoiabagurPV.Application.DTOs.Inventory;
using JoiabagurPV.Application.DTOs.Products;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for inventory management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IStockImportService _stockImportService;
    private readonly IInventoryMovementService _movementService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<AssignProductRequest> _assignValidator;
    private readonly IValidator<BulkAssignProductsRequest> _bulkAssignValidator;
    private readonly IValidator<UnassignProductRequest> _unassignValidator;
    private readonly IValidator<StockAdjustmentRequest> _adjustmentValidator;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IInventoryService inventoryService,
        IStockImportService stockImportService,
        IInventoryMovementService movementService,
        ICurrentUserService currentUserService,
        IValidator<AssignProductRequest> assignValidator,
        IValidator<BulkAssignProductsRequest> bulkAssignValidator,
        IValidator<UnassignProductRequest> unassignValidator,
        IValidator<StockAdjustmentRequest> adjustmentValidator,
        ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _stockImportService = stockImportService;
        _movementService = movementService;
        _currentUserService = currentUserService;
        _assignValidator = assignValidator;
        _bulkAssignValidator = bulkAssignValidator;
        _unassignValidator = unassignValidator;
        _adjustmentValidator = adjustmentValidator;
        _logger = logger;
    }

    #region Assignment Endpoints

    /// <summary>
    /// Assigns a product to a point of sale.
    /// Only administrators can assign products.
    /// </summary>
    /// <param name="request">The assignment request.</param>
    /// <returns>The assignment result.</returns>
    [HttpPost("assign")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(AssignmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignProduct([FromBody] AssignProductRequest request)
    {
        var validationResult = await _assignValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var result = await _inventoryService.AssignProductAsync(request);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    /// <summary>
    /// Assigns multiple products to a point of sale.
    /// Only administrators can assign products.
    /// </summary>
    /// <param name="request">The bulk assignment request.</param>
    /// <returns>The bulk assignment result.</returns>
    [HttpPost("assign/bulk")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BulkAssignmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignProducts([FromBody] BulkAssignProductsRequest request)
    {
        var validationResult = await _bulkAssignValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var result = await _inventoryService.AssignProductsAsync(request);

        if (!result.Success && result.Errors.Any())
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Unassigns a product from a point of sale.
    /// Only administrators can unassign products.
    /// </summary>
    /// <param name="request">The unassignment request.</param>
    /// <returns>The assignment result.</returns>
    [HttpPost("unassign")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(AssignmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UnassignProduct([FromBody] UnassignProductRequest request)
    {
        var validationResult = await _unassignValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var result = await _inventoryService.UnassignProductAsync(request);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets products assigned to a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of assigned inventory records.</returns>
    [HttpGet("assigned")]
    [ProducesResponseType(typeof(PaginatedInventoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAssignedProducts(
        [FromQuery] Guid pointOfSaleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (pointOfSaleId == Guid.Empty)
        {
            return BadRequest(new { error = "El ID del punto de venta es requerido." });
        }

        var result = await _inventoryService.GetAssignedProductsAsync(pointOfSaleId, page, pageSize);
        return Ok(result);
    }

    #endregion

    #region Stock View Endpoints

    /// <summary>
    /// Gets stock for a point of sale.
    /// </summary>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of inventory records.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedInventoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStock(
        [FromQuery] Guid pointOfSaleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (pointOfSaleId == Guid.Empty)
        {
            return BadRequest(new { error = "El ID del punto de venta es requerido." });
        }

        var result = await _inventoryService.GetStockByPointOfSaleAsync(pointOfSaleId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets centralized stock view (aggregated by product).
    /// Only administrators can access this endpoint.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of centralized stock records.</returns>
    [HttpGet("centralized")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(PaginatedCentralizedStockResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCentralizedStock(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _inventoryService.GetCentralizedStockAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets stock breakdown for a product across all points of sale.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>Centralized stock with breakdown.</returns>
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(typeof(CentralizedStockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductStockBreakdown(Guid productId)
    {
        var result = await _inventoryService.GetStockBreakdownAsync(productId);

        if (result == null)
        {
            return NotFound(new { error = "Producto no encontrado o sin stock asignado." });
        }

        return Ok(result);
    }

    #endregion

    #region Stock Import Endpoints

    /// <summary>
    /// Downloads an Excel template for stock import.
    /// Only administrators can access this endpoint.
    /// </summary>
    /// <returns>An Excel file template with headers and example data.</returns>
    [HttpGet("import-template")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult DownloadImportTemplate()
    {
        _logger.LogInformation("Generating stock import template");

        var templateStream = _stockImportService.GenerateTemplate();
        var content = templateStream.ToArray();

        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "stock-import-template.xlsx");
    }

    /// <summary>
    /// Imports stock from an Excel file.
    /// Only administrators can import stock.
    /// </summary>
    /// <param name="file">The Excel file (.xlsx or .xls).</param>
    /// <param name="pointOfSaleId">The target point of sale ID.</param>
    /// <returns>Import result with counts.</returns>
    [HttpPost("import")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(StockImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StockImportResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> ImportStock(IFormFile file, [FromQuery] Guid pointOfSaleId)
    {
        // Validate point of sale ID
        if (pointOfSaleId == Guid.Empty)
        {
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "El ID del punto de venta es requerido.",
                Errors = new List<ImportError>
                {
                    new ImportError { RowNumber = 0, Field = "PointOfSaleId", Message = "El ID del punto de venta es requerido." }
                }
            });
        }

        // Validate file presence
        if (file == null || file.Length == 0)
        {
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "No se proporcionó ningún archivo.",
                Errors = new List<ImportError>
                {
                    new ImportError { RowNumber = 0, Field = "File", Message = "El archivo es requerido." }
                }
            });
        }

        // Validate file size
        if (file.Length > _stockImportService.MaxFileSizeBytes)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new StockImportResult
            {
                Success = false,
                Message = "El archivo excede el tamaño máximo permitido.",
                Errors = new List<ImportError>
                {
                    new ImportError 
                    { 
                        RowNumber = 0, 
                        Field = "File", 
                        Message = $"El tamaño del archivo excede el máximo de {_stockImportService.MaxFileSizeBytes / (1024 * 1024)} MB." 
                    }
                }
            });
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_stockImportService.AllowedExtensions.Contains(extension))
        {
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "Formato de archivo inválido.",
                Errors = new List<ImportError>
                {
                    new ImportError 
                    { 
                        RowNumber = 0, 
                        Field = "File", 
                        Message = $"Tipo de archivo inválido. Tipos permitidos: {string.Join(", ", _stockImportService.AllowedExtensions)}" 
                    }
                }
            });
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(new { error = "Usuario no autenticado." });
        }

        _logger.LogInformation("Starting stock import from file: {FileName} to POS: {PointOfSaleId}", 
            file.FileName, pointOfSaleId);

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _stockImportService.ImportAsync(stream, pointOfSaleId, userId.Value);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Stock import completed: {Updated} updated, {Assigned} assigned",
                    result.StockUpdatedCount, result.AssignmentsCreatedCount);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Stock import failed with {ErrorCount} errors", result.Errors.Count);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during stock import");
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "Ocurrió un error durante la importación.",
                Errors = new List<ImportError>
                {
                    new ImportError { RowNumber = 0, Field = "File", Message = ex.Message }
                }
            });
        }
    }

    /// <summary>
    /// Validates an Excel file for stock import without performing the import.
    /// Only administrators can validate import files.
    /// </summary>
    /// <param name="file">The Excel file (.xlsx or .xls).</param>
    /// <param name="pointOfSaleId">The target point of sale ID.</param>
    /// <returns>Validation result with any errors found.</returns>
    [HttpPost("import/validate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(StockImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(StockImportResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> ValidateImport(IFormFile file, [FromQuery] Guid pointOfSaleId)
    {
        // Validate point of sale ID
        if (pointOfSaleId == Guid.Empty)
        {
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "El ID del punto de venta es requerido.",
                Errors = new List<ImportError>
                {
                    new ImportError { RowNumber = 0, Field = "PointOfSaleId", Message = "El ID del punto de venta es requerido." }
                }
            });
        }

        // Validate file presence
        if (file == null || file.Length == 0)
        {
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "No se proporcionó ningún archivo.",
                Errors = new List<ImportError>
                {
                    new ImportError { RowNumber = 0, Field = "File", Message = "El archivo es requerido." }
                }
            });
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_stockImportService.AllowedExtensions.Contains(extension))
        {
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "Formato de archivo inválido.",
                Errors = new List<ImportError>
                {
                    new ImportError 
                    { 
                        RowNumber = 0, 
                        Field = "File", 
                        Message = $"Tipo de archivo inválido. Tipos permitidos: {string.Join(", ", _stockImportService.AllowedExtensions)}" 
                    }
                }
            });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _stockImportService.ValidateAsync(stream, pointOfSaleId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stock import file");
            return BadRequest(new StockImportResult
            {
                Success = false,
                Message = "Ocurrió un error durante la validación.",
                Errors = new List<ImportError>
                {
                    new ImportError { RowNumber = 0, Field = "File", Message = ex.Message }
                }
            });
        }
    }

    #endregion

    #region Stock Adjustment Endpoints

    /// <summary>
    /// Adjusts stock for a product at a point of sale.
    /// Only administrators can adjust stock.
    /// </summary>
    /// <param name="request">The adjustment request.</param>
    /// <returns>The adjustment result.</returns>
    [HttpPost("adjustment")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(StockAdjustmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentRequest request)
    {
        var validationResult = await _adjustmentValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(new { error = "Usuario no autenticado." });
        }

        var result = await _inventoryService.AdjustStockAsync(request, userId.Value);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    #endregion

    #region Movement History Endpoints

    /// <summary>
    /// Gets movement history with optional filters.
    /// Default date range is last 30 days.
    /// </summary>
    /// <param name="productId">Optional product ID filter.</param>
    /// <param name="pointOfSaleId">Optional point of sale ID filter.</param>
    /// <param name="startDate">Start date filter.</param>
    /// <param name="endDate">End date filter.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated list of movements.</returns>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(PaginatedMovementResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMovementHistory(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? pointOfSaleId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var filter = new MovementHistoryFilter
        {
            ProductId = productId,
            PointOfSaleId = pointOfSaleId,
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _movementService.GetMovementHistoryAsync(filter);
        return Ok(result);
    }

    #endregion
}

