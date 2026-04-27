using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public InventoryController(IProductOperationsService service)
    {
        _service = service;
    }

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InventoryTransactionDto>>> GetTransactions(
        [FromQuery] InventoryTransactionFilterDto? filter,
        CancellationToken cancellationToken)
    {
        var effectiveFilter = filter ?? new InventoryTransactionFilterDto();
        var transactions = await _service.GetInventoryTransactionsAsync(effectiveFilter, cancellationToken);
        return Ok(transactions);
    }

    [HttpPost("transactions")]
    [ProducesResponseType(typeof(InventoryTransactionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryTransactionDto>> CreateTransaction(
        [FromBody] CreateInventoryTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        var transaction = await _service.CreateInventoryTransactionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTransactions), transaction);
    }

    [HttpGet("reservations")]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InventoryReservationDto>>> GetReservations(
        [FromQuery] InventoryReservationFilterDto? filter,
        CancellationToken cancellationToken)
    {
        var effectiveFilter = filter ?? new InventoryReservationFilterDto();
        var reservations = await _service.GetInventoryReservationsAsync(effectiveFilter, cancellationToken);
        return Ok(reservations);
    }

    [HttpPost("reservations")]
    [ProducesResponseType(typeof(InventoryReservationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryReservationDto>> CreateReservation(
        [FromBody] CreateInventoryReservationRequestDto request,
        CancellationToken cancellationToken)
    {
        var reservation = await _service.CreateInventoryReservationAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetReservations), reservation);
    }

    [HttpPatch("reservations/{reservationId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReservationStatus(
        Guid reservationId,
        [FromBody] UpdateInventoryReservationStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateInventoryReservationStatusAsync(reservationId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("reservations/{reservationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReservation(Guid reservationId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteInventoryReservationAsync(reservationId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
