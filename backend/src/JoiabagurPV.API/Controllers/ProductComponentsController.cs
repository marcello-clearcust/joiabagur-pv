using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Interfaces;
using JoiabagurPV.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for product component master table and assignment operations.
/// All endpoints require Administrator role.
/// </summary>
[ApiController]
[Route("api/product-components")]
[Authorize(Roles = "Administrator")]
public class ProductComponentsController : ControllerBase
{
    private readonly IProductComponentService _componentService;
    private readonly IComponentAssignmentService _assignmentService;
    private readonly ILogger<ProductComponentsController> _logger;

    public ProductComponentsController(
        IProductComponentService componentService,
        IComponentAssignmentService assignmentService,
        ILogger<ProductComponentsController> logger)
    {
        _componentService = componentService;
        _assignmentService = assignmentService;
        _logger = logger;
    }

    // ─── Master Table CRUD ─────────────────────────────────────────────

    /// <summary>
    /// Gets a paginated list of components with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetComponents([FromQuery] ComponentQueryParameters parameters)
    {
        var result = await _componentService.GetComponentsAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Gets a component by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComponent(Guid id)
    {
        var component = await _componentService.GetByIdAsync(id);
        if (component == null)
            return NotFound();

        return Ok(component);
    }

    /// <summary>
    /// Creates a new component.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateComponent([FromBody] CreateComponentRequest request)
    {
        var component = await _componentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetComponent), new { id = component.Id }, component);
    }

    /// <summary>
    /// Updates an existing component.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateComponent(Guid id, [FromBody] UpdateComponentRequest request)
    {
        var component = await _componentService.UpdateAsync(id, request);
        return Ok(component);
    }

    /// <summary>
    /// Searches active components by description for autocomplete.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchComponents([FromQuery] string query)
    {
        var results = await _componentService.SearchAsync(query);
        return Ok(results);
    }

    // ─── Component Assignments on Products ─────────────────────────────

    /// <summary>
    /// Gets all component assignments for a product.
    /// </summary>
    [HttpGet("~/api/products/{productId:guid}/components")]
    public async Task<IActionResult> GetProductComponents(Guid productId)
    {
        var assignments = await _assignmentService.GetByProductIdAsync(productId);
        return Ok(assignments);
    }

    /// <summary>
    /// Saves (replaces) all component assignments for a product.
    /// </summary>
    [HttpPut("~/api/products/{productId:guid}/components")]
    public async Task<IActionResult> SaveProductComponents(Guid productId, [FromBody] SaveComponentAssignmentsRequest request)
    {
        var assignments = await _assignmentService.SaveAssignmentsAsync(productId, request);
        return Ok(assignments);
    }

    /// <summary>
    /// Gets a preview of price sync from master for a product.
    /// </summary>
    [HttpGet("~/api/products/{productId:guid}/components/sync-preview")]
    public async Task<IActionResult> GetSyncPreview(Guid productId)
    {
        var preview = await _assignmentService.GetSyncPreviewAsync(productId);
        return Ok(preview);
    }

    /// <summary>
    /// Applies master table prices to all assignments of a product.
    /// </summary>
    [HttpPost("~/api/products/{productId:guid}/components/sync-from-master")]
    public async Task<IActionResult> SyncFromMaster(Guid productId)
    {
        var assignments = await _assignmentService.ApplySyncFromMasterAsync(productId);
        return Ok(assignments);
    }

    /// <summary>
    /// Applies a template to a product with merge logic.
    /// </summary>
    [HttpPost("~/api/products/{productId:guid}/components/apply-template")]
    public async Task<IActionResult> ApplyTemplate(Guid productId, [FromBody] ApplyTemplateRequest request)
    {
        var result = await _assignmentService.ApplyTemplateAsync(productId, request.TemplateId);
        return Ok(result);
    }
}
