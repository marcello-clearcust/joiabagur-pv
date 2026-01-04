using FluentValidation;
using JoiabagurPV.Application.DTOs.Users;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for user management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPointOfSaleService _userPointOfSaleService;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly IValidator<ChangePasswordRequest> _passwordValidator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IUserPointOfSaleService userPointOfSaleService,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator,
        IValidator<ChangePasswordRequest> passwordValidator,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _userPointOfSaleService = userPointOfSaleService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _passwordValidator = passwordValidator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive users. Defaults to true when null.</param>
    /// <returns>List of users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] bool? includeInactive = null)
    {
        var users = await _userService.GetAllAsync(includeInactive ?? true);
        return Ok(users);
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user details.</returns>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound(new { error = "Usuario no encontrado" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <returns>The created user.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var user = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { userId = user.Id }, user);
        }
        catch (DomainException ex) when (ex.Message.Contains("en uso") || ex.Message.Contains("registrado"))
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
    /// Updates an existing user.
    /// </summary>
    /// <param name="userId">The user ID to update.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var user = await _userService.UpdateAsync(userId, request);
            return Ok(user);
        }
        catch (DomainException ex) when (ex.Message.Contains("no encontrado"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainException ex) when (ex.Message.Contains("registrado"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="request">The password change request.</param>
    /// <returns>204 No Content on success.</returns>
    [HttpPut("{userId:guid}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request)
    {
        var validationResult = await _passwordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            await _userService.ChangePasswordAsync(userId, request);
            return NoContent();
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
    /// Gets all point of sale assignments for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>List of assignments.</returns>
    [HttpGet("{userId:guid}/point-of-sales")]
    [ProducesResponseType(typeof(List<UserPointOfSaleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserPointOfSales(Guid userId)
    {
        var assignments = await _userPointOfSaleService.GetUserAssignmentsAsync(userId);
        return Ok(assignments);
    }

    /// <summary>
    /// Assigns a user to a point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>The assignment details.</returns>
    [HttpPost("{userId:guid}/point-of-sales/{pointOfSaleId:guid}")]
    [ProducesResponseType(typeof(UserPointOfSaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignPointOfSale(Guid userId, Guid pointOfSaleId)
    {
        try
        {
            var assignment = await _userPointOfSaleService.AssignAsync(userId, pointOfSaleId);
            return StatusCode(StatusCodes.Status201Created, assignment);
        }
        catch (DomainException ex) when (ex.Message.Contains("ya está asignado"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Unassigns a user from a point of sale.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="pointOfSaleId">The point of sale ID.</param>
    /// <returns>204 No Content on success.</returns>
    [HttpDelete("{userId:guid}/point-of-sales/{pointOfSaleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UnassignPointOfSale(Guid userId, Guid pointOfSaleId)
    {
        try
        {
            await _userPointOfSaleService.UnassignAsync(userId, pointOfSaleId);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
