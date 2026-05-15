using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

/// <summary>
/// Harici projelerin (dış istemciler) tüketmesi için tasarlanmış salt okunur ürün endpointleri.
/// Kimlik doğrulama gerektirmez; yalnızca listeleme ve detay sorgularını destekler.
/// </summary>
[ApiController]
[Route("api/public/products")]
public sealed class PublicProductsController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public PublicProductsController(IProductOperationsService service)
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
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetProductDetail(
    Guid productId,
    CancellationToken cancellationToken)
    {
        var product = await _service.GetProductDetailByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
