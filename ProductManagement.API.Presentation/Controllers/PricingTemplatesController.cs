using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Security;

namespace ProductManagement.Presentation.Controllers;

/// <summary>
/// Ürün bağımsız fiyat şablonları. Bir kez kurulan fiyatlandırma buraya kaydedilir
/// ve başka ürünlere kopyalanarak hızlıca uygulanır.
/// </summary>
[ApiController]
[Route("api/pricing-templates")]
public sealed class PricingTemplatesController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public PricingTemplatesController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission(Permissions.PricingTemplates.View)]
    [ProducesResponseType(typeof(IReadOnlyList<PricingTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PricingTemplateDto>>> GetPricingTemplates(
        [FromQuery] int? templateKind = null,
        [FromQuery] Guid? unitDefinitionId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var templates = await _service.GetPricingTemplatesAsync(templateKind, unitDefinitionId, includeInactive, cancellationToken);
        return Ok(templates);
    }

    [HttpGet("{pricingTemplateId:guid}")]
    [RequirePermission(Permissions.PricingTemplates.View)]
    [ProducesResponseType(typeof(PricingTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PricingTemplateDto>> GetPricingTemplateById(
        Guid pricingTemplateId,
        CancellationToken cancellationToken)
    {
        var template = await _service.GetPricingTemplateByIdAsync(pricingTemplateId, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }

    /// <summary>Şablonun hangi ürünlerde kullanıldığını ve sürüm farkını döner.</summary>
    [HttpGet("{pricingTemplateId:guid}/usages")]
    [RequirePermission(Permissions.PricingTemplates.View)]
    [ProducesResponseType(typeof(IReadOnlyList<PricingTemplateUsageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PricingTemplateUsageDto>>> GetPricingTemplateUsages(
        Guid pricingTemplateId,
        CancellationToken cancellationToken)
    {
        var usages = await _service.GetPricingTemplateUsagesAsync(pricingTemplateId, cancellationToken);
        return Ok(usages);
    }

    [HttpPost]
    [RequirePermission(Permissions.PricingTemplates.Manage)]
    [ProducesResponseType(typeof(PricingTemplateDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PricingTemplateDto>> CreatePricingTemplate(
        [FromBody] CreatePricingTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreatePricingTemplateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPricingTemplateById), new { pricingTemplateId = created.Id }, created);
    }

    [HttpPut("{pricingTemplateId:guid}")]
    [RequirePermission(Permissions.PricingTemplates.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePricingTemplate(
        Guid pricingTemplateId,
        [FromBody] UpdatePricingTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdatePricingTemplateAsync(pricingTemplateId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{pricingTemplateId:guid}")]
    [RequirePermission(Permissions.PricingTemplates.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePricingTemplate(
        Guid pricingTemplateId,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePricingTemplateAsync(pricingTemplateId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Şablonu tek bir ürüne uygular.</summary>
    [HttpPost("{pricingTemplateId:guid}/apply")]
    [RequirePermission(Permissions.PricingTemplates.Manage)]
    [ProducesResponseType(typeof(ApplyPricingTemplateResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplyPricingTemplateResultDto>> ApplyPricingTemplate(
        Guid pricingTemplateId,
        [FromBody] ApplyPricingTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ApplyPricingTemplateAsync(pricingTemplateId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Şablonu birden çok ürüne uygular. Her ürün ayrı işlendiği için sonuç listesi
    /// başarılı ve başarısız ürünleri birlikte döner.
    /// </summary>
    [HttpPost("{pricingTemplateId:guid}/apply-bulk")]
    [RequirePermission(Permissions.PricingTemplates.Manage)]
    [ProducesResponseType(typeof(IReadOnlyList<ApplyPricingTemplateResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApplyPricingTemplateResultDto>>> ApplyPricingTemplateBulk(
        Guid pricingTemplateId,
        [FromBody] ApplyPricingTemplateBulkRequestDto request,
        CancellationToken cancellationToken)
    {
        var results = await _service.ApplyPricingTemplateBulkAsync(pricingTemplateId, request, cancellationToken);
        return Ok(results);
    }
}
