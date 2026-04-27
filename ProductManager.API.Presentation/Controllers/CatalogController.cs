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

    [HttpPost("categories")]
    [ProducesResponseType(typeof(ProductCategoryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductCategoryDto>> CreateCategory(
        [FromBody] CreateProductCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdCategory = await _service.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.Id }, createdCategory);
    }

    [HttpPut("categories/{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        Guid categoryId,
        [FromBody] UpdateProductCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateCategoryAsync(categoryId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("categories/{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteCategoryAsync(categoryId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
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

    [HttpPost("suppliers")]
    [ProducesResponseType(typeof(ProductSupplierDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductSupplierDto>> CreateSupplier(
        [FromBody] CreateProductSupplierRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdSupplier = await _service.CreateSupplierAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetSupplierById), new { supplierId = createdSupplier.Id }, createdSupplier);
    }

    [HttpPut("suppliers/{supplierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier(
        Guid supplierId,
        [FromBody] UpdateProductSupplierRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateSupplierAsync(supplierId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("suppliers/{supplierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier(Guid supplierId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteSupplierAsync(supplierId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
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

    [HttpPost("warehouses")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse(
        [FromBody] CreateWarehouseRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdWarehouse = await _service.CreateWarehouseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetWarehouseById), new { warehouseId = createdWarehouse.Id }, createdWarehouse);
    }

    [HttpPut("warehouses/{warehouseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWarehouse(
        Guid warehouseId,
        [FromBody] UpdateWarehouseRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateWarehouseAsync(warehouseId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("warehouses/{warehouseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWarehouse(Guid warehouseId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteWarehouseAsync(warehouseId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
