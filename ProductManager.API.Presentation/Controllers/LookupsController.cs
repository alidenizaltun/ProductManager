using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/lookups")]
public sealed class LookupsController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public LookupsController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProductReferenceLookupsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductReferenceLookupsDto>> GetReferenceLookups(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var lookups = await _service.GetReferenceLookupsAsync(includeInactive, cancellationToken);
        return Ok(lookups);
    }

    [HttpGet("products")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetProducts(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetProductLookupsAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetCategories(CancellationToken cancellationToken = default)
    {
        var items = await _service.GetCategoryLookupsAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetWarehouses(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetWarehouseLookupsAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetSuppliers(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetSupplierLookupsAsync(includeInactive, cancellationToken);
        return Ok(items);
    }

    [HttpGet("price-lists")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetPriceLists(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.GetPriceListLookupsAsync(includeInactive, cancellationToken);
        return Ok(items);
    }
}
