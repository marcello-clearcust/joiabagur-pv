using JoiabagurPV.Application.DTOs.Components;
using JoiabagurPV.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JoiabagurPV.API.Controllers;

/// <summary>
/// Controller for component template management.
/// All endpoints require Administrator role.
/// </summary>
[ApiController]
[Route("api/component-templates")]
[Authorize(Roles = "Administrator")]
public class ComponentTemplatesController : ControllerBase
{
    private readonly IComponentTemplateService _templateService;
    private readonly ILogger<ComponentTemplatesController> _logger;

    public ComponentTemplatesController(
        IComponentTemplateService templateService,
        ILogger<ComponentTemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all templates with their items.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var templates = await _templateService.GetAllAsync();
        return Ok(templates);
    }

    /// <summary>
    /// Gets a template by ID with items.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null)
            return NotFound();

        return Ok(template);
    }

    /// <summary>
    /// Creates a new template.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveComponentTemplateRequest request)
    {
        var template = await _templateService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
    }

    /// <summary>
    /// Updates an existing template.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveComponentTemplateRequest request)
    {
        var template = await _templateService.UpdateAsync(id, request);
        return Ok(template);
    }

    /// <summary>
    /// Deletes a template.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _templateService.DeleteAsync(id);
        return NoContent();
    }
}
