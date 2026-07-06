using System.Globalization;
using System.Text.Json;
using ProductManager.Shared.Dtos.PriceEngine;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Exceptions;

namespace ProductManager.Service.Concrete.PriceEngine
{
    internal static class PricingRuleAdjustmentEvaluator
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static decimal ApplyPricingRules(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            decimal subtotalNet,
            List<PriceCalculationLineDto> lines)
        {
            var now = DateTime.UtcNow;
            var currentPrice = subtotalNet;
            var previousResult = 0m;

            foreach (var rule in product.PricingRules
                .Where(rule => RuleScopeMatches(product, rule, request, now))
                .OrderBy(rule => rule.Priority)
                .ThenBy(rule => rule.CreatedAt))
            {
                var adjustment = ParseAdjustment(rule);
                if (!ConditionsMatch(product, request, rule.ConditionsJson, adjustment.Conditions))
                {
                    continue;
                }

                var amount = CalculateAdjustment(product, request, adjustment, subtotalNet, currentPrice, previousResult);
                amount = ApplyLimits(amount, currentPrice, adjustment.Limits);
                if (amount == 0)
                {
                    previousResult = amount;
                    continue;
                }

                var quantity = ResolveUnitCount(product, request, adjustment);
                lines.Add(new PriceCalculationLineDto
                {
                    LineType = PriceCalculationLineTypes.PricingRule,
                    Description = rule.Name,
                    Quantity = quantity,
                    UnitAmount = quantity > 0 ? amount / quantity : amount,
                    Amount = amount,
                    ReferenceId = rule.Id.ToString(),
                    Metadata = $"code={rule.Code};type={ResolveType(adjustment)};applyOn={ResolveApplyOn(adjustment)}"
                });

                currentPrice += amount;
                previousResult = amount;
            }

            return Math.Max(0, currentPrice);
        }

        private static bool RuleScopeMatches(
            ProductDetailDto product,
            ProductPricingRuleDto rule,
            CalculateProductPriceRequestDto request,
            DateTime now)
        {
            if (!rule.IsActive)
            {
                return false;
            }

            if (rule.ValidFrom.HasValue && rule.ValidFrom.Value > now)
            {
                return false;
            }

            if (rule.ValidTo.HasValue && rule.ValidTo.Value < now)
            {
                return false;
            }

            if (rule.ProductVariantId.HasValue && rule.ProductVariantId != request.ProductVariantId)
            {
                return false;
            }

            if (rule.ProductLicenseOfferingId.HasValue && rule.ProductLicenseOfferingId != request.LicenseOfferingId)
            {
                return false;
            }

            if (!RuleProductUnitsMatch(product, rule, request))
            {
                return false;
            }

            if (!TextMatches(rule.SalesChannel, request.SalesChannel))
            {
                return false;
            }

            return TextMatches(rule.CustomerGroupCode, request.CustomerGroupCode);
        }

        private static bool RuleProductUnitsMatch(
            ProductDetailDto product,
            ProductPricingRuleDto rule,
            CalculateProductPriceRequestDto request)
        {
            IReadOnlyList<ProductUnitDto> assignedUnits = rule.ProductUnits;

            if (assignedUnits.Count == 0)
            {
                return true;
            }

            if (request.OfferingUnits is null || request.OfferingUnits.Count == 0)
            {
                return false;
            }

            var assignedUnitDefinitionIds = assignedUnits
                .Select(unit => unit.UnitDefinitionId)
                .ToHashSet();

            return request.OfferingUnits.Any(input => assignedUnitDefinitionIds.Contains(input.UnitDefinitionId));
        }

        private static bool TextMatches(string? ruleValue, string? requestValue)
            => string.IsNullOrWhiteSpace(ruleValue)
                || string.Equals(ruleValue, requestValue, StringComparison.OrdinalIgnoreCase);

