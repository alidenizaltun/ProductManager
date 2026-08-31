using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.PriceEngine;
using ProductManagement.Shared.Infrastructure.Security;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Security;

namespace ProductManagement.Presentation.Controllers;

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

    [RequirePermission(Permissions.Products.View)]
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

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("modules")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductModuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductModuleDto>>> GetModules(Guid productId, CancellationToken cancellationToken)
    {
        var items = await _service.GetProductModulesAsync(productId, cancellationToken);
        return Ok(items);
    }

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("modules/{moduleId:guid}")]
    [ProducesResponseType(typeof(ProductModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductModuleDto>> GetModuleById(Guid productId, Guid moduleId, CancellationToken cancellationToken)
    {
        var item = await _service.GetProductModuleByIdAsync(moduleId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [RequirePermission(Permissions.Products.Manage)]
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

    [RequirePermission(Permissions.Products.Manage)]
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

    [RequirePermission(Permissions.Products.Manage)]
    [HttpDelete("modules/{moduleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteModule(Guid productId, Guid moduleId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteProductModuleAsync(moduleId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Module Offering Prices ───────────────────────────────────────────────────

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("modules/{moduleId:guid}/offering-prices")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductModuleOfferingPriceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductModuleOfferingPriceDto>>> GetModuleOfferingPrices(
        Guid productId, Guid moduleId, CancellationToken cancellationToken)
    {
        var items = await _service.GetModuleOfferingPricesAsync(moduleId, cancellationToken);
        return Ok(items);
    }

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("modules/{moduleId:guid}/offering-prices/{priceId:guid}")]
    [ProducesResponseType(typeof(ProductModuleOfferingPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductModuleOfferingPriceDto>> GetModuleOfferingPriceById(
        Guid productId, Guid moduleId, Guid priceId, CancellationToken cancellationToken)
    {
        var item = await _service.GetModuleOfferingPriceByIdAsync(priceId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpPost("modules/{moduleId:guid}/offering-prices")]
    [ProducesResponseType(typeof(ProductModuleOfferingPriceDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductModuleOfferingPriceDto>> CreateModuleOfferingPrice(
        Guid productId, Guid moduleId,
        [FromBody] CreateProductModuleOfferingPriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateModuleOfferingPriceAsync(
            request with { ProductModuleId = moduleId }, cancellationToken);
        return CreatedAtAction(nameof(GetModuleOfferingPriceById),
            new { productId, moduleId, priceId = created.Id }, created);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpPut("modules/{moduleId:guid}/offering-prices/{priceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateModuleOfferingPrice(
        Guid productId, Guid moduleId, Guid priceId,
        [FromBody] UpdateProductModuleOfferingPriceRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateModuleOfferingPriceAsync(priceId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpDelete("modules/{moduleId:guid}/offering-prices/{priceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteModuleOfferingPrice(
        Guid productId, Guid moduleId, Guid priceId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteModuleOfferingPriceAsync(priceId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── LicenseOfferings ─────────────────────────────────────────────────────────

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("license-offerings")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductLicenseOfferingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductLicenseOfferingDto>>> GetLicenseOfferings(Guid productId, CancellationToken cancellationToken)
    {
        var items = await _service.GetProductLicenseOfferingsAsync(productId, cancellationToken);
        return Ok(items);
    }

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("license-offerings/{offeringId:guid}")]
    [ProducesResponseType(typeof(ProductLicenseOfferingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductLicenseOfferingDto>> GetLicenseOfferingById(Guid productId, Guid offeringId, CancellationToken cancellationToken)
    {
        var item = await _service.GetProductLicenseOfferingByIdAsync(offeringId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [RequirePermission(Permissions.Products.Manage)]
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

    [RequirePermission(Permissions.Products.Manage)]
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

    [RequirePermission(Permissions.Products.Manage)]
    [HttpDelete("license-offerings/{offeringId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLicenseOffering(Guid productId, Guid offeringId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteProductLicenseOfferingAsync(offeringId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
