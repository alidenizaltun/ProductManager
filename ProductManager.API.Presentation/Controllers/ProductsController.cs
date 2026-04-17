using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public ProductsController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(
        [FromQuery] ProductFilterDto? filter,
        CancellationToken cancellationToken)
    {
        var effectiveFilter = filter ?? new ProductFilterDto();
        var products = await _service.GetProductsAsync(effectiveFilter, cancellationToken);
        return Ok(products);
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _service.GetProductByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var createdProduct = await _service.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { productId = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid productId,
        [FromBody] UpdateProductRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateProductAsync(productId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid productId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteProductAsync(productId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
