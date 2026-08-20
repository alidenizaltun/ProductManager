using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/pricelists")]
public sealed class PriceListsController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public PriceListsController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductPriceListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductPriceListDto>>> GetPriceLists(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var priceLists = await _service.GetPriceListsAsync(includeInactive, cancellationToken);
        return Ok(priceLists);
    }

    [HttpGet("{priceListId:guid}")]
    [ProducesResponseType(typeof(ProductPriceListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceListDto>> GetPriceListById(Guid priceListId, CancellationToken cancellationToken)
    {
        var priceList = await _service.GetPriceListByIdAsync(priceListId, cancellationToken);
        if (priceList is null)
        {
            return NotFound();
        }

        return Ok(priceList);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductPriceListDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductPriceListDto>> CreatePriceList(
        [FromBody] CreateProductPriceListRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdPriceList = await _service.CreatePriceListAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPriceListById), new { priceListId = createdPriceList.Id }, createdPriceList);
    }

    [HttpPut("{priceListId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePriceList(
        Guid priceListId,
        [FromBody] UpdateProductPriceListRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdatePriceListAsync(priceListId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{priceListId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePriceList(Guid priceListId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePriceListAsync(priceListId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{priceListId:guid}/items")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductPriceListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductPriceListItemDto>>> GetPriceListItems(Guid priceListId, CancellationToken cancellationToken)
    {
        var items = await _service.GetPriceListItemsAsync(priceListId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("items/{priceListItemId:guid}")]
    [ProducesResponseType(typeof(ProductPriceListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceListItemDto>> GetPriceListItemById(Guid priceListItemId, CancellationToken cancellationToken)
    {
        var item = await _service.GetPriceListItemByIdAsync(priceListItemId, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(ProductPriceListItemDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductPriceListItemDto>> CreatePriceListItem(
        [FromBody] CreateProductPriceListItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdItem = await _service.CreatePriceListItemAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPriceListItemById), new { priceListItemId = createdItem.Id }, createdItem);
    }

    [HttpPut("items/{priceListItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePriceListItem(
        Guid priceListItemId,
        [FromBody] UpdateProductPriceListItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdatePriceListItemAsync(priceListItemId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("items/{priceListItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePriceListItem(Guid priceListItemId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePriceListItemAsync(priceListItemId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
