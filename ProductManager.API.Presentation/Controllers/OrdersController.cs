using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.Orders;

namespace ProductManager.Presentation.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Ürün ve lisans parametrelerine göre temel fiyat hesaplar (bayi/kampanya indirimi uygulanmaz).
    /// </summary>
    [HttpPost("calculate-order-price")]
    [ProducesResponseType(typeof(OrderPriceCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderPriceCalculationResultDto>> CalculateOrderPrice(
        [FromBody] OrderPriceCalculationRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.CalculateOrderPriceAsync(request, cancellationToken);
        return Ok(result);
    }
}
