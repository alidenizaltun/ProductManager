using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.PriceEngine;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/products/{productId:guid}")]
public sealed class SoftwareLicenseController : ControllerBase
{
    private readonly IProductOperationsService _service;
    private readonly IPriceEngineService _priceEngineService;

    public SoftwareLicenseController(
        IProductOperationsService service,
        IPriceEngineService priceEngineService)
    {
        _service = service;
        _priceEngineService = priceEngineService;
    }

    /// <summary>
    /// Seçilen lisans teklifi için UI'da gösterilecek birim parametrelerini döner (Kullanıcı, API istek vb.).
    /// </summary>
    [HttpGet("license-offerings/{offeringId:guid}/pricing-parameters")]
    [ProducesResponseType(typeof(LicenseOfferingPricingParametersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LicenseOfferingPricingParametersDto>> GetLicenseOfferingPricingParameters(
        Guid productId,
        Guid offeringId,
        CancellationToken cancellationToken)
    {
        var parameters = await _priceEngineService.GetLicenseOfferingPricingParametersAsync(
            productId, offeringId, cancellationToken);

        return parameters is null ? NotFound() : Ok(parameters);
    }

    // ─── Modules ─────────────────────────────────────────────────────────────────

    [HttpGet("modules")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductModuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductModuleDto>>> GetModules(Guid productId, CancellationToken cancellationToken)
    {
        var items = await _service.GetProductModulesAsync(productId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("modules/{moduleId:guid}")]
    [ProducesResponseType(typeof(ProductModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductModuleDto>> GetModuleById(Guid productId, Guid moduleId, CancellationToken cancellationToken)
    {
        var item = await _service.GetProductModuleByIdAsync(moduleId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("modules")]
    [ProducesResponseType(typeof(ProductModuleDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductModuleDto>> CreateModule(
    Guid productId,
    [FromBody] CreateProductModuleRequestDto request,
    CancellationToken cancellationToken)
    {
        var created = await _service.CreateProductModuleAsync(
        request with { ProductId = productId }, cancellationToken);
        return CreatedAtAction(nameof(GetModuleById), new { productId, moduleId = created.Id }, created);
    }

    [HttpPut("modules/{moduleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateModule(
    Guid productId, Guid moduleId,
    [FromBody] UpdateProductModuleRequestDto request,
    CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateProductModuleAsync(moduleId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("modules/{moduleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteModule(Guid productId, Guid moduleId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteProductModuleAsync(moduleId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── SoftwarePricingTiers ─────────────────────────────────────────────────────

    [HttpGet("pricing-tiers")]
    [ProducesResponseType(typeof(IReadOnlyList<SoftwarePricingTierDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SoftwarePricingTierDto>>> GetPricingTiers(Guid productId, CancellationToken cancellationToken)
    {
        var items = await _service.GetSoftwarePricingTiersAsync(productId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("pricing-tiers/{tierId:guid}")]
    [ProducesResponseType(typeof(SoftwarePricingTierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SoftwarePricingTierDto>> GetPricingTierById(Guid productId, Guid tierId, CancellationToken cancellationToken)
    {
        var item = await _service.GetSoftwarePricingTierByIdAsync(tierId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("pricing-tiers")]
    [ProducesResponseType(typeof(SoftwarePricingTierDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<SoftwarePricingTierDto>> CreatePricingTier(
    Guid productId,
    [FromBody] CreateSoftwarePricingTierRequestDto request,
    CancellationToken cancellationToken)
    {
        var created = await _service.CreateSoftwarePricingTierAsync(
        request with { ProductId = productId }, cancellationToken);
        return CreatedAtAction(nameof(GetPricingTierById), new { productId, tierId = created.Id }, created);
    }

    [HttpPut("pricing-tiers/{tierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePricingTier(
    Guid productId, Guid tierId,
    [FromBody] UpdateSoftwarePricingTierRequestDto request,
    CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateSoftwarePricingTierAsync(tierId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("pricing-tiers/{tierId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePricingTier(Guid productId, Guid tierId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteSoftwarePricingTierAsync(tierId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── LicenseOfferings ─────────────────────────────────────────────────────────

    [HttpGet("license-offerings")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductLicenseOfferingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductLicenseOfferingDto>>> GetLicenseOfferings(Guid productId, CancellationToken cancellationToken)
    {
        var items = await _service.GetProductLicenseOfferingsAsync(productId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("license-offerings/{offeringId:guid}")]
    [ProducesResponseType(typeof(ProductLicenseOfferingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductLicenseOfferingDto>> GetLicenseOfferingById(Guid productId, Guid offeringId, CancellationToken cancellationToken)
    {
        var item = await _service.GetProductLicenseOfferingByIdAsync(offeringId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("license-offerings")]
    [ProducesResponseType(typeof(ProductLicenseOfferingDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductLicenseOfferingDto>> CreateLicenseOffering(
    Guid productId,
    [FromBody] CreateProductLicenseOfferingRequestDto request,
    CancellationToken cancellationToken)
    {
        var created = await _service.CreateProductLicenseOfferingAsync(
        request with { ProductId = productId }, cancellationToken);
        return CreatedAtAction(nameof(GetLicenseOfferingById), new { productId, offeringId = created.Id }, created);
    }

    [HttpPut("license-offerings/{offeringId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLicenseOffering(
    Guid productId, Guid offeringId,
    [FromBody] UpdateProductLicenseOfferingRequestDto request,
    CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateProductLicenseOfferingAsync(offeringId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("license-offerings/{offeringId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLicenseOffering(Guid productId, Guid offeringId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteProductLicenseOfferingAsync(offeringId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
