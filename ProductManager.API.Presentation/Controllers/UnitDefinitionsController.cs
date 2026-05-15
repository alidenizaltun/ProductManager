using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/unit-definitions")]
public sealed class UnitDefinitionsController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public UnitDefinitionsController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UnitDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UnitDefinitionDto>>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetUnitDefinitionsAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UnitDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnitDefinitionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetUnitDefinitionByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UnitDefinitionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UnitDefinitionDto>> Create(
        [FromBody] CreateUnitDefinitionRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateUnitDefinitionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUnitDefinitionRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateUnitDefinitionAsync(id, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteUnitDefinitionAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
