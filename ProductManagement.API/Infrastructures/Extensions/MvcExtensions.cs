using Microsoft.AspNetCore.Mvc;
using ProductManagement.API.Models;
using System.Text.RegularExpressions;

namespace ProductManagement.API.Infrastructures.Extensions
{
    public static class MvcExtensions
    {
        private static readonly IReadOnlyDictionary<string, string> FieldLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["amount"] = "Tutar",
            ["barcode"] = "Barkod",
            ["brand"] = "Marka",
            ["code"] = "Kod",
            ["conditionsJson"] = "Koşul JSON",
            ["currencyCode"] = "Para birimi",
            ["defaultCurrencyCode"] = "Varsayılan para birimi",
            ["description"] = "Açıklama",
            ["discountAmount"] = "İndirim tutarı",
            ["discountPercent"] = "İndirim oranı",
            ["id"] = "Kimlik",
            ["isActive"] = "Aktiflik durumu",
            ["isPurchasable"] = "Satın alınabilirlik durumu",
            ["isSellable"] = "Satılabilirlik durumu",
            ["kind"] = "Ürün tipi",
            ["manufacturer"] = "Üretici",
            ["maxQuantity"] = "Maksimum miktar",
            ["metadataJson"] = "Metadata JSON",
            ["minQuantity"] = "Minimum miktar",
            ["name"] = "Ad",
            ["priceAdjustment"] = "Fiyat ayarı",
            ["priceAdjustmentJson"] = "Fiyat ayarı JSON",
            ["product"] = "Ürün",
            ["productCode"] = "Ürün kodu",
            ["productId"] = "Ürün",
            ["quantity"] = "Miktar",
            ["shortDescription"] = "Kısa açıklama",
            ["sku"] = "SKU",
            ["status"] = "Durum",
            ["taxCode"] = "Vergi kodu",
            ["taxRate"] = "Vergi oranı",
            ["taxRateOverride"] = "Vergi oranı",
            ["trackInventory"] = "Stok takibi",
            ["unitCost"] = "Birim maliyet",
            ["unitDefinitionId"] = "Birim"
        };

        private static readonly Regex JsonPathRegex = new(@"Path:\s*(?<path>\$[^\|]*)", RegexOptions.Compiled);
        private static readonly Regex RequiredPropertyRegex = new(@"required propert(?:y|ies).*?:\s*(?<fields>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void ConfigureCacheProfiles(this MvcOptions options, IHostApplicationBuilder builder)
        {
            var cacheProfiles = builder.Configuration
                .GetSection("CacheProfiles")
                .GetChildren();

            foreach (var cacheProfile in cacheProfiles)
            {
                options.CacheProfiles
                    .Add(cacheProfile.Key,
                        value: cacheProfile.Get<CacheProfile>()!
                    );
            }
        }

        public static void ConfigureUserFriendlyValidationErrors(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(error => new
                        {
                            Field = NormalizeFieldKey(x.Key, error.Exception?.Message ?? error.ErrorMessage),
                            Message = BuildFriendlyValidationMessage(x.Key, error.ErrorMessage, error.Exception)
                        }))
                        .GroupBy(x => x.Field, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Select(error => error.Message).Distinct().ToArray(),
                            StringComparer.OrdinalIgnoreCase);

                    if (errors.Count == 0)
                    {
                        errors["request"] = ["Gönderilen veri geçerli değil. Lütfen alanları kontrol edin."];
                    }

                    var errorModel = new SystemModel.ErrorDetails
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        ErrorCode = string.Concat("VAL_", Guid.NewGuid().ToString("N").AsSpan(0, 8)),
                        Message = "Gönderilen bilgilerde hata var. Lütfen işaretli alanları kontrol edin.",
                        AdditionalData = errors
                    };