        private static PriceAdjustmentDefinition ParseAdjustment(ProductPricingRuleDto rule)
        {
            if (string.IsNullOrWhiteSpace(rule.PriceAdjustmentJson))
            {
                throw new ValidationException("priceAdjustmentJson", $"Fiyat kuralı boş priceAdjustment içeriyor: {rule.Code}");
            }

            try
            {
                return JsonSerializer.Deserialize<PriceAdjustmentDefinition>(rule.PriceAdjustmentJson, SerializerOptions)
                    ?? throw new ValidationException("priceAdjustmentJson", $"Fiyat kuralı okunamadı: {rule.Code}");
            }
            catch (JsonException)
            {
                throw new ValidationException("priceAdjustmentJson", $"Fiyat kuralı JSON formatı geçersiz: {rule.Code}");
            }
        }

        private static decimal CalculateAdjustment(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            PriceAdjustmentDefinition adjustment,
            decimal basePrice,
            decimal currentPrice,
            decimal previousResult)
        {
            var basis = ResolveBasis(adjustment, basePrice, currentPrice, previousResult);
            var tiers = adjustment.Tiers ?? [];
            if (tiers.Count > 0)
            {
                var unitCount = ResolveUnitCount(product, request, adjustment);
                return tiers.Sum(tier => CalculateTierAdjustment(tier, adjustment, basis, unitCount));
            }

            var units = string.Equals(adjustment.Mode, "unit", StringComparison.OrdinalIgnoreCase)
                ? Math.Max(0, ResolveUnitCount(product, request, adjustment) - (adjustment.Unit?.FreeUnits ?? 0))
                : 1m;

            return CalculateSingleAdjustment(ResolveType(adjustment), ResolveValue(adjustment), basis, units, adjustment);
        }

        private static decimal CalculateTierAdjustment(
            PriceAdjustmentTier tier,
            PriceAdjustmentDefinition adjustment,
            decimal basis,
            decimal unitCount)
        {
            var from = Math.Max(1, tier.From);
            var to = tier.To ?? unitCount;
            var tierUnits = Math.Max(0, Math.Min(unitCount, to) - from + 1);
            if (tierUnits <= 0)
            {
                return 0;
            }

            return CalculateSingleAdjustment(
                tier.Type ?? ResolveType(adjustment),
                tier.Value ?? ResolveValue(adjustment),
                basis,
                tierUnits,
                adjustment);
        }

        private static decimal CalculateSingleAdjustment(
            string type,
            decimal value,
            decimal basis,
            decimal units,
            PriceAdjustmentDefinition adjustment)
        {
            var amount = type.ToLowerInvariant() switch
            {
                "fixed" or "amount" => value * units,
                "percent" or "percentage" => basis * (value / 100m) * units,
                "multiplier" => basis * (value - 1m) * units,
                "custom" => throw new ValidationException("priceAdjustmentJson", "Custom fiyat kuralı tipi için backend stratejisi tanımlanmalıdır."),
                _ => throw new ValidationException("priceAdjustmentJson", $"Desteklenmeyen fiyat kuralı tipi: {type}")
            };

            if (IsSubtract(adjustment))
            {
                return -Math.Abs(amount);
            }

            return amount;
        }

        private static decimal ApplyLimits(decimal amount, decimal currentPrice, PriceAdjustmentLimits? limits)
        {
            if (limits is null)
            {
                return amount;
            }

            if (limits.MinAdjustment.HasValue && amount < limits.MinAdjustment.Value)
            {
                amount = limits.MinAdjustment.Value;
            }

            if (limits.MaxAdjustment.HasValue && amount > limits.MaxAdjustment.Value)
            {
                amount = limits.MaxAdjustment.Value;
            }

            var finalPrice = currentPrice + amount;
            if (limits.MinFinalPrice.HasValue && finalPrice < limits.MinFinalPrice.Value)
            {
                amount += limits.MinFinalPrice.Value - finalPrice;
                finalPrice = currentPrice + amount;
            }

            if (limits.MaxFinalPrice.HasValue && finalPrice > limits.MaxFinalPrice.Value)
            {
                amount -= finalPrice - limits.MaxFinalPrice.Value;
            }

            return amount;
        }

