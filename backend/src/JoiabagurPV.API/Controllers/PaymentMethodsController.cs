using FluentValidation;
using JoiabagurPV.Application.DTOs.PaymentMethods;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for payment method management operations.
/// </summary>
[ApiController]
[Route("api/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IValidator<CreatePaymentMethodRequest> _createValidator;
    private readonly IValidator<UpdatePaymentMethodRequest> _updateValidator;
    private readonly ILogger<PaymentMethodsController> _logger;

    public PaymentMethodsController(
        IPaymentMethodService paymentMethodService,
        IValidator<CreatePaymentMethodRequest> createValidator,
        IValidator<UpdatePaymentMethodRequest> updateValidator,
        ILogger<PaymentMethodsController> logger)
    {
        _paymentMethodService = paymentMethodService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all payment methods. Administrator only.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive payment methods. Defaults to false.</param>
    /// <returns>List of payment methods.</returns>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(List<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var paymentMethods = await _paymentMethodService.GetAllAsync(includeInactive);
        return Ok(paymentMethods);
    }

    /// <summary>
    /// Gets a payment method by ID. Administrator only.
    /// </summary>
    /// <param name="id">The payment method ID.</param>
    /// <returns>The payment method details.</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var paymentMethod = await _paymentMethodService.GetByIdAsync(id);

        if (paymentMethod == null)
        {
            return NotFound(new { error = "Método de pago no encontrado" });
        }

        return Ok(paymentMethod);
    }

    /// <summary>
    /// Creates a new payment method. Administrator only.
    /// </summary>
    /// <param name="request">The payment method creation request.</param>
    /// <returns>The created payment method.</returns>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var paymentMethod = await _paymentMethodService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = paymentMethod.Id }, paymentMethod);
        }
        catch (DomainException ex) when (ex.Message.Contains("en uso"))
        {
            return Conflict(new
            {
                error = new
                {
                    message = ex.Message,
                    type = ex.GetType().Name,
                    statusCode = 409
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new
            {
                error = new
                {
                    message = ex.Message,
                    type = ex.GetType().Name,
                    statusCode = 400
                },
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Updates an existing payment method. Administrator only.
    /// </summary>
    /// <param name="id">The payment method ID to update.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated payment method.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentMethodRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var paymentMethod = await _paymentMethodService.UpdateAsync(id, request);
            return Ok(paymentMethod);
        }
        catch (DomainException ex) when (ex.Message.Contains("no encontrado"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Changes the status (activate/deactivate) of a payment method. Administrator only.
    /// </summary>
    /// <param name="id">The payment method ID.</param>
    /// <param name="request">The status change request.</param>
    /// <returns>The updated payment method.</returns>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] PaymentMethodStatusRequest request)
    {
        try
        {
            var paymentMethod = await _paymentMethodService.ChangeStatusAsync(id, request.IsActive);
            return Ok(paymentMethod);
        }
        catch (DomainException ex) when (ex.Message.Contains("no encontrado"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request DTO for changing payment method status.
/// </summary>
public class PaymentMethodStatusRequest
{
    /// <summary>
    /// The new active status.
    /// </summary>
    public bool IsActive { get; set; }
}
