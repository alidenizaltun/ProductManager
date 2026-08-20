using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/attributes")]
public sealed class AttributesController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public AttributesController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductAttributeDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductAttributeDefinitionDto>>> GetAttributeDefinitions(CancellationToken cancellationToken)
    {
        var attributes = await _service.GetAttributeDefinitionsAsync(cancellationToken);
        return Ok(attributes);
    }

    [HttpGet("{attributeDefinitionId:guid}")]
    [ProducesResponseType(typeof(ProductAttributeDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductAttributeDefinitionDto>> GetAttributeDefinitionById(Guid attributeDefinitionId, CancellationToken cancellationToken)
    {
        var attribute = await _service.GetAttributeDefinitionByIdAsync(attributeDefinitionId, cancellationToken);
        if (attribute is null)
        {
            return NotFound();
        }

        return Ok(attribute);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductAttributeDefinitionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductAttributeDefinitionDto>> CreateAttributeDefinition(
        [FromBody] CreateProductAttributeDefinitionRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdAttribute = await _service.CreateAttributeDefinitionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAttributeDefinitionById), new { attributeDefinitionId = createdAttribute.Id }, createdAttribute);
    }

    [HttpPut("{attributeDefinitionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAttributeDefinition(
        Guid attributeDefinitionId,
        [FromBody] UpdateProductAttributeDefinitionRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAttributeDefinitionAsync(attributeDefinitionId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{attributeDefinitionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttributeDefinition(Guid attributeDefinitionId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAttributeDefinitionAsync(attributeDefinitionId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