        private static decimal ResolveBasis(PriceAdjustmentDefinition adjustment, decimal basePrice, decimal currentPrice, decimal previousResult)
            => ResolveApplyOn(adjustment).ToLowerInvariant() switch
            {
                "baseprice" => basePrice,
                "previousresult" => previousResult,
                _ => currentPrice
            };

        private static decimal ResolveUnitCount(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            PriceAdjustmentDefinition adjustment)
        {
            var raw = adjustment.Unit?.Field is null
                ? request.Quantity
                : ResolveNumericField(product, request, adjustment.Unit.Field);

            var rounded = (adjustment.Unit?.Rounding ?? "none").ToLowerInvariant() switch
            {
                "ceil" => Math.Ceiling(raw),
                "floor" => Math.Floor(raw),
                "round" => Math.Round(raw, 0, MidpointRounding.AwayFromZero),
                _ => raw
            };

            return Math.Max(0, rounded);
        }

        private static decimal ResolveNumericField(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            string field)
        {
            var value = ResolveField(product, request, field);
            if (TryConvertToDecimal(value, out var number))
            {
                return number;
            }

            throw new ValidationException("priceAdjustmentJson", $"Fiyat kuralı alanı sayısal değil veya bulunamadı: {field}");
        }

        private static bool ConditionsMatch(
            ProductDetailDto product,
            CalculateProductPriceRequestDto request,
            string? conditionsJson,
            JsonElement? embeddedConditions)
        {
            if (!string.IsNullOrWhiteSpace(conditionsJson))
            {
                using var document = JsonDocument.Parse(conditionsJson);
                return ConditionsMatch(product, request, document.RootElement);
            }

            if (embeddedConditions.HasValue && embeddedConditions.Value.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null)
            {
                return ConditionsMatch(product, request, embeddedConditions.Value);
            }

            return true;
        }

        private static bool ConditionsMatch(ProductDetailDto product, CalculateProductPriceRequestDto request, JsonElement conditions)
        {
            var mode = ReadString(conditions, "operator")
                ?? ReadString(conditions, "mode")
                ?? ReadString(conditions, "match")
                ?? "all";

            var items = ReadArray(conditions, "items")
                ?? ReadArray(conditions, "conditions")
                ?? (conditions.ValueKind == JsonValueKind.Array ? conditions.EnumerateArray().ToArray() : null);

            if (items is null && conditions.ValueKind == JsonValueKind.Object && conditions.TryGetProperty("field", out _))
            {
                items = [conditions];
            }

            if (items is null || items.Length == 0)
            {
                return true;
            }

            return string.Equals(mode, "any", StringComparison.OrdinalIgnoreCase)
                ? items.Any(item => ConditionMatches(product, request, item))
                : items.All(item => ConditionMatches(product, request, item));
        }

        private static bool ConditionMatches(ProductDetailDto product, CalculateProductPriceRequestDto request, JsonElement condition)
        {
            var field = ReadString(condition, "field")
                ?? throw new ValidationException("conditionsJson", "Koşul alanı eksik: field");
            var op = ReadString(condition, "operator") ?? ReadString(condition, "op") ?? "eq";
            var actual = ResolveField(product, request, field);

            if (string.Equals(op, "exists", StringComparison.OrdinalIgnoreCase))
            {
                return actual.Exists;
            }

            if (!condition.TryGetProperty("value", out var expected))
            {
                return false;
            }

            return Compare(actual, expected, op);
        }

