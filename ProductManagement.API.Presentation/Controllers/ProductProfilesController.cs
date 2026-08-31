using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Security;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/profiles")]
public sealed class ProductProfilesController : ControllerBase
{
    private readonly IProductOperationsService _service;

    public ProductProfilesController(IProductOperationsService service)
    {
        _service = service;
    }

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("physical")]
    [ProducesResponseType(typeof(ProductPhysicalProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPhysicalProfileDto>> GetPhysicalProfile(Guid productId, CancellationToken cancellationToken)
    {
        var profile = await _service.GetPhysicalProfileAsync(productId, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpPut("physical")]
    [ProducesResponseType(typeof(ProductPhysicalProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductPhysicalProfileDto>> UpsertPhysicalProfile(
        Guid productId,
        [FromBody] UpsertProductPhysicalProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var profile = await _service.UpsertPhysicalProfileAsync(productId, request, cancellationToken);
        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpDelete("physical")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhysicalProfile(Guid productId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeletePhysicalProfileAsync(productId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("software")]
    [ProducesResponseType(typeof(ProductSoftwareProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductSoftwareProfileDto>> GetSoftwareProfile(Guid productId, CancellationToken cancellationToken)
    {
        var profile = await _service.GetSoftwareProfileAsync(productId, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpPut("software")]
    [ProducesResponseType(typeof(ProductSoftwareProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductSoftwareProfileDto>> UpsertSoftwareProfile(
        Guid productId,
        [FromBody] UpsertProductSoftwareProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var profile = await _service.UpsertSoftwareProfileAsync(productId, request, cancellationToken);
        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpDelete("software")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSoftwareProfile(Guid productId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteSoftwareProfileAsync(productId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("service")]
    [ProducesResponseType(typeof(ProductServiceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductServiceProfileDto>> GetServiceProfile(Guid productId, CancellationToken cancellationToken)
    {
        var profile = await _service.GetServiceProfileAsync(productId, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpPut("service")]
    [ProducesResponseType(typeof(ProductServiceProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductServiceProfileDto>> UpsertServiceProfile(
        Guid productId,
        [FromBody] UpsertProductServiceProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var profile = await _service.UpsertServiceProfileAsync(productId, request, cancellationToken);
        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpDelete("service")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteServiceProfile(Guid productId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteServiceProfileAsync(productId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [RequirePermission(Permissions.Products.View)]
    [HttpGet("subscription")]
    [ProducesResponseType(typeof(ProductSubscriptionProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductSubscriptionProfileDto>> GetSubscriptionProfile(Guid productId, CancellationToken cancellationToken)
    {
        var profile = await _service.GetSubscriptionProfileAsync(productId, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpPut("subscription")]
    [ProducesResponseType(typeof(ProductSubscriptionProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductSubscriptionProfileDto>> UpsertSubscriptionProfile(
        Guid productId,
        [FromBody] UpsertProductSubscriptionProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var profile = await _service.UpsertSubscriptionProfileAsync(productId, request, cancellationToken);
        return Ok(profile);
    }

    [RequirePermission(Permissions.Products.Manage)]
    [HttpDelete("subscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubscriptionProfile(Guid productId, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteSubscriptionProfileAsync(productId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
