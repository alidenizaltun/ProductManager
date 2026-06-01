using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.Orders;
using ProductManager.Shared.Dtos.PriceEngine;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Exceptions;

namespace ProductManager.Service.Concrete;

public sealed class OrderService : IOrderService
{
    private const int ProductKindSoftware = 2;
    private const int LicenseModelTrial = 5;
    private const decimal DefaultTaxRatePercent = 18m;

    private readonly IProductOperationsRepository _repository;
    private readonly IPriceEngineService _priceEngine;

    public OrderService(
        IProductOperationsRepository repository,
        IPriceEngineService priceEngine)
    {
        _repository = repository;
        _priceEngine = priceEngine;
    }

    public async Task<OrderPriceCalculationResultDto> CalculateOrderPriceAsync(
        OrderPriceCalculationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var itemResults = new List<OrderPriceCalculationItemResultDto>();
        decimal orderSubtotal = 0;
        decimal orderTaxAmount = 0;
        decimal orderTotalAmount = 0;
        string? currencyCode = null;
        decimal? orderTaxRatePercent = null;

        foreach (var item in request.Items)
        {
            var product = await _repository.GetProductByIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException($"Product with id {item.ProductId} not found");

            var productDetail = await _repository.GetProductDetailByIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException($"Product with id {item.ProductId} not found");

            var offering = ResolveOffering(productDetail, item.ProductPricingPlanId);
            var taxRatePercent = ResolveTaxRatePercent(productDetail);
            orderTaxRatePercent ??= taxRatePercent;
            currencyCode ??= offering?.CurrencyCode ?? product.DefaultCurrencyCode;

            if (item.IsTrial || offering?.LicenseModel == LicenseModelTrial)
            {
                itemResults.Add(BuildTrialLine(item, product.Name, offering));
                continue;
            }

            var priceRequest = BuildPriceRequest(item, taxRatePercent);

            var priceResult = await _priceEngine.CalculateProductPriceAsync(
                item.ProductId,
                priceRequest,
                cancellationToken);

            var setupFee = 0m;
            var lineScale = productDetail.Kind == ProductKindSoftware ? item.Quantity : 1;
            var lineSubtotal = priceResult.SubtotalNet * lineScale + setupFee;
            var lineTax = priceResult.TaxAmount * lineScale;
            var lineTotal = priceResult.TotalGross * lineScale + setupFee;

            var featureAdjustments = BuildFeatureAdjustments(priceResult);
            var featureSubtotal = CalculateFeatureSubtotal(priceResult);

            itemResults.Add(new OrderPriceCalculationItemResultDto(
                ProductId: item.ProductId,
                ProductName: product.Name,
                PlanName: offering?.Name,
                Quantity: item.Quantity,
                BasePrice: ResolveBasePrice(priceResult, offering),
                SetupFee: setupFee,
                BillingPeriodMonths: ResolveBillingPeriodMonths(offering),
                FeatureAdjustments: featureAdjustments,
                FeatureSubtotal: featureSubtotal,
                LineSubtotal: lineSubtotal,
                TaxAmount: lineTax,
                LineTotal: lineTotal,
                IsTrial: false,
                CurrencyCode: offering?.CurrencyCode ?? product.DefaultCurrencyCode
            ));

            orderSubtotal += lineSubtotal;
            orderTaxAmount += lineTax;
            orderTotalAmount += lineTotal;
        }

        return new OrderPriceCalculationResultDto(
            Items: itemResults,
            Subtotal: orderSubtotal,
            TaxRate: orderTaxRatePercent ?? DefaultTaxRatePercent,
            TaxAmount: orderTaxAmount,
            TotalAmount: orderTotalAmount,
            CurrencyCode: currencyCode ?? "TRY"
        );
    }

    private static CalculateProductPriceRequestDto BuildPriceRequest(
        OrderPriceCalculationItemRequestDto item,
        decimal taxRatePercent)
    {
        var offeringUnits = item.Features?
            .Where(f => f.UnitDefinitionId.HasValue && f.UnitDefinitionId != Guid.Empty)
            .Select(f => new LicenseOfferingUnitInputDto
            {
                UnitDefinitionId = f.UnitDefinitionId!.Value,
                Value = int.TryParse(f.Value, out var parsed) ? Math.Max(1, parsed) : 1
            })
            .ToList();

        return new CalculateProductPriceRequestDto
        {
            LicenseOfferingId = item.ProductPricingPlanId,
            Quantity = Math.Max(1, item.Quantity),
            OfferingUnits = offeringUnits,
            TaxRateOverride = taxRatePercent
        };
    }

    private static ProductLicenseOfferingDto? ResolveOffering(
        ProductDetailDto product,
        Guid? productPricingPlanId)
    {
        if (!productPricingPlanId.HasValue)
        {
            return null;
        }

        return product.LicenseOfferings.FirstOrDefault(o => o.Id == productPricingPlanId.Value);
    }

    private static decimal ResolveTaxRatePercent(ProductDetailDto product)
        => product.TaxRate ?? DefaultTaxRatePercent;

    private static OrderPriceCalculationItemResultDto BuildTrialLine(
        OrderPriceCalculationItemRequestDto item,
        string productName,
        ProductLicenseOfferingDto? offering)
    {
        return new OrderPriceCalculationItemResultDto(
            ProductId: item.ProductId,
            ProductName: productName,
            PlanName: offering?.Name,
            Quantity: item.Quantity,
            BasePrice: 0,
            SetupFee: 0,
            BillingPeriodMonths: ResolveBillingPeriodMonths(offering),
            FeatureAdjustments: [],
            FeatureSubtotal: 0,
            LineSubtotal: 0,
            TaxAmount: 0,
            LineTotal: 0,
            IsTrial: true,
            CurrencyCode: offering?.CurrencyCode ?? "TRY");
    }

    private static int ResolveBillingPeriodMonths(ProductLicenseOfferingDto? offering)
    {
        if (offering?.BillingPeriodValue is > 0)
        {
            return offering.BillingPeriodValue.Value;
        }

        return 12;
    }

    private static decimal ResolveBasePrice(
        ProductPriceCalculationResultDto priceResult,
        ProductLicenseOfferingDto? offering)
        => offering?.BasePrice ?? priceResult.SubtotalNet;

    private static List<FeatureAdjustmentDto> BuildFeatureAdjustments(ProductPriceCalculationResultDto priceResult)
    {
        var adjustmentLineTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PriceCalculationLineTypes.PricingTier,
            PriceCalculationLineTypes.Module,
            PriceCalculationLineTypes.VariantSurcharge
        };

        return priceResult.Lines
            .Where(l => adjustmentLineTypes.Contains(l.LineType))
            .Select(l => new FeatureAdjustmentDto(
                FeatureName: l.LineType,
                FeatureDisplayName: l.Description,
                Value: l.Quantity.ToString(),
                Adjustment: l.Amount))
            .ToList();
    }

    private static decimal CalculateFeatureSubtotal(ProductPriceCalculationResultDto priceResult)
    {
        var adjustmentLineTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PriceCalculationLineTypes.PricingTier,
            PriceCalculationLineTypes.Module,
            PriceCalculationLineTypes.VariantSurcharge
        };

        return priceResult.Lines
            .Where(l => adjustmentLineTypes.Contains(l.LineType))
            .Sum(l => l.Amount);
    }
}