        private static bool Compare(ResolvedValue actual, JsonElement expected, string op)
        {
            if (TryConvertToDecimal(actual, out var actualNumber) && TryConvertToDecimal(expected, out var expectedNumber))
            {
                return op.ToLowerInvariant() switch
                {
                    "eq" or "=" or "==" => actualNumber == expectedNumber,
                    "neq" or "!=" => actualNumber != expectedNumber,
                    "gt" or ">" => actualNumber > expectedNumber,
                    "gte" or ">=" => actualNumber >= expectedNumber,
                    "lt" or "<" => actualNumber < expectedNumber,
                    "lte" or "<=" => actualNumber <= expectedNumber,
                    _ => false
                };
            }

            var actualText = actual.Text ?? string.Empty;
            if (expected.ValueKind == JsonValueKind.Array)
            {
                return expected.EnumerateArray().Any(item =>
                    string.Equals(actualText, ReadElementAsString(item), StringComparison.OrdinalIgnoreCase));
            }

            var expectedText = ReadElementAsString(expected);
            return op.ToLowerInvariant() switch
            {
                "eq" or "=" or "==" => string.Equals(actualText, expectedText, StringComparison.OrdinalIgnoreCase),
                "neq" or "!=" => !string.Equals(actualText, expectedText, StringComparison.OrdinalIgnoreCase),
                "contains" => actualText.Contains(expectedText, StringComparison.OrdinalIgnoreCase),
                "in" => expected.ValueKind == JsonValueKind.Array && expected.EnumerateArray().Any(item =>
                    string.Equals(actualText, ReadElementAsString(item), StringComparison.OrdinalIgnoreCase)),
                _ => false
            };
        }

        private static ResolvedValue ResolveField(ProductDetailDto product, CalculateProductPriceRequestDto request, string field)
        {
            var normalized = field.Trim();
            if (normalized.StartsWith("feature.", StringComparison.OrdinalIgnoreCase))
            {
                var key = normalized["feature.".Length..];
                var feature = ResolveRequestFeature(request, key) ?? ResolveRequestFeature(request, normalized);
                if (feature is not null)
                {
                    return feature.Value;
                }

                var attribute = product.AttributeValues.FirstOrDefault(value =>
                    string.Equals(value.AttributeKey, key, StringComparison.OrdinalIgnoreCase));
                return attribute is null ? ResolvedValue.Missing : ResolvedValue.FromAttribute(attribute);
            }

            return normalized.ToLowerInvariant() switch
            {
                "quantity" or "request.quantity" => ResolvedValue.FromDecimal(request.Quantity),
                "saleschannel" or "request.saleschannel" => ResolvedValue.FromString(request.SalesChannel),
                "customergroupcode" or "request.customergroupcode" => ResolvedValue.FromString(request.CustomerGroupCode),
                "product.kind" => ResolvedValue.FromDecimal(product.Kind),
                "product.status" => ResolvedValue.FromDecimal(product.Status),
                "product.code" or "product.productcode" => ResolvedValue.FromString(product.ProductCode),
                "product.name" => ResolvedValue.FromString(product.Name),
                _ => ResolveRequestFeature(request, normalized) ?? ResolvedValue.Missing
            };
        }

        private static ResolvedValue? ResolveRequestFeature(CalculateProductPriceRequestDto request, string key)
        {
            if (request.FeatureValues is null)
            {
                return null;
            }

            foreach (var feature in request.FeatureValues)
            {
                if (string.Equals(feature.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return ResolvedValue.FromJson(feature.Value);
                }
            }

            return null;
        }

        private static string ResolveType(PriceAdjustmentDefinition adjustment)
            => adjustment.Type
                ?? adjustment.Kind
                ?? adjustment.AdjustmentType
                ?? "fixed";

        private static decimal ResolveValue(PriceAdjustmentDefinition adjustment)
            => adjustment.Value
                ?? adjustment.Amount
                ?? adjustment.Adjustment
                ?? adjustment.Percent
                ?? 0m;

        private static string ResolveApplyOn(PriceAdjustmentDefinition adjustment)
            => adjustment.ApplyOn ?? "currentPrice";

        private static bool IsSubtract(PriceAdjustmentDefinition adjustment)
            => string.Equals(adjustment.Operation, "subtract", StringComparison.OrdinalIgnoreCase)
                || string.Equals(adjustment.Direction, "subtract", StringComparison.OrdinalIgnoreCase);

        private static string? ReadString(JsonElement element, string propertyName)
            => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property)
                ? ReadElementAsString(property)
                : null;

