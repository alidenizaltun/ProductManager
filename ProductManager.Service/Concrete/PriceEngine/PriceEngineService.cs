using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.PriceEngine;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Exceptions;

namespace ProductManager.Service.Concrete.PriceEngine
{
    public sealed class PriceEngineService : IPriceEngineService
    {
        private const int ProductKindBundle = 5;
        private const int MaxBundleDepth = 3;

        private readonly IProductOperationsRepository _repository;

        public PriceEngineService(IProductOperationsRepository repository)
        {
            _repository = repository;
        }

        public async Task<LicenseOfferingPricingParametersDto?> GetLicenseOfferingPricingParametersAsync(
            Guid productId,
            Guid licenseOfferingId,
            CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetProductDetailByIdAsync(productId, cancellationToken)
                ?? throw new NotFoundException("Product", productId);

            return PriceEngineCalculator.BuildPricingParameters(product, licenseOfferingId);
        }

        public async Task<ProductPriceCalculationResultDto> CalculateProductPriceAsync(
            Guid productId,
            CalculateProductPriceRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetProductDetailByIdAsync(productId, cancellationToken)
                ?? throw new NotFoundException("Product", productId);

            if (!product.IsActive || !product.IsSellable)
            {
                throw new ValidationException("product", "Ürün satışa uygun değil veya pasif durumda.");
            }

            if (product.Kind == ProductKindBundle)
            {
                var componentResults = await CalculateBundleComponentsAsync(product, request, depth: 0, cancellationToken);
                return PriceEngineCalculator.Calculate(product, request, componentResults);
            }

            return PriceEngineCalculator.Calculate(product, request);
        }

        private async Task<IReadOnlyList<ProductPriceCalculationResultDto>> CalculateBundleComponentsAsync(
            ProductDetailDto bundleProduct,
            CalculateProductPriceRequestDto request,
            int depth,
            CancellationToken cancellationToken)
        {
            if (depth >= MaxBundleDepth)
            {
                throw new ValidationException("request", "Paket iç içe geçme derinliği izin verilen sınırı aştı.");
            }

            var selectedOptional = request.SelectedBundleItemIds?.ToHashSet() ?? [];
            var itemsToCalculate = bundleProduct.BundleItems
                .Where(i => !i.IsOptional || selectedOptional.Contains(i.Id))
                .ToList();

            if (itemsToCalculate.Count == 0)
            {
                throw new ValidationException("selectedBundleItemIds",
                    "Paket için en az bir zorunlu bileşen veya seçili opsiyonel bileşen gerekir.");
            }

            var results = new List<ProductPriceCalculationResultDto>();
            foreach (var item in itemsToCalculate)
            {
                var childRequest = new CalculateProductPriceRequestDto
                {
                    Quantity = Math.Max(1, (int)Math.Ceiling(item.Quantity)),
                    ProductVariantId = item.ChildVariantId,
                    PriceListCode = request.PriceListCode,
                    SalesChannel = request.SalesChannel,
                    CustomerGroupCode = request.CustomerGroupCode,
                    TaxRateOverride = request.TaxRateOverride,
                    PricesIncludeTax = request.PricesIncludeTax
                };

                var childProduct = await _repository.GetProductDetailByIdAsync(item.ChildProductId, cancellationToken)
                    ?? throw new NotFoundException("Product", item.ChildProductId);

                ProductPriceCalculationResultDto childResult;
                if (childProduct.Kind == ProductKindBundle)
                {
                    var nested = await CalculateBundleComponentsAsync(childProduct, childRequest, depth + 1, cancellationToken);
                    childResult = PriceEngineCalculator.Calculate(childProduct, childRequest, nested);
                }
                else
                {
                    childResult = PriceEngineCalculator.Calculate(childProduct, childRequest);
                }

                var scaledQuantity = item.Quantity;
                if (scaledQuantity != 1)
                {
                    childResult = ScaleResult(childResult, scaledQuantity);
                }

                results.Add(childResult);
            }

            return results;
        }

        private static ProductPriceCalculationResultDto ScaleResult(
            ProductPriceCalculationResultDto result,
            decimal bundleItemQuantity)
        {
            var factor = bundleItemQuantity;
            return result with
            {
                Quantity = (int)Math.Ceiling(result.Quantity * factor),
                SubtotalNet = result.SubtotalNet * factor,
                DiscountAmount = result.DiscountAmount * factor,
                NetBeforeTax = result.NetBeforeTax * factor,
                TaxAmount = result.TaxAmount * factor,
                TotalGross = result.TotalGross * factor,
                Lines = result.Lines
                    .Where(l => l.LineType != PriceCalculationLineTypes.Tax)
                    .Select(l => l with
                    {
                        Quantity = l.Quantity * factor,
                        Amount = l.Amount * factor
                    })
                    .ToList()
            };
        }
    }
}
