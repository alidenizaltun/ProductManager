using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.Identity;
using ProductManagement.Shared.Infrastructure.Security;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserManagementService _service;

    public UsersController(IUserManagementService service)
    {
        _service = service;
    }

    [RequirePermission(Permissions.Users.View)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetUsersAsync(search, includeInactive, cancellationToken);
        return Ok(items);
    }

    [RequirePermission(Permissions.Users.View)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetUserByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [RequirePermission(Permissions.Users.Manage)]
    [HttpPost]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminUserDto>> Create(
        [FromBody] CreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [RequirePermission(Permissions.Users.Manage)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateUserAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [RequirePermission(Permissions.Users.Manage)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var deactivated = await _service.DeactivateUserAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound();
    }

    [RequirePermission(Permissions.Users.Manage)]
    [HttpPost("{id:guid}/resend-invitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendInvitation(Guid id, CancellationToken cancellationToken)
    {
        var sent = await _service.ResendInvitationAsync(id, cancellationToken);
        return sent ? NoContent() : NotFound();
    }
}