        private static JsonElement[]? ReadArray(JsonElement element, string propertyName)
            => element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty(propertyName, out var property)
                && property.ValueKind == JsonValueKind.Array
                    ? property.EnumerateArray().ToArray()
                    : null;

        private static string ReadElementAsString(JsonElement element)
            => element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => element.GetRawText()
            };

        private static bool TryConvertToDecimal(ResolvedValue value, out decimal number)
        {
            if (value.Number.HasValue)
            {
                number = value.Number.Value;
                return true;
            }

            return decimal.TryParse(value.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out number);
        }

        private static bool TryConvertToDecimal(JsonElement value, out decimal number)
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out number))
            {
                return true;
            }

            return decimal.TryParse(ReadElementAsString(value), NumberStyles.Any, CultureInfo.InvariantCulture, out number);
        }

        private sealed record PriceAdjustmentDefinition
        {
            public string? Mode { get; init; }
            public string? Type { get; init; }
            public string? Kind { get; init; }
            public string? AdjustmentType { get; init; }
            public string? Operation { get; init; }
            public string? Direction { get; init; }
            public decimal? Value { get; init; }
            public decimal? Amount { get; init; }
            public decimal? Adjustment { get; init; }
            public decimal? Percent { get; init; }
            public string? ApplyOn { get; init; }
            public PriceAdjustmentUnit? Unit { get; init; }
            public IReadOnlyList<PriceAdjustmentTier>? Tiers { get; init; }
            public PriceAdjustmentLimits? Limits { get; init; }
            public JsonElement? Conditions { get; init; }
        }

        private sealed record PriceAdjustmentUnit
        {
            public string? Field { get; init; }
            public decimal FreeUnits { get; init; }
            public string? Rounding { get; init; }
        }

        private sealed record PriceAdjustmentTier
        {
            public decimal From { get; init; }
            public decimal? To { get; init; }
            public string? Type { get; init; }
            public decimal? Value { get; init; }
        }

        private sealed record PriceAdjustmentLimits
        {
            public decimal? MinAdjustment { get; init; }
            public decimal? MaxAdjustment { get; init; }
            public decimal? MinFinalPrice { get; init; }
            public decimal? MaxFinalPrice { get; init; }
        }

        private readonly record struct ResolvedValue(bool Exists, string? Text, decimal? Number)
        {
            public static ResolvedValue Missing => new(false, null, null);

            public static ResolvedValue FromDecimal(decimal value) => new(true, value.ToString(CultureInfo.InvariantCulture), value);

            public static ResolvedValue FromString(string? value) => string.IsNullOrWhiteSpace(value)
                ? Missing
                : new(true, value, null);

            public static ResolvedValue FromAttribute(ProductAttributeValueDto attribute)
            {
                if (attribute.ValueNumber.HasValue)
                {
                    return FromDecimal(attribute.ValueNumber.Value);
                }

                if (attribute.ValueBool.HasValue)
                {
                    return new(true, attribute.ValueBool.Value ? "true" : "false", null);
                }

                if (attribute.ValueDate.HasValue)
                {
                    return new(true, attribute.ValueDate.Value.ToString("O", CultureInfo.InvariantCulture), null);
                }

                return FromString(attribute.ValueText ?? attribute.ValueJson);
            }

            public static ResolvedValue FromJson(JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.Number when element.TryGetDecimal(out var number) => FromDecimal(number),
                    JsonValueKind.String => FromString(element.GetString()),
                    JsonValueKind.True => new(true, "true", null),
                    JsonValueKind.False => new(true, "false", null),
                    JsonValueKind.Null or JsonValueKind.Undefined => Missing,
                    _ => new(true, element.GetRawText(), null)
                };
            }
        }
    }
}
