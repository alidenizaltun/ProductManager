using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Controllers;

/// <summary>
/// Herkese açık ürün vitrini. B2B paneli ve dış tüketiciler bu uçları token'sız çağırır.
/// Açıklık bilinçlidir ve <see cref="AllowAnonymousAttribute"/> ile <b>açıkça</b> beyan edilir —
/// "attribute unutulmuş" ile "bilerek açık" arasındaki farkı görünür kılmak için.
/// </summary>
[ApiController]
[Route("api/public/products")]
[AllowAnonymous]
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
