using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductRelationsController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public ProductRelationsController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet("{productId:guid}/attribute-values")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductAttributeValueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductAttributeValueDto>>> GetAttributeValues(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var values = await _service.GetProductAttributeValuesAsync(productId, cancellationToken);
        return Ok(values);
    }

    [HttpGet("attribute-values/{attributeValueId:guid}")]
    [ProducesResponseType(typeof(ProductAttributeValueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductAttributeValueDto>> GetAttributeValueById(
        Guid attributeValueId,
        CancellationToken cancellationToken)
    {
        var value = await _service.GetAttributeValueByIdAsync(attributeValueId, cancellationToken);
        if (value is null)
        {
            return NotFound();
        }

        return Ok(value);
    }

    [HttpPost("{productId:guid}/attribute-values")]
    [ProducesResponseType(typeof(ProductAttributeValueDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductAttributeValueDto>> CreateAttributeValue(
        Guid productId,
        [FromBody] CreateProductAttributeValueRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { ProductId = productId };
        var createdValue = await _service.CreateAttributeValueAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetAttributeValueById), new { attributeValueId = createdValue.Id }, createdValue);
    }

    [HttpPut("attribute-values/{attributeValueId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAttributeValue(
        Guid attributeValueId,
        [FromBody] UpdateProductAttributeValueRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAttributeValueAsync(attributeValueId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("attribute-values/{attributeValueId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttributeValue(Guid attributeValueId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAttributeValueAsync(attributeValueId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{productId:guid}/category-maps")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductCategoryMapDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductCategoryMapDto>>> GetCategoryMaps(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var maps = await _service.GetProductCategoryMapsAsync(productId, cancellationToken);
        return Ok(maps);
    }

    [HttpGet("category-maps/{categoryMapId:guid}")]
    [ProducesResponseType(typeof(ProductCategoryMapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductCategoryMapDto>> GetCategoryMapById(
        Guid categoryMapId,
        CancellationToken cancellationToken)
    {
        var map = await _service.GetCategoryMapByIdAsync(categoryMapId, cancellationToken);
        if (map is null)
        {
            return NotFound();
        }

        return Ok(map);
    }

    [HttpPost("{productId:guid}/category-maps")]
    [ProducesResponseType(typeof(ProductCategoryMapDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductCategoryMapDto>> CreateCategoryMap(
        Guid productId,
        [FromBody] CreateProductCategoryMapRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { ProductId = productId };
        var createdMap = await _service.CreateCategoryMapAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryMapById), new { categoryMapId = createdMap.Id }, createdMap);
    }

    [HttpPut("category-maps/{categoryMapId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategoryMap(
        Guid categoryMapId,
        [FromBody] UpdateProductCategoryMapRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateCategoryMapAsync(categoryMapId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("category-maps/{categoryMapId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategoryMap(Guid categoryMapId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteCategoryMapAsync(categoryMapId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{productId:guid}/media")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductMediaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductMediaDto>>> GetMedia(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var mediaItems = await _service.GetProductMediaAsync(productId, cancellationToken);
        return Ok(mediaItems);
    }

    [HttpGet("media/{mediaId:guid}")]
    [ProducesResponseType(typeof(ProductMediaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductMediaDto>> GetMediaById(Guid mediaId, CancellationToken cancellationToken)
    {
        var media = await _service.GetMediaByIdAsync(mediaId, cancellationToken);
        if (media is null)
        {
            return NotFound();
        }

        return Ok(media);
    }

    [HttpPost("{productId:guid}/media")]
    [ProducesResponseType(typeof(ProductMediaDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductMediaDto>> CreateMedia(
        Guid productId,
        [FromBody] CreateProductMediaRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { ProductId = productId };
        var createdMedia = await _service.CreateMediaAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetMediaById), new { mediaId = createdMedia.Id }, createdMedia);
    }

    [HttpPut("media/{mediaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMedia(
        Guid mediaId,
        [FromBody] UpdateProductMediaRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateMediaAsync(mediaId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("media/{mediaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedia(Guid mediaId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteMediaAsync(mediaId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{productId:guid}/bundle-items")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductBundleItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductBundleItemDto>>> GetBundleItems(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var items = await _service.GetBundleItemsAsync(productId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("bundle-items/{bundleItemId:guid}")]
    [ProducesResponseType(typeof(ProductBundleItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductBundleItemDto>> GetBundleItemById(
        Guid bundleItemId,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetBundleItemByIdAsync(bundleItemId, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost("{productId:guid}/bundle-items")]
    [ProducesResponseType(typeof(ProductBundleItemDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductBundleItemDto>> CreateBundleItem(
        Guid productId,
        [FromBody] CreateProductBundleItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { BundleProductId = productId };
        var createdItem = await _service.CreateBundleItemAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetBundleItemById), new { bundleItemId = createdItem.Id }, createdItem);
    }

    [HttpPut("bundle-items/{bundleItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBundleItem(
        Guid bundleItemId,
        [FromBody] UpdateProductBundleItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateBundleItemAsync(bundleItemId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("bundle-items/{bundleItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBundleItem(Guid bundleItemId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteBundleItemAsync(bundleItemId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{productId:guid}/supplier-maps")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductSupplierMapDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductSupplierMapDto>>> GetSupplierMaps(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var maps = await _service.GetProductSupplierMapsAsync(productId, cancellationToken);
        return Ok(maps);
    }

    [HttpGet("supplier-maps/{supplierMapId:guid}")]
    [ProducesResponseType(typeof(ProductSupplierMapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductSupplierMapDto>> GetSupplierMapById(
        Guid supplierMapId,
        CancellationToken cancellationToken)
    {
        var map = await _service.GetSupplierMapByIdAsync(supplierMapId, cancellationToken);
        if (map is null)
        {
            return NotFound();
        }

        return Ok(map);
    }

    [HttpPost("{productId:guid}/supplier-maps")]
    [ProducesResponseType(typeof(ProductSupplierMapDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductSupplierMapDto>> CreateSupplierMap(
        Guid productId,
        [FromBody] CreateProductSupplierMapRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { ProductId = productId };
        var createdMap = await _service.CreateSupplierMapAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetSupplierMapById), new { supplierMapId = createdMap.Id }, createdMap);
    }

    [HttpPut("supplier-maps/{supplierMapId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplierMap(
        Guid supplierMapId,
        [FromBody] UpdateProductSupplierMapRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateSupplierMapAsync(supplierMapId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("supplier-maps/{supplierMapId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplierMap(Guid supplierMapId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteSupplierMapAsync(supplierMapId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
