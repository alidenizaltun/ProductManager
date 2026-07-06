using ProductManager.Shared.Dtos.PriceEngine;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Exceptions;
using System.Text.Json;

namespace ProductManager.Service.Concrete.PriceEngine
{
    internal static class PriceEngineCalculator
    {
        private const decimal DefaultTaxRatePercent = 20m;
        private const int ProductKindSoftware = 2;
        private const int ProductKindBundle = 5;
        private const int PriceTypeSale = 1;
        private const int LicenseModelTrial = 5;
        private const int LicenseModelSeatBased = 4;
        private const int LicenseModelUsageBased = 3;

        private sealed record OfferingUnitEntry(
            Guid? ProductUnitId,
            Guid UnitDefinitionId,
            int Value,
            string DisplayLabel,
            string? UnitDefinitionCode,
            string? UnitDefinitionName);

        public static LicenseOfferingPricingParametersDto? BuildPricingParameters(
            ProductDetailDto product,
            Guid licenseOfferingId)
        {
            var offering = product.LicenseOfferings.FirstOrDefault(o => o.Id == licenseOfferingId);
            if (offering is null)
            {
                return null;
            }

            var unitParameters = BuildUnitParameters(product, offering);
            var ruleParameters = BuildRuleParameters(product, offering.Id);
            return new LicenseOfferingPricingParametersDto
            {
                ProductId = product.Id,
                LicenseOfferingId = offering.Id,
                LicenseOfferingName = offering.Name,
                LicenseModel = offering.LicenseModel,
                RequiresUnitInput = unitParameters.Count > 0 || ruleParameters.Count > 0,
                UnitParameters = unitParameters,
                RuleParameters = ruleParameters
            };
        }

        public static ProductPriceCalculationResultDto Calculate(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            IReadOnlyList<ProductPriceCalculationResultDto>? bundleComponentResults = null)
        {
            var quantity = Math.Max(1, request.Quantity);
            var lines = new List<PriceCalculationLineDto>();
            decimal subtotalNet;
            IReadOnlyList<AppliedLicenseOfferingUnitDto> appliedOfferingUnits = [];

            if (product.Kind == ProductKindBundle)
            {
                subtotalNet = CalculateBundleSubtotal(bundleComponentResults, lines);
            }
            else if (product.Kind == ProductKindSoftware)
            {
                (subtotalNet, appliedOfferingUnits) = CalculateSoftwareSubtotal(product, request, lines);
                quantity = 1;
            }
            else
            {
                subtotalNet = CalculateStandardSubtotal(product, request, quantity, lines);
            }

            subtotalNet = PricingRuleAdjustmentEvaluator.ApplyPricingRules(product, request, subtotalNet, lines);

            var (discountAmount, discountLines) = ApplyDiscount(subtotalNet, request);
            lines.AddRange(discountLines);

            var netBeforeTax = Math.Max(0, subtotalNet - discountAmount);
            var taxRate = ResolveTaxRate(product, request);
            var (taxAmount, totalGross, netStored) = ApplyTax(netBeforeTax, taxRate, request.PricesIncludeTax);
            lines.Add(new PriceCalculationLineDto
            {
                LineType = PriceCalculationLineTypes.Tax,
                Description = $"KDV (%{taxRate:0.##})",
                Quantity = 1,
                UnitAmount = taxAmount,
                Amount = taxAmount
            });

            var offering = product.Kind == ProductKindSoftware
                ? ResolveLicenseOffering(product, request, required: true)
                : ResolveLicenseOffering(product, request, required: false);

            return new ProductPriceCalculationResultDto
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.Name,
                ProductKind = product.Kind,
                CurrencyCode = product.DefaultCurrencyCode,
                Quantity = quantity,
                OfferingUnits = appliedOfferingUnits,
                LicenseOfferingId = offering?.Id,
                LicenseOfferingName = offering?.Name,
                LicenseModel = offering?.LicenseModel,
                SubtotalNet = Round(subtotalNet),
                DiscountAmount = Round(discountAmount),
                NetBeforeTax = Round(netStored),
                TaxRate = taxRate,
                TaxCode = product.TaxCode,
                TaxAmount = Round(taxAmount),
                TotalGross = Round(totalGross),
                CompareAtTotalGross = null,
                PricesIncludeTax = request.PricesIncludeTax,
                Lines = lines,
                CalculatedAt = DateTime.UtcNow
            };
        }

