using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Creates an ActionResult from a service result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <returns>An ActionResult.</returns>
    protected ActionResult<T> OkOrNotFound<T>(T? result)
    {
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Creates a CreatedAtAction result for newly created resources.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="result">The created resource.</param>
    /// <returns>A CreatedAtActionResult.</returns>
    protected ActionResult<T> Created<T>(T result)
    {
        var id = GetEntityId(result);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    /// <summary>
    /// Gets the ID of an entity (abstract method to be implemented by derived controllers).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity ID.</returns>
    protected abstract object GetEntityId<T>(T entity);

    /// <summary>
    /// Placeholder method for GetById action - should be implemented in derived controllers.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>An IActionResult.</returns>
    [HttpGet("{id}")]
    public virtual IActionResult GetById(Guid id)
    {
        return NotImplemented();
    }

    /// <summary>
    /// Returns a 501 Not Implemented result.
    /// </summary>
    /// <returns>A NotImplementedResult.</returns>
    protected IActionResult NotImplemented()
    {
        return StatusCode(501, new { message = "Not implemented" });
    }
}