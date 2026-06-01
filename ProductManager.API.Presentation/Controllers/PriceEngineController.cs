using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.PriceEngine;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/price")]
public sealed class PriceEngineController : ControllerBase
{
    private readonly IPriceEngineService _priceEngineService;

    public PriceEngineController(IPriceEngineService priceEngineService)
    {
        _priceEngineService = priceEngineService;
    }

    [HttpPost("calculate")]
    [ProducesResponseType(typeof(ProductPriceCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductPriceCalculationResultDto>> CalculatePrice(
        Guid productId,
        [FromBody] CalculateProductPriceRequestDto? request,
        CancellationToken cancellationToken)
    {
        var result = await _priceEngineService.CalculateProductPriceAsync(
            productId,
            request ?? new CalculateProductPriceRequestDto(),
            cancellationToken);

        return Ok(result);
    }
}