        private static decimal CalculateBundleSubtotal(
            IReadOnlyList<ProductPriceCalculationResultDto>? componentResults,
            List<PriceCalculationLineDto> lines)
        {
            if (componentResults is null || componentResults.Count == 0)
            {
                throw new ValidationException("request", "Paket ürün fiyatı için en az bir zorunlu bileşen hesaplanmalıdır.");
            }

            decimal total = 0;
            foreach (var component in componentResults)
            {
                total += component.NetBeforeTax;
                lines.Add(new PriceCalculationLineDto
                {
                    LineType = PriceCalculationLineTypes.BundleComponent,
                    Description = $"{component.ProductName} ({component.ProductCode})",
                    Quantity = component.Quantity,
                    UnitAmount = component.NetBeforeTax / Math.Max(1, component.Quantity),
                    Amount = component.NetBeforeTax,
                    ReferenceId = component.ProductId.ToString()
                });
            }

            return total;
        }

        private static (decimal Subtotal, IReadOnlyList<AppliedLicenseOfferingUnitDto> AppliedUnits) CalculateSoftwareSubtotal(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            List<PriceCalculationLineDto> lines)
        {
            var offering = ResolveLicenseOffering(product, request, required: true)!;
            ValidateOfferingValidity(offering);

            if (offering.LicenseModel == LicenseModelTrial)
            {
                lines.Add(new PriceCalculationLineDto
                {
                    LineType = PriceCalculationLineTypes.LicenseBase,
                    Description = $"{offering.Name} (Deneme)",
                    Quantity = 1,
                    UnitAmount = 0,
                    Amount = 0,
                    ReferenceId = offering.Id.ToString()
                });
                return (AddModuleLines(product, request, lines), []);
            }

            decimal subtotal = 0;
            var appliedUnits = new List<AppliedLicenseOfferingUnitDto>();
            var unitParameters = BuildUnitParameters(product, offering);
            var unitInputs = ResolveOfferingUnitInputs(offering, unitParameters, request);

            if (offering.BasePrice > 0
                && offering.LicenseModel is not (LicenseModelSeatBased or LicenseModelUsageBased))
            {
                lines.Add(new PriceCalculationLineDto
                {
                    LineType = PriceCalculationLineTypes.LicenseBase,
                    Description = offering.Name,
                    Quantity = 1,
                    UnitAmount = offering.BasePrice,
                    Amount = offering.BasePrice,
                    ReferenceId = offering.Id.ToString()
                });
                subtotal += offering.BasePrice;
            }

            foreach (var unitInput in unitInputs)
            {
                if (offering.LicenseModel is LicenseModelSeatBased or LicenseModelUsageBased
                    && offering.BasePrice > 0)
                {
                    var unitAmount = offering.BasePrice * unitInput.Value;
                    lines.Add(new PriceCalculationLineDto
                    {
                        LineType = PriceCalculationLineTypes.LicenseBase,
                        Description = $"{offering.Name} ({unitInput.Value} {unitInput.DisplayLabel})",
                        Quantity = unitInput.Value,
                        UnitAmount = offering.BasePrice,
                        Amount = unitAmount,
                        ReferenceId = offering.Id.ToString(),
                        Metadata = $"productUnitId={unitInput.ProductUnitId};unitDefinitionId={unitInput.UnitDefinitionId}"
                    });
                    subtotal += unitAmount;
                    appliedUnits.Add(new AppliedLicenseOfferingUnitDto
                    {
                        UnitDefinitionId = unitInput.UnitDefinitionId,
                        UnitDefinitionCode = unitInput.UnitDefinitionCode,
                        UnitDefinitionName = unitInput.UnitDefinitionName,
                        DisplayLabel = unitInput.DisplayLabel,
                        Value = unitInput.Value,
                        Amount = Round(unitAmount)
                    });
                    continue;
                }

                throw new ValidationException(
                    "offeringUnits",
                    $"\"{unitInput.DisplayLabel}\" için {unitInput.Value} değeri ile eşleşen aktif fiyat kademesi bulunamadı.");
            }

            if (unitParameters.Count > 0 && unitInputs.Count == 0)
            {
                var labels = string.Join(", ", unitParameters.Select(p => p.DisplayLabel));
                throw new ValidationException(
                    "offeringUnits",
                    $"Bu lisans teklifi için şu birim parametreleri zorunludur: {labels}.");
            }

            subtotal += AddModuleLines(product, request, lines);
            return (subtotal, appliedUnits);
        }