                    return new BadRequestObjectResult(errorModel);
                };
            });
        }

        private static string BuildFriendlyValidationMessage(string key, string? errorMessage, Exception? exception)
        {
            var sourceMessage = exception?.Message ?? errorMessage ?? string.Empty;
            var fieldKey = NormalizeFieldKey(key, sourceMessage);
            var fieldName = GetFieldLabel(fieldKey);

            if (sourceMessage.Contains("could not be converted", StringComparison.OrdinalIgnoreCase))
            {
                if (sourceMessage.Contains("System.Decimal", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{fieldName} alanı sayısal bir değer olmalıdır. Örn: 18 veya 18.5.";
                }

                if (sourceMessage.Contains("System.Int", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{fieldName} alanı tam sayı olmalıdır.";
                }

                if (sourceMessage.Contains("System.Guid", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{fieldName} alanı geçerli bir kimlik değeri olmalıdır.";
                }

                if (sourceMessage.Contains("System.Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{fieldName} alanı true veya false olmalıdır.";
                }

                if (sourceMessage.Contains("System.DateTime", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{fieldName} alanı geçerli bir tarih olmalıdır.";
                }

                return $"{fieldName} alanının veri tipi geçersiz.";
            }

            if (sourceMessage.Contains("The JSON value is either too large or too small", StringComparison.OrdinalIgnoreCase))
            {
                return $"{fieldName} alanı izin verilen aralığın dışında.";
            }

            if (sourceMessage.Contains("is missing required properties", StringComparison.OrdinalIgnoreCase) ||
                sourceMessage.Contains("missing required property", StringComparison.OrdinalIgnoreCase))
            {
                return BuildMissingRequiredPropertiesMessage(sourceMessage);
            }

            if (sourceMessage.Contains("invalid start of a value", StringComparison.OrdinalIgnoreCase) ||
                sourceMessage.Contains("'<' is an invalid start", StringComparison.OrdinalIgnoreCase) ||
                sourceMessage.Contains("Expected", StringComparison.OrdinalIgnoreCase))
            {
                return "JSON formatı geçersiz. Lütfen gönderilen veriyi kontrol edin.";
            }

            return TranslateKnownValidationMessage(sourceMessage, fieldName);
        }

        private static string NormalizeFieldKey(string key, string? message)
        {
            var candidate = string.IsNullOrWhiteSpace(key) ? ExtractJsonPath(message) : key;
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return "request";
            }

            candidate = candidate.Trim();
            if (candidate.StartsWith("$.", StringComparison.Ordinal))
            {
                candidate = candidate[2..];
            }
            else if (candidate.Equals("$", StringComparison.Ordinal))
            {
                return "request";
            }

            candidate = candidate.Replace("[", ".", StringComparison.Ordinal)
                .Replace("]", string.Empty, StringComparison.Ordinal);

            return candidate.Trim('.') is { Length: > 0 } normalized ? normalized : "request";
        }

        private static string ExtractJsonPath(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            var match = JsonPathRegex.Match(message);
            return match.Success ? match.Groups["path"].Value.Trim() : string.Empty;
        }

        private static string GetFieldLabel(string fieldKey)
        {
            var lastSegment = fieldKey.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? fieldKey;

            if (FieldLabels.TryGetValue(lastSegment, out var label))
            {
                return label;
            }

            return lastSegment;
        }

        private static string BuildMissingRequiredPropertiesMessage(string message)
        {
            var match = RequiredPropertyRegex.Match(message);
            if (!match.Success)
            {
                return "Zorunlu alanlardan biri eksik. Lütfen gönderilen veriyi kontrol edin.";
            }

            var labels = match.Groups["fields"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim('\'', '"', '.', ' '))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(GetFieldLabel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return labels.Length == 0
                ? "Zorunlu alanlardan biri eksik. Lütfen gönderilen veriyi kontrol edin."
                : $"Zorunlu alan eksik: {string.Join(", ", labels)}.";
        }

        private static string TranslateKnownValidationMessage(string message, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return $"{fieldName} alanı geçerli değil.";
            }

            return message switch
            {
                "The input was not valid." => $"{fieldName} alanı geçerli değil.",
                var m when m.Contains("is required", StringComparison.OrdinalIgnoreCase) => $"{fieldName} alanı zorunludur.",
                var m when m.Contains("cannot be negative", StringComparison.OrdinalIgnoreCase) => $"{fieldName} alanı negatif olamaz.",
                var m when m.Contains("max length is", StringComparison.OrdinalIgnoreCase) => $"{fieldName} alanı izin verilen uzunluğu aşıyor.",
                var m when m.Contains("must be 3 characters", StringComparison.OrdinalIgnoreCase) => $"{fieldName} alanı 3 karakter olmalıdır.",
                var m when m.Contains("uppercase ISO format", StringComparison.OrdinalIgnoreCase) => $"{fieldName} alanı büyük harf ISO formatında olmalıdır.",
                var m when m.Contains("Invalid", StringComparison.OrdinalIgnoreCase) => $"{fieldName} alanı geçerli değil.",
                _ => message
            };
        }
    }
}
