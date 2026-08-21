using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductCommerceController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public ProductCommerceController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet("{productId:guid}/variants")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductVariantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductVariantDto>>> GetVariants(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var variants = await _service.GetProductVariantsAsync(productId, cancellationToken);
        return Ok(variants);
    }

    [HttpGet("variants/{variantId:guid}")]
    [ProducesResponseType(typeof(ProductVariantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductVariantDto>> GetVariantById(Guid variantId, CancellationToken cancellationToken)
    {
        var variant = await _service.GetVariantByIdAsync(variantId, cancellationToken);
        if (variant is null)
        {
            return NotFound();
        }

        return Ok(variant);
    }

    [HttpPost("{productId:guid}/variants")]
    [ProducesResponseType(typeof(ProductVariantDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductVariantDto>> CreateVariant(
        Guid productId,
        [FromBody] CreateProductVariantRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { ProductId = productId };
        var createdVariant = await _service.CreateVariantAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetVariantById), new { variantId = createdVariant.Id }, createdVariant);
    }

    [HttpPut("variants/{variantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVariant(
        Guid variantId,
        [FromBody] UpdateProductVariantRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateVariantAsync(variantId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("variants/{variantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVariant(Guid variantId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteVariantAsync(variantId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{productId:guid}/prices")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductPriceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductPriceDto>>> GetPrices(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var prices = await _service.GetProductPricesAsync(productId, cancellationToken);
        return Ok(prices);
    }

    [HttpGet("prices/{priceId:guid}")]
    [ProducesResponseType(typeof(ProductPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceDto>> GetPriceById(Guid priceId, CancellationToken cancellationToken)
    {
        var price = await _service.GetPriceByIdAsync(priceId, cancellationToken);
        if (price is null)
        {
            return NotFound();
        }

        return Ok(price);
    }

    [HttpPost("{productId:guid}/prices")]
    [ProducesResponseType(typeof(ProductPriceDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductPriceDto>> CreatePrice(
        Guid productId,
        [FromBody] CreateProductPriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { ProductId = productId };
        var createdPrice = await _service.CreatePriceAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetPriceById), new { priceId = createdPrice.Id }, createdPrice);
    }

    [HttpPut("prices/{priceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice(
        Guid priceId,
        [FromBody] UpdateProductPriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdatePriceAsync(priceId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("prices/{priceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePrice(Guid priceId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePriceAsync(priceId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{productId:guid}/pricing-rules")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductPricingRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductPricingRuleDto>>> GetPricingRules(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var rules = await _service.GetProductPricingRulesAsync(productId, cancellationToken);
        return Ok(rules);
    }

    [HttpGet("pricing-rules/{pricingRuleId:guid}")]
    [ProducesResponseType(typeof(ProductPricingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPricingRuleDto>> GetPricingRuleById(
        Guid pricingRuleId,
        CancellationToken cancellationToken)
    {
        var rule = await _service.GetPricingRuleByIdAsync(pricingRuleId, cancellationToken);
        if (rule is null)
        {
            return NotFound();
        }

        return Ok(rule);
    }

    [HttpPost("{productId:guid}/pricing-rules")]
    [ProducesResponseType(typeof(ProductPricingRuleDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductPricingRuleDto>> CreatePricingRule(
        Guid productId,
        [FromBody] CreateProductPricingRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var normalizedRequest = request with { ProductId = productId };
        var createdRule = await _service.CreatePricingRuleAsync(normalizedRequest, cancellationToken);
        return CreatedAtAction(nameof(GetPricingRuleById), new { pricingRuleId = createdRule.Id }, createdRule);
    }

    [HttpPut("pricing-rules/{pricingRuleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePricingRule(
        Guid pricingRuleId,
        [FromBody] UpdateProductPricingRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdatePricingRuleAsync(pricingRuleId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{productId:guid}/pricing-rules/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderPricingRules(
        Guid productId,
        [FromBody] ReorderProductPricingRulesRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.ReorderPricingRulesAsync(productId, request.OrderedPricingRuleIds, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Var olan bir fiyatlandırma kuralını, başka ürünlerde yeniden kullanılmak üzere
    /// ürün bağımsız bir fiyat şablonuna dönüştürür.
    /// </summary>
    [HttpPost("pricing-rules/{pricingRuleId:guid}/save-as-template")]
    [ProducesResponseType(typeof(PricingTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PricingTemplateDto>> SavePricingRuleAsTemplate(
        Guid pricingRuleId,
        [FromBody] SavePricingRuleAsTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        var template = await _service.SavePricingRuleAsTemplateAsync(pricingRuleId, request, cancellationToken);
        return Created($"/api/pricing-templates/{template.Id}", template);
    }

    [HttpDelete("pricing-rules/{pricingRuleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePricingRule(
        Guid pricingRuleId,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePricingRuleAsync(pricingRuleId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{productId:guid}/units")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductUnitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductUnitDto>>> GetProductUnits(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var units = await _service.GetProductUnitsAsync(productId, cancellationToken);
        return Ok(units);
    }

    [HttpGet("units/{productUnitId:guid}")]
    [ProducesResponseType(typeof(ProductUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductUnitDto>> GetProductUnitById(
        Guid productUnitId,
        CancellationToken cancellationToken)
    {
        var unit = await _service.GetProductUnitByIdAsync(productUnitId, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpPost("{productId:guid}/units")]
    [ProducesResponseType(typeof(ProductUnitDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductUnitDto>> CreateProductUnit(
        Guid productId,
        [FromBody] CreateProductUnitRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateProductUnitAsync(request with { ProductId = productId }, cancellationToken);
        return CreatedAtAction(nameof(GetProductUnitById), new { productUnitId = created.Id }, created);
    }

    [HttpPut("units/{productUnitId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductUnit(
        Guid productUnitId,
        [FromBody] UpdateProductUnitRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateProductUnitAsync(productUnitId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("units/{productUnitId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductUnit(
        Guid productUnitId,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteProductUnitAsync(productUnitId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