        private static IReadOnlyList<LicenseOfferingUnitParameterDto> BuildUnitParameters(
            ProductDetailDto product,
            ProductLicenseOfferingDto offering)
        {
            if (offering.LicenseModel is LicenseModelSeatBased or LicenseModelUsageBased)
            {
                var assignedUnits = offering.ProductUnits.Count > 0
                    ? offering.ProductUnits.Where(unit => unit.IsActive)
                    : product.ProductUnits.Where(unit =>
                        unit.IsActive
                        && unit.IsDefault);

                var parameters = assignedUnits
                    .GroupBy(unit => unit.UnitDefinitionId)
                    .Select(group =>
                    {
                        var productUnit = group
                            .OrderBy(unit => unit.SortOrder)
                            .ThenBy(unit => unit.Name)
                            .First();
                        var displayLabel = productUnit.Name;
                        return new LicenseOfferingUnitParameterDto
                        {
                            ProductUnitId = productUnit.Id,
                            UnitDefinitionId = productUnit.UnitDefinitionId,
                            UnitDefinitionCode = productUnit.UnitDefinitionCode ?? string.Empty,
                            UnitDefinitionName = productUnit.UnitDefinitionName ?? string.Empty,
                            DisplayLabel = displayLabel,
                            HelpText = $"Birim miktarı \"{displayLabel}\" alanından girilir; fiyat birim başına hesaplanır.",
                            IsRequired = true,
                            MinValue = 1,
                            MaxValue = offering.MaxSeats
                        };
                    })
                    .ToList();

                if (parameters.Count > 0)
                {
                    return parameters;
                }

                throw new ValidationException(
                    "licenseOfferingId",
                    "Seat/kullanım teklifi için en az bir aktif ProductUnit tanımlanmalıdır.");
            }

            return [];
        }

        private static IReadOnlyList<PricingRuleParameterDto> BuildRuleParameters(
            ProductDetailDto product,
            Guid licenseOfferingId)
        {
            var now = DateTime.UtcNow;
            var parameters = new Dictionary<string, PricingRuleParameterDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in product.PricingRules.Where(rule =>
                rule.IsActive
                && (rule.ProductLicenseOfferingId is null || rule.ProductLicenseOfferingId == licenseOfferingId)
                && (rule.ValidFrom is null || rule.ValidFrom <= now)
                && (rule.ValidTo is null || rule.ValidTo >= now)))
            {
                if (!TryBuildRuleParameter(rule, out var parameter))
                {
                    continue;
                }

                parameters.TryAdd(parameter.Field, parameter);
            }

