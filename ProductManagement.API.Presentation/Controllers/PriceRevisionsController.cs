using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Security;

namespace ProductManagement.Presentation.Controllers;

/// <summary>
/// Zam yönetimi. Bir revizyon taslak olarak açılır, kapsamı seçilir, önizlenir,
/// onaya gönderilir, onaylanır, uygulanır ve gerekirse geri alınır.
/// </summary>
[ApiController]
[Route("api/price-revisions")]
public sealed class PriceRevisionsController : ControllerBase
{
    private readonly IProductOperationsService _service;
    private readonly ICurrentUserService _currentUserService;

    public PriceRevisionsController(
        IProductOperationsService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [RequirePermission(Permissions.PriceRevisions.View)]
    [ProducesResponseType(typeof(IReadOnlyList<PriceRevisionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PriceRevisionDto>>> GetPriceRevisions(
        [FromQuery] int? status = null,
        CancellationToken cancellationToken = default)
    {
        var revisions = await _service.GetPriceRevisionsAsync(status, cancellationToken);
        return Ok(revisions);
    }

    [HttpGet("{priceRevisionId:guid}")]
    [RequirePermission(Permissions.PriceRevisions.View)]
    [ProducesResponseType(typeof(PriceRevisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceRevisionDto>> GetPriceRevisionById(
        Guid priceRevisionId,
        CancellationToken cancellationToken)
    {
        var revision = await _service.GetPriceRevisionByIdAsync(priceRevisionId, cancellationToken);
        return revision is null ? NotFound() : Ok(revision);
    }

    [HttpPost]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(typeof(PriceRevisionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PriceRevisionDto>> CreatePriceRevision(
        [FromBody] CreatePriceRevisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreatePriceRevisionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPriceRevisionById), new { priceRevisionId = created.Id }, created);
    }

    [HttpPut("{priceRevisionId:guid}")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePriceRevision(
        Guid priceRevisionId,
        [FromBody] UpdatePriceRevisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdatePriceRevisionAsync(priceRevisionId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{priceRevisionId:guid}")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePriceRevision(
        Guid priceRevisionId,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePriceRevisionAsync(priceRevisionId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Kapsam ──────────────────────────────────────────────────────────────────

    [HttpPost("{priceRevisionId:guid}/scopes")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(typeof(PriceRevisionScopeDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PriceRevisionScopeDto>> CreateScope(
        Guid priceRevisionId,
        [FromBody] CreatePriceRevisionScopeRequestDto request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreatePriceRevisionScopeAsync(priceRevisionId, request, cancellationToken);
        return CreatedAtAction(nameof(GetPriceRevisionById), new { priceRevisionId }, created);
    }

    [HttpDelete("{priceRevisionId:guid}/scopes/{scopeId:guid}")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScope(
        Guid priceRevisionId,
        Guid scopeId,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePriceRevisionScopeAsync(priceRevisionId, scopeId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Önizleme ────────────────────────────────────────────────────────────────

    /// <summary>Kapsamı tarar ve etkilenecek her fiyat için bir satır üretir.</summary>
    [HttpPost("{priceRevisionId:guid}/preview")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(typeof(PriceRevisionSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PriceRevisionSummaryDto>> Preview(
        Guid priceRevisionId,
        CancellationToken cancellationToken)
    {
        var summary = await _service.PreviewPriceRevisionAsync(priceRevisionId, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("{priceRevisionId:guid}/lines")]
    [RequirePermission(Permissions.PriceRevisions.View)]
    [ProducesResponseType(typeof(PriceRevisionLinePageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PriceRevisionLinePageDto>> GetLines(
        Guid priceRevisionId,
        [FromQuery] PriceRevisionLineFilterDto filter,
        CancellationToken cancellationToken)
    {
        var page = await _service.GetPriceRevisionLinesAsync(priceRevisionId, filter, cancellationToken);
        return Ok(page);
    }

    /// <summary>Bir satırı kapsam dışına alır ya da önizlenen değerini elle düzeltir.</summary>
    [HttpPatch("{priceRevisionId:guid}/lines/{lineId:guid}")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLine(
        Guid priceRevisionId,
        Guid lineId,
        [FromBody] UpdatePriceRevisionLineRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdatePriceRevisionLineAsync(priceRevisionId, lineId, request, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    // ─── Onay akışı ──────────────────────────────────────────────────────────────

    [HttpPost("{priceRevisionId:guid}/submit")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid priceRevisionId, CancellationToken cancellationToken)
    {
        var submitted = await _service.SubmitPriceRevisionAsync(priceRevisionId, _currentUserService.UserId, cancellationToken);
        return submitted ? NoContent() : NotFound();
    }

    [HttpPost("{priceRevisionId:guid}/approve")]
    [RequirePermission(Permissions.PriceRevisions.Approve)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid priceRevisionId,
        [FromBody] ApprovePriceRevisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var approved = await _service.ApprovePriceRevisionAsync(priceRevisionId, _currentUserService.UserId, request.Note, cancellationToken);
        return approved ? NoContent() : NotFound();
    }

    [HttpPost("{priceRevisionId:guid}/reject")]
    [RequirePermission(Permissions.PriceRevisions.Approve)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid priceRevisionId,
        [FromBody] RejectPriceRevisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var rejected = await _service.RejectPriceRevisionAsync(priceRevisionId, _currentUserService.UserId, request.Note, cancellationToken);
        return rejected ? NoContent() : NotFound();
    }

    [HttpPost("{priceRevisionId:guid}/cancel")]
    [RequirePermission(Permissions.PriceRevisions.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid priceRevisionId, CancellationToken cancellationToken)
    {
        var cancelled = await _service.CancelPriceRevisionAsync(priceRevisionId, _currentUserService.UserId, cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }

    // ─── Uygulama ────────────────────────────────────────────────────────────────

    [HttpPost("{priceRevisionId:guid}/apply")]
    [RequirePermission(Permissions.PriceRevisions.Apply)]
    [ProducesResponseType(typeof(PriceRevisionExecutionResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PriceRevisionExecutionResultDto>> Apply(
        Guid priceRevisionId,
        CancellationToken cancellationToken)
    {
        var result = await _service.ApplyPriceRevisionAsync(priceRevisionId, _currentUserService.UserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{priceRevisionId:guid}/rollback")]
    [RequirePermission(Permissions.PriceRevisions.Apply)]
    [ProducesResponseType(typeof(PriceRevisionExecutionResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PriceRevisionExecutionResultDto>> Rollback(
        Guid priceRevisionId,
        CancellationToken cancellationToken)
    {
        var result = await _service.RollbackPriceRevisionAsync(priceRevisionId, _currentUserService.UserId, cancellationToken);
        return Ok(result);
    }
}
