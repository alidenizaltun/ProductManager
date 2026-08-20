using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.SystemManagement;
using ProductManagement.Shared.Infrastructure.Security;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/integrations")]
public sealed class IntegrationsController : ControllerBase
{
    private readonly IIntegrationService _service;

    public IntegrationsController(IIntegrationService service)
    {
        _service = service;
    }

    [RequirePermission(Permissions.Integrations.View)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<IntegrationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IntegrationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetIntegrationsAsync(cancellationToken);
        return Ok(items);
    }

    [RequirePermission(Permissions.Integrations.View)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IntegrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntegrationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetIntegrationByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [RequirePermission(Permissions.Integrations.Manage)]
    [HttpPost]
    [ProducesResponseType(typeof(IntegrationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<IntegrationDto>> Create(
        [FromBody] CreateIntegrationRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateIntegrationAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [RequirePermission(Permissions.Integrations.Manage)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateIntegrationRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateIntegrationAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [RequirePermission(Permissions.Integrations.Manage)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteIntegrationAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [RequirePermission(Permissions.Integrations.Manage)]
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(typeof(IntegrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntegrationDto>> Test(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.TestIntegrationAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