            return parameters.Values
                .OrderBy(parameter => parameter.DisplayLabel)
                .ToList();
        }

        private static bool TryBuildRuleParameter(ProductPricingRuleDto rule, out PricingRuleParameterDto parameter)
        {
            parameter = new PricingRuleParameterDto();
            if (string.IsNullOrWhiteSpace(rule.PriceAdjustmentJson))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(rule.PriceAdjustmentJson);
                var root = document.RootElement;
                if (!root.TryGetProperty("unit", out var unit)
                    || unit.ValueKind != JsonValueKind.Object
                    || !unit.TryGetProperty("field", out var fieldElement))
                {
                    return false;
                }

                var field = fieldElement.GetString();
                if (string.IsNullOrWhiteSpace(field))
                {
                    return false;
                }

                var displayLabel = field.StartsWith("feature.", StringComparison.OrdinalIgnoreCase)
                    ? field["feature.".Length..]
                    : field;

                parameter = new PricingRuleParameterDto
                {
                    Field = field,
                    DisplayLabel = displayLabel,
                    IsRequired = true,
                    MinValue = ResolveRuleMinValue(root),
                    MaxValue = ResolveRuleMaxValue(root),
                    Rounding = unit.TryGetProperty("rounding", out var rounding) ? rounding.GetString() : null
                };
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static decimal ResolveRuleMinValue(JsonElement adjustment)
        {
            if (adjustment.TryGetProperty("tiers", out var tiers) && tiers.ValueKind == JsonValueKind.Array)
            {
                var tierMinimums = tiers.EnumerateArray()
                    .Select(tier => tier.TryGetProperty("from", out var from) && from.TryGetDecimal(out var value) ? value : (decimal?)null)
                    .Where(value => value.HasValue)
                    .Select(value => value!.Value)
                    .ToList();

                if (tierMinimums.Count > 0)
                {
                    return tierMinimums.Min();
                }
            }

            if (adjustment.TryGetProperty("unit", out var unit)
                && unit.TryGetProperty("freeUnits", out var freeUnits)
                && freeUnits.TryGetDecimal(out var freeUnitValue))
            {
                return Math.Max(0, freeUnitValue);
            }

            return 0;
        }

        private static decimal? ResolveRuleMaxValue(JsonElement adjustment)
        {
            if (!adjustment.TryGetProperty("tiers", out var tiers) || tiers.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            decimal? maxValue = null;
            foreach (var tier in tiers.EnumerateArray())
            {
                if (!tier.TryGetProperty("to", out var to))
                {
                    continue;
                }

                if (to.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                {
                    return null;
                }

                if (to.TryGetDecimal(out var value))
                {
                    maxValue = !maxValue.HasValue || value > maxValue.Value ? value : maxValue;
                }
            }

            return maxValue;
        }

        private static IReadOnlyList<OfferingUnitEntry> ResolveOfferingUnitInputs(
            ProductLicenseOfferingDto offering,
            IReadOnlyList<LicenseOfferingUnitParameterDto> unitParameters,
            CalculateProductPriceRequestDto request)
        {
            if (unitParameters.Count == 0)
            {
                if (request.OfferingUnits is { Count: > 0 })
                {
                    throw new ValidationException(
                        "offeringUnits",
                        "Bu lisans teklifinde birim parametresi tanımlı değil; offeringUnits gönderilmemelidir.");
                }

                return [];
            }

            if (request.OfferingUnits is null or { Count: 0 })
            {
                var labels = string.Join(", ", unitParameters.Select(p => p.DisplayLabel));
                throw new ValidationException(
                    "offeringUnits",
                    $"Lisans teklifi için zorunlu birim parametreleri eksik: {labels}.");
            }

            var parameterById = unitParameters.ToDictionary(p => p.UnitDefinitionId);
            var requiredIds = parameterById.Keys.ToHashSet();
            var seen = new HashSet<Guid>();
            var result = new List<OfferingUnitEntry>();

            foreach (var input in request.OfferingUnits)
            {
                if (!seen.Add(input.UnitDefinitionId))
                {
                    throw new ValidationException(
                        "offeringUnits",
                        $"Aynı birim parametresi tekrar gönderilemez: {input.UnitDefinitionId}.");
                }

                if (!parameterById.TryGetValue(input.UnitDefinitionId, out var parameter))
                {
                    throw new ValidationException(
                        "offeringUnits",
                        $"Gönderilen birim parametresi '{input.UnitDefinitionId}' bu lisans teklifine ait değil.");
                }

                if (input.Value < parameter.MinValue)
                {
                    throw new ValidationException(
                        "offeringUnits",
                        $"\"{parameter.DisplayLabel}\" değeri en az {parameter.MinValue} olmalıdır.");
                }

                if (parameter.MaxValue.HasValue && input.Value > parameter.MaxValue.Value)
                {
                    throw new ValidationException(
                        "offeringUnits",
                        $"\"{parameter.DisplayLabel}\" değeri en fazla {parameter.MaxValue.Value} olabilir.");
                }

                result.Add(new OfferingUnitEntry(
                    parameter.ProductUnitId,
                    input.UnitDefinitionId,
                    input.Value,
                    parameter.DisplayLabel,
                    parameter.UnitDefinitionCode,
                    parameter.UnitDefinitionName));
            }

            var missing = requiredIds.Except(seen).ToList();
            if (missing.Count > 0)
            {
                var missingLabels = missing
                    .Select(id => parameterById[id].DisplayLabel);
                throw new ValidationException(
                    "offeringUnits",
                    $"Eksik birim parametreleri: {string.Join(", ", missingLabels)}.");
            }

            return result;
        }

        private static decimal CalculateStandardSubtotal(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            int quantity,
            List<PriceCalculationLineDto> lines)
        {
            var (unitPrice, lineType, description, referenceId) = ResolveUnitPrice(product, request);
            var variantSurcharge = ResolveVariantSurcharge(product, request);

            if (variantSurcharge > 0)
            {
                lines.Add(new PriceCalculationLineDto
                {
                    LineType = PriceCalculationLineTypes.VariantSurcharge,
                    Description = "Varyant ek fiyatı",
                    Quantity = quantity,
                    UnitAmount = variantSurcharge,
                    Amount = variantSurcharge * quantity,
                    ReferenceId = request.ProductVariantId?.ToString()
                });
            }

            var lineAmount = unitPrice * quantity;
            lines.Add(new PriceCalculationLineDto
            {
                LineType = lineType,
                Description = description,
                Quantity = quantity,
                UnitAmount = unitPrice,
                Amount = lineAmount,
                ReferenceId = referenceId
            });

            return lineAmount + (variantSurcharge * quantity);
        }

        private static decimal AddModuleLines(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            List<PriceCalculationLineDto> lines)
        {
            if (request.SelectedModuleIds is null || request.SelectedModuleIds.Count == 0)
            {
                return 0;
            }

            decimal moduleTotal = 0;
            var moduleIds = request.SelectedModuleIds.ToHashSet();

            var offeringPriceLookup = product.Modules
                .SelectMany(m => m.OfferingPrices)
                .Where(p => p.IsActive && request.LicenseOfferingId.HasValue && p.ProductLicenseOfferingId == request.LicenseOfferingId.Value)
                .ToDictionary(p => p.ProductModuleId, p => p.Price);

            foreach (var module in product.Modules.Where(m => m.IsActive && moduleIds.Contains(m.Id)))
            {
                var modulePrice = offeringPriceLookup.TryGetValue(module.Id, out var price) ? price : 0;
                lines.Add(new PriceCalculationLineDto
                {
                    LineType = PriceCalculationLineTypes.Module,
                    Description = module.Name,
                    Quantity = 1,
                    UnitAmount = modulePrice,
                    Amount = modulePrice,
                    ReferenceId = module.Id.ToString(),
                    Metadata = module.ModuleCode
                });
                moduleTotal += modulePrice;
            }

            var missing = moduleIds.Except(product.Modules.Select(m => m.Id)).ToList();
            if (missing.Count > 0)
            {
                throw new ValidationException("selectedModuleIds", "Seçilen modüllerden biri veya birkaçı bu ürüne ait değil.");
            }

            return moduleTotal;
        }

        private static (decimal Amount, string LineType, string Description, string? ReferenceId) ResolveUnitPrice(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request)
        {
            var priceListItem = FindPriceListItem(product, request);
            if (priceListItem is not null)
            {
                return (
                    priceListItem.Amount,
                    PriceCalculationLineTypes.PriceList,
                    $"Fiyat listesi: {priceListItem.PriceListName ?? priceListItem.PriceListCode}",
                    priceListItem.Id.ToString());
            }

            var price = FindProductPrice(product, request)
                ?? throw new ValidationException("price", "Ürün için geçerli bir satış fiyatı bulunamadı.");

            return (
                price.Amount,
                PriceCalculationLineTypes.UnitPrice,
                "Birim fiyat",
                price.Id.ToString());
        }

        private static ProductPriceListItemDto? FindPriceListItem(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PriceListCode))
            {
                return null;
            }

            var quantity = Math.Max(1, request.Quantity);
            var code = request.PriceListCode.Trim();

            return product.PriceListItems
                .Where(i =>
                    string.Equals(i.PriceListCode, code, StringComparison.OrdinalIgnoreCase)
                    && i.ProductId == product.Id
                    && (request.ProductVariantId is null || i.ProductVariantId == request.ProductVariantId)
                    && (i.MinQuantity is null || quantity >= i.MinQuantity)
                    && (i.MaxQuantity is null || quantity <= i.MaxQuantity))
                .OrderByDescending(i => i.MinQuantity ?? 0)
                .FirstOrDefault();
        }

        private static ProductPriceDto? FindProductPrice(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request)
        {
            var now = DateTime.UtcNow;
            var priceType = request.PriceType ?? PriceTypeSale;
            var quantity = Math.Max(1, request.Quantity);

            return product.Prices
                .Where(p =>
                    p.PriceType == priceType
                    && (request.ProductVariantId is null
                        ? p.ProductVariantId is null
                        : p.ProductVariantId == request.ProductVariantId)
                    && (string.IsNullOrWhiteSpace(request.SalesChannel)
                        || string.Equals(p.SalesChannel, request.SalesChannel, StringComparison.OrdinalIgnoreCase))
                    && (string.IsNullOrWhiteSpace(request.CustomerGroupCode)
                        || string.Equals(p.CustomerGroupCode, request.CustomerGroupCode, StringComparison.OrdinalIgnoreCase))
                    && (p.ValidFrom is null || p.ValidFrom <= now)
                    && (p.ValidTo is null || p.ValidTo >= now)
                    && (p.MinQuantity is null || quantity >= p.MinQuantity)
                    && (p.MaxQuantity is null || quantity <= p.MaxQuantity))
                .OrderByDescending(p => p.MinQuantity ?? 0)
                .ThenByDescending(p => !string.IsNullOrWhiteSpace(p.SalesChannel))
                .FirstOrDefault();
        }

        private static decimal ResolveVariantSurcharge(ProductDetailDto product, CalculateProductPriceRequestDto request)
        {
            if (request.ProductVariantId is null)
            {
                return 0;
            }

            var variant = product.Variants.FirstOrDefault(v => v.Id == request.ProductVariantId && v.IsActive)
                ?? throw new ValidationException("productVariantId", "Geçerli bir ürün varyantı bulunamadı.");

            return variant.AdditionalPrice ?? 0;
        }

        private static ProductLicenseOfferingDto? ResolveLicenseOffering(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            bool required)
        {
            if (!request.LicenseOfferingId.HasValue)
            {
                if (required)
                {
                    throw new ValidationException("licenseOfferingId", "Yazılım fiyatı için lisans teklifi seçilmelidir.");
                }

                return null;
            }

            var now = DateTime.UtcNow;
            var offering = product.LicenseOfferings.FirstOrDefault(o =>
                o.Id == request.LicenseOfferingId.Value
                && o.IsActive
                && (o.ValidFrom is null || o.ValidFrom <= now)
                && (o.ValidTo is null || o.ValidTo >= now));

            if (offering is null && required)
            {
                throw new ValidationException("licenseOfferingId", "Geçerli bir lisans teklifi bulunamadı.");
            }

            return offering;
        }

        private static void ValidateOfferingValidity(ProductLicenseOfferingDto offering)
        {
            var now = DateTime.UtcNow;
            if (!offering.IsActive)
            {
                throw new ValidationException("licenseOfferingId", "Seçilen lisans teklifi aktif değil.");
            }

            if (offering.ValidFrom.HasValue && offering.ValidFrom > now)
            {
                throw new ValidationException("licenseOfferingId", "Seçilen lisans teklifi henüz yürürlükte değil.");
            }

            if (offering.ValidTo.HasValue && offering.ValidTo < now)
            {
                throw new ValidationException("licenseOfferingId", "Seçilen lisans teklifinin süresi dolmuş.");
            }
        }

        private static (decimal DiscountAmount, List<PriceCalculationLineDto> Lines) ApplyDiscount(
            decimal subtotalNet,
            CalculateProductPriceRequestDto request)
        {
            var lines = new List<PriceCalculationLineDto>();
            decimal discount = 0;

            if (request.DiscountAmount.HasValue && request.DiscountAmount.Value > 0)
            {
                discount = Math.Min(subtotalNet, request.DiscountAmount.Value);
            }
            else if (request.DiscountPercent.HasValue && request.DiscountPercent.Value > 0)
            {
                discount = subtotalNet * (request.DiscountPercent.Value / 100m);
            }

            if (discount > 0)
            {
                lines.Add(new PriceCalculationLineDto
                {
                    LineType = PriceCalculationLineTypes.Discount,
                    Description = request.DiscountAmount.HasValue ? "Sabit indirim" : "Yüzde indirim",
                    Quantity = 1,
                    UnitAmount = -discount,
                    Amount = -discount
                });
            }

            return (discount, lines);
        }

        private static decimal ResolveTaxRate(ProductDetailDto product, CalculateProductPriceRequestDto request)
            => request.TaxRateOverride ?? product.TaxRate ?? DefaultTaxRatePercent;

        private static (decimal TaxAmount, decimal TotalGross, decimal NetBeforeTax) ApplyTax(
            decimal netBeforeTax,
            decimal taxRatePercent,
            bool pricesIncludeTax)
        {
            if (taxRatePercent <= 0)
            {
                return (0, netBeforeTax, netBeforeTax);
            }

            var rate = taxRatePercent / 100m;
            if (pricesIncludeTax)
            {
                var net = netBeforeTax / (1 + rate);
                var tax = netBeforeTax - net;
                return (tax, netBeforeTax, net);
            }

            var taxAmount = netBeforeTax * rate;
            return (taxAmount, netBeforeTax + taxAmount, netBeforeTax);
        }

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
