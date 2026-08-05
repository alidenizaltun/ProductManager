using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.Identity;
using ProductManager.Shared.Infrastructure.Security;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleManagementService _service;

    public RolesController(IRoleManagementService service)
    {
        _service = service;
    }

    [RequirePermission(Permissions.Roles.View)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetRolesAsync(cancellationToken);
        return Ok(items);
    }

    [RequirePermission(Permissions.Roles.View)]
    [HttpGet("permissions/catalog")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDefinitionDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<PermissionDefinitionDto>> GetPermissionCatalog()
    {
        return Ok(_service.GetPermissionCatalog());
    }

    [RequirePermission(Permissions.Roles.View)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetRoleByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [RequirePermission(Permissions.Roles.Manage)]
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RoleDto>> Create(
        [FromBody] CreateRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [RequirePermission(Permissions.Roles.Manage)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateRoleAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [RequirePermission(Permissions.Roles.Manage)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteRoleAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
