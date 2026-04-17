using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public CatalogController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductCategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _service.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("categories/{categoryId:guid}")]
    [ProducesResponseType(typeof(ProductCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductCategoryDto>> GetCategoryById(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _service.GetCategoryByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpGet("suppliers")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductSupplierDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductSupplierDto>>> GetSuppliers(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var suppliers = await _service.GetSuppliersAsync(includeInactive, cancellationToken);
        return Ok(suppliers);
    }

    [HttpGet("suppliers/{supplierId:guid}")]
    [ProducesResponseType(typeof(ProductSupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductSupplierDto>> GetSupplierById(Guid supplierId, CancellationToken cancellationToken)
    {
        var supplier = await _service.GetSupplierByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    [HttpGet("warehouses")]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetWarehouses(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var warehouses = await _service.GetWarehousesAsync(includeInactive, cancellationToken);
        return Ok(warehouses);
    }

    [HttpGet("warehouses/{warehouseId:guid}")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseDto>> GetWarehouseById(Guid warehouseId, CancellationToken cancellationToken)
    {
        var warehouse = await _service.GetWarehouseByIdAsync(warehouseId, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        return Ok(warehouse);
    }
}
