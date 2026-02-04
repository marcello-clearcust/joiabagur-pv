using FluentValidation;
using JoiabagurPV.Application.DTOs.Returns;
using JoiabagurPV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for returns management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IReturnService _returnService;
    private readonly IImageCompressionService _imageCompressionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateReturnRequest> _createValidator;
    private readonly ILogger<ReturnsController> _logger;

    public ReturnsController(
        IReturnService returnService,
        IImageCompressionService imageCompressionService,
        ICurrentUserService currentUserService,
        IValidator<CreateReturnRequest> createValidator,
        ILogger<ReturnsController> logger)
    {
        _returnService = returnService;
        _imageCompressionService = imageCompressionService;
        _currentUserService = currentUserService;
        _createValidator = createValidator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new return with optional photo attachment.
    /// Validates eligible sales, quantities, and operator access.
    /// </summary>
    /// <param name="request">Return creation request.</param>
    /// <returns>Created return with updated stock quantity.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateReturnResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateReturnResult>> CreateReturn([FromBody] CreateReturnRequest request)
    {
        // Validate request
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        // Get current user
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized(new { message = "Usuario no autenticado." });
        }

        var userId = _currentUserService.UserId.Value;

        // Compress photo if provided
        if (!string.IsNullOrEmpty(request.PhotoBase64))
        {
            try
            {
                var photoBytes = Convert.FromBase64String(request.PhotoBase64);

                // Validate image
                var (isValid, errorMessage) = await _imageCompressionService.ValidateImageAsync(photoBytes);
                if (!isValid)
                {
                    return BadRequest(new { message = errorMessage });
                }

                // Compress image
                var compressedBytes = await _imageCompressionService.CompressImageAsync(photoBytes);
                request.PhotoBase64 = Convert.ToBase64String(compressedBytes);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process return photo");
                return BadRequest(new { message = "Error al procesar la foto. Por favor intente de nuevo." });
            }
        }

        // Create return (admins can create from any POS)
        var result = await _returnService.CreateReturnAsync(request, userId, _currentUserService.IsAdmin);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        var response = new
        {
            returnData = result.Return,
            newStockQuantity = result.NewStockQuantity
        };

        return CreatedAtAction(nameof(GetReturnById), new { id = result.Return!.Id }, response);
    }

    /// <summary>
    /// Gets a return by ID.
    /// Admins can view all returns, operators can only view returns from their assigned points of sale.
    /// </summary>
    /// <param name="id">Return ID.</param>
    /// <returns>Return details if found and authorized.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReturnDto>> GetReturnById(Guid id)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var returnDto = await _returnService.GetReturnByIdAsync(
            id,
            _currentUserService.UserId.Value,
            _currentUserService.IsAdmin);

        if (returnDto == null)
        {
            return NotFound(new { message = "Devolución no encontrada o acceso denegado." });
        }

        return Ok(returnDto);
    }

    /// <summary>
    /// Gets returns history with filtering and pagination.
    /// Admins see all returns, operators see only returns from their assigned points of sale.
    /// </summary>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="pointOfSaleId">Optional point of sale filter.</param>
    /// <param name="productId">Optional product filter.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size (max 50).</param>
    /// <returns>Paginated returns history.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnsHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReturnsHistoryResponse>> GetReturnsHistory(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? pointOfSaleId,
        [FromQuery] Guid? productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        // Convert dates to UTC to avoid PostgreSQL "Kind=Unspecified" error
        DateTime? startDateUtc = startDate.HasValue
            ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
            : null;
        DateTime? endDateUtc = endDate.HasValue
            ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : null;

        var request = new ReturnsHistoryFilterRequest
        {
            StartDate = startDateUtc,
            EndDate = endDateUtc,
            PointOfSaleId = pointOfSaleId,
            ProductId = productId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _returnService.GetReturnsHistoryAsync(
            request,
            _currentUserService.UserId.Value,
            _currentUserService.IsAdmin);

        return Ok(result);
    }

    /// <summary>
    /// Gets sales eligible for return based on product and point of sale.
    /// Returns only sales within the 30-day return window that have remaining returnable quantity.
    /// </summary>
    /// <param name="productId">Product ID.</param>
    /// <param name="pointOfSaleId">Point of sale ID.</param>
    /// <returns>List of eligible sales with available quantities.</returns>
    [HttpGet("eligible-sales")]
    [ProducesResponseType(typeof(EligibleSalesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EligibleSalesResponse>> GetEligibleSales(
        [FromQuery] Guid productId,
        [FromQuery] Guid pointOfSaleId)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        if (productId == Guid.Empty)
        {
            return BadRequest(new { message = "El ID del producto es requerido." });
        }

        if (pointOfSaleId == Guid.Empty)
        {
            return BadRequest(new { message = "El ID del punto de venta es requerido." });
        }

        var result = await _returnService.GetEligibleSalesAsync(
            productId,
            pointOfSaleId,
            _currentUserService.UserId.Value,
            _currentUserService.IsAdmin);

        if (result == null)
        {
            return Forbid(); // Not authorized for this POS
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets the photo file for a return.
    /// </summary>
    /// <param name="id">Return ID.</param>
    /// <returns>Photo file if exists.</returns>
    [HttpGet("{id}/photo/file")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReturnPhoto(Guid id)
    {
        // Verify user has access to this return
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var returnDto = await _returnService.GetReturnByIdAsync(
            id,
            _currentUserService.UserId.Value,
            _currentUserService.IsAdmin);

        if (returnDto == null)
        {
            return NotFound(new { message = "Devolución no encontrada o acceso denegado." });
        }

        if (!returnDto.HasPhoto)
        {
            return NotFound(new { message = "Esta devolución no tiene foto adjunta." });
        }

        var photoResult = await _returnService.GetReturnPhotoStreamAsync(id);
        if (photoResult == null)
        {
            _logger.LogWarning("Photo file not found for return {ReturnId}", id);
            return NotFound(new { message = "Archivo de foto no encontrado." });
        }

        // Set caching headers
        Response.Headers.Append("Cache-Control", "private, max-age=86400"); // Cache for 24 hours
        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{photoResult.Value.FileName}\"");

        return File(photoResult.Value.Stream, photoResult.Value.ContentType);
    }
}
