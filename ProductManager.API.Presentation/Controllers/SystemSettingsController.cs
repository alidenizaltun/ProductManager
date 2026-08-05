using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.SystemManagement;
using ProductManager.Shared.Infrastructure.Security;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/system-settings")]
public sealed class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _service;

    public SystemSettingsController(ISystemSettingsService service)
    {
        _service = service;
    }

    [RequirePermission(Permissions.Settings.View)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SystemSettingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SystemSettingDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetSettingsAsync(cancellationToken);
        return Ok(items);
    }

    [RequirePermission(Permissions.Settings.Manage)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BulkUpdate(
        [FromBody] BulkUpdateSystemSettingsRequestDto request,
        CancellationToken cancellationToken)
    {
        await _service.BulkUpdateAsync(request, cancellationToken);
        return NoContent();
    }
}
