using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Security;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public InventoryController(IProductOperationsService service)
    {
        _service = service;
    }

    [RequirePermission(Permissions.Inventory.View)]
    [HttpGet("inventories")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductInventoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductInventoryDto>>> GetInventories(
        [FromQuery] ProductInventoryFilterDto? filter,
        CancellationToken cancellationToken)
    {
        var effectiveFilter = filter ?? new ProductInventoryFilterDto();
        var inventories = await _service.GetProductInventoriesAsync(effectiveFilter, cancellationToken);
        return Ok(inventories);
    }

    [RequirePermission(Permissions.Inventory.View)]
    [HttpGet("inventories/{inventoryId:guid}")]
    [ProducesResponseType(typeof(ProductInventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductInventoryDto>> GetInventoryById(Guid inventoryId, CancellationToken cancellationToken)
    {
        var inventory = await _service.GetInventoryByIdAsync(inventoryId, cancellationToken);
        if (inventory is null)
        {
            return NotFound();
        }

        return Ok(inventory);
    }

    [RequirePermission(Permissions.Inventory.Manage)]
    [HttpPost("inventories")]
    [ProducesResponseType(typeof(ProductInventoryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductInventoryDto>> CreateInventory(
        [FromBody] CreateProductInventoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var inventory = await _service.CreateInventoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetInventoryById), new { inventoryId = inventory.Id }, inventory);
    }

    [RequirePermission(Permissions.Inventory.Manage)]
    [HttpPut("inventories/{inventoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInventory(
        Guid inventoryId,
        [FromBody] UpdateProductInventoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateInventoryAsync(inventoryId, request, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [RequirePermission(Permissions.Inventory.Manage)]
    [HttpDelete("inventories/{inventoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInventory(Guid inventoryId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteInventoryAsync(inventoryId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [RequirePermission(Permissions.Inventory.View)]
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

    [RequirePermission(Permissions.Inventory.Manage)]
    [HttpPost("transactions")]
    [ProducesResponseType(typeof(InventoryTransactionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryTransactionDto>> CreateTransaction(
        [FromBody] CreateInventoryTransactionRequestDto request,
        CancellationToken cancellationToken)
    {
        var transaction = await _service.CreateInventoryTransactionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTransactionById), new { transactionId = transaction.Id }, transaction);
    }

    [RequirePermission(Permissions.Inventory.View)]
    [HttpGet("transactions/{transactionId:guid}")]
    [ProducesResponseType(typeof(InventoryTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryTransactionDto>> GetTransactionById(Guid transactionId, CancellationToken cancellationToken)
    {
        var transaction = await _service.GetInventoryTransactionByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }

    [RequirePermission(Permissions.Inventory.View)]
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

    [RequirePermission(Permissions.Inventory.Manage)]
    [HttpPost("reservations")]
    [ProducesResponseType(typeof(InventoryReservationDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryReservationDto>> CreateReservation(
        [FromBody] CreateInventoryReservationRequestDto request,
        CancellationToken cancellationToken)
    {
        var reservation = await _service.CreateInventoryReservationAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetReservationById), new { reservationId = reservation.Id }, reservation);
    }

    [RequirePermission(Permissions.Inventory.View)]
    [HttpGet("reservations/{reservationId:guid}")]
    [ProducesResponseType(typeof(InventoryReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryReservationDto>> GetReservationById(Guid reservationId, CancellationToken cancellationToken)
    {
        var reservation = await _service.GetInventoryReservationByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return NotFound();
        }

        return Ok(reservation);
    }

    [RequirePermission(Permissions.Inventory.Manage)]
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

    [RequirePermission(Permissions.Inventory.Manage)]
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
