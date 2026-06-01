namespace ProductManager.Shared.Dtos.Orders;

public sealed record OrderPriceCalculationRequestDto(
    IReadOnlyList<OrderPriceCalculationItemRequestDto> Items);

public sealed record OrderPriceCalculationItemRequestDto(
    Guid ProductId,
    Guid? ProductPricingPlanId,
    int Quantity,
    bool IsTrial,
    IReadOnlyList<OrderPriceFeatureInputDto>? Features);

public sealed record OrderPriceFeatureInputDto(
    Guid? UnitDefinitionId,
    string? FeatureName,
    string? Value);

public sealed record OrderPriceCalculationResultDto(
    IReadOnlyList<OrderPriceCalculationItemResultDto> Items,
    decimal Subtotal,
    decimal TaxRate,
    decimal TaxAmount,
    decimal TotalAmount,
    string CurrencyCode);

public sealed record OrderPriceCalculationItemResultDto(
    Guid ProductId,
    string ProductName,
    string? PlanName,
    int Quantity,
    decimal BasePrice,
    decimal SetupFee,
    int BillingPeriodMonths,
    IReadOnlyList<FeatureAdjustmentDto> FeatureAdjustments,
    decimal FeatureSubtotal,
    decimal LineSubtotal,
    decimal TaxAmount,
    decimal LineTotal,
    bool IsTrial,
    string CurrencyCode);

public sealed record FeatureAdjustmentDto(
    string FeatureName,
    string FeatureDisplayName,
    string Value,
    decimal Adjustment);
