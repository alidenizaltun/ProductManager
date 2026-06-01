using ProductManager.Shared.Dtos.Orders;

namespace ProductManager.Service.Shared.Abstract;

public interface IOrderService
{
    Task<OrderPriceCalculationResultDto> CalculateOrderPriceAsync(
        OrderPriceCalculationRequestDto request,
        CancellationToken cancellationToken = default);
}
