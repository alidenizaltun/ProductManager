using FluentValidation;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Presentation.Validators.ProductOperations
{
    public sealed class CreatePricingTemplateRequestDtoValidator : AbstractValidator<CreatePricingTemplateRequestDto>
    {
        public CreatePricingTemplateRequestDtoValidator()
        {
            // Kod gönderilmezse sistem üretir; gönderildiyse yalnızca uzunluğu doğrulanır.
            RuleFor(x => x.Code)
                .MaximumLength(64).WithMessage("Şablon kodu en fazla 64 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.Code));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Şablon adı zorunludur.")
                .MaximumLength(200).WithMessage("Şablon adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.TemplateKind)
                .InclusiveBetween(1, 5).WithMessage("Geçersiz şablon türü.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Para birimi zorunludur.")
                .Length(3).WithMessage("Para birimi 3 karakter olmalıdır.");

            RuleFor(x => x)
                .Must(HavePayload)
                .WithMessage("payload veya payloadJson alanı zorunludur.");
        }

        internal static bool HavePayload(CreatePricingTemplateRequestDto request)
            => !string.IsNullOrWhiteSpace(request.PayloadJson) || request.Payload.HasValue;
    }

    public sealed class UpdatePricingTemplateRequestDtoValidator : AbstractValidator<UpdatePricingTemplateRequestDto>
    {
        public UpdatePricingTemplateRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Şablon kodu zorunludur.")
                .MaximumLength(64).WithMessage("Şablon kodu en fazla 64 karakter olabilir.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Şablon adı zorunludur.")
                .MaximumLength(200).WithMessage("Şablon adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Para birimi zorunludur.")
                .Length(3).WithMessage("Para birimi 3 karakter olmalıdır.");

            RuleFor(x => x)
                .Must(request => !string.IsNullOrWhiteSpace(request.PayloadJson) || request.Payload.HasValue)
                .WithMessage("payload veya payloadJson alanı zorunludur.");
        }
    }

    public sealed class ApplyPricingTemplateRequestDtoValidator : AbstractValidator<ApplyPricingTemplateRequestDto>
    {
        public ApplyPricingTemplateRequestDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Ürün seçilmelidir.");

            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom!.Value)
                .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.")
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue);
        }
    }

    public sealed class ApplyPricingTemplateBulkRequestDtoValidator : AbstractValidator<ApplyPricingTemplateBulkRequestDto>
    {
        public ApplyPricingTemplateBulkRequestDtoValidator()
        {
            RuleFor(x => x.ProductIds)
                .NotEmpty().WithMessage("En az bir ürün seçilmelidir.");
        }
    }

    public sealed class CreatePriceRevisionRequestDtoValidator : AbstractValidator<CreatePriceRevisionRequestDto>
    {
        public CreatePriceRevisionRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .MaximumLength(64).WithMessage("Revizyon kodu en fazla 64 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.Code));

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Revizyon adı zorunludur.")
                .MaximumLength(200).WithMessage("Revizyon adı en fazla 200 karakter olabilir.");

            PriceRevisionRules.Apply(this, x => x.AdjustmentType, x => x.Value, x => x.RoundingMode, x => x.RoundingStep, x => x.CurrencyCode);
        }
    }

    public sealed class UpdatePriceRevisionRequestDtoValidator : AbstractValidator<UpdatePriceRevisionRequestDto>
    {
        public UpdatePriceRevisionRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Revizyon kodu zorunludur.")
                .MaximumLength(64).WithMessage("Revizyon kodu en fazla 64 karakter olabilir.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Revizyon adı zorunludur.")
                .MaximumLength(200).WithMessage("Revizyon adı en fazla 200 karakter olabilir.");

            PriceRevisionRules.Apply(this, x => x.AdjustmentType, x => x.Value, x => x.RoundingMode, x => x.RoundingStep, x => x.CurrencyCode);
        }
    }

    public sealed class CreatePriceRevisionScopeRequestDtoValidator : AbstractValidator<CreatePriceRevisionScopeRequestDto>
    {
        private const int ScopeTypeProductKind = 7;

        public CreatePriceRevisionScopeRequestDtoValidator()
        {
            RuleFor(x => x.ScopeType)
                .InclusiveBetween(1, 8).WithMessage("Geçersiz kapsam türü.");

            RuleFor(x => x.TargetId)
                .NotEmpty().WithMessage("Kapsam hedefi seçilmelidir.")
                .When(x => x.ScopeType != ScopeTypeProductKind);

            RuleFor(x => x.TargetValue)
                .NotEmpty().WithMessage("Ürün tipi kapsamı için bir değer gerekir.")
                .MaximumLength(64)
                .When(x => x.ScopeType == ScopeTypeProductKind);
        }
    }

    public sealed class RejectPriceRevisionRequestDtoValidator : AbstractValidator<RejectPriceRevisionRequestDto>
    {
        public RejectPriceRevisionRequestDtoValidator()
        {
            RuleFor(x => x.Note)
                .NotEmpty().WithMessage("Ret gerekçesi zorunludur.")
                .MaximumLength(1000).WithMessage("Ret gerekçesi en fazla 1000 karakter olabilir.");
        }
    }

    public sealed class ApprovePriceRevisionRequestDtoValidator : AbstractValidator<ApprovePriceRevisionRequestDto>
    {
        public ApprovePriceRevisionRequestDtoValidator()
        {
            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Onay notu en fazla 1000 karakter olabilir.");
        }
    }

    public sealed class CreateProductPricingRuleRequestDtoValidator : AbstractValidator<CreateProductPricingRuleRequestDto>
    {
        public CreateProductPricingRuleRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kural kodu zorunludur.")
                .MaximumLength(64).WithMessage("Kural kodu en fazla 64 karakter olabilir.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kural adı zorunludur.")
                .MaximumLength(200).WithMessage("Kural adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.Priority)
                .GreaterThanOrEqualTo(0).WithMessage("Öncelik negatif olamaz.");

            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom!.Value)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
                .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }
    }

    public sealed class UpdateProductPricingRuleRequestDtoValidator : AbstractValidator<UpdateProductPricingRuleRequestDto>
    {
        public UpdateProductPricingRuleRequestDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kural kodu zorunludur.")
                .MaximumLength(64).WithMessage("Kural kodu en fazla 64 karakter olabilir.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kural adı zorunludur.")
                .MaximumLength(200).WithMessage("Kural adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.Priority)
                .GreaterThanOrEqualTo(0).WithMessage("Öncelik negatif olamaz.");

            RuleFor(x => x.ValidTo)
                .GreaterThan(x => x.ValidFrom!.Value)
                .When(x => x.ValidFrom.HasValue && x.ValidTo.HasValue)
                .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }
    }

    public sealed class ReorderProductPricingRulesRequestDtoValidator : AbstractValidator<ReorderProductPricingRulesRequestDto>
    {
        public ReorderProductPricingRulesRequestDtoValidator()
        {
            RuleFor(x => x.OrderedPricingRuleIds)
                .NotEmpty().WithMessage("En az bir kural sıralanmalıdır.");
        }
    }

    public sealed class SavePricingRuleAsTemplateRequestDtoValidator : AbstractValidator<SavePricingRuleAsTemplateRequestDto>
    {
        public SavePricingRuleAsTemplateRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Şablon adı en fazla 200 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Code)
                .MaximumLength(64).WithMessage("Şablon kodu en fazla 64 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.Code));
        }
    }

    public sealed class UpdatePriceRevisionLineRequestDtoValidator : AbstractValidator<UpdatePriceRevisionLineRequestDto>
    {
        public UpdatePriceRevisionLineRequestDtoValidator()
        {
            RuleFor(x => x.NewValue)
                .GreaterThanOrEqualTo(0).When(x => x.NewValue.HasValue)
                .WithMessage("Yeni değer negatif olamaz.");
        }
    }

    /// <summary>
    /// Oran, yuvarlama ve para birimi kuralları iki istek tipinde de aynı olduğu için
    /// tek yerde tutulur.
    /// </summary>
    internal static class PriceRevisionRules
    {
        private const int AdjustmentTypePercent = 1;
        private const int AdjustmentTypeAmount = 2;
        private const int AdjustmentTypeSetValue = 3;
        private const int AdjustmentTypeMultiplier = 4;
        private const int RoundingModeNone = 1;

        public static void Apply<T>(
            AbstractValidator<T> validator,
            Func<T, int> adjustmentType,
            Func<T, decimal> value,
            Func<T, int> roundingMode,
            Func<T, decimal?> roundingStep,
            Func<T, string?> currencyCode)
        {
            validator.RuleFor(x => adjustmentType(x))
                .InclusiveBetween(1, 4).WithMessage("Geçersiz zam türü.")
                .OverridePropertyName("adjustmentType");

            validator.RuleFor(x => roundingMode(x))
                .InclusiveBetween(1, 4).WithMessage("Geçersiz yuvarlama türü.")
                .OverridePropertyName("roundingMode");

            validator.RuleFor(x => roundingStep(x))
                .GreaterThan(0).WithMessage("Yuvarlama adımı sıfırdan büyük olmalıdır.")
                .OverridePropertyName("roundingStep")
                .When(x => roundingMode(x) != RoundingModeNone && roundingStep(x).HasValue);

            validator.RuleFor(x => value(x))
                .GreaterThan(0).WithMessage("Çarpan sıfırdan büyük olmalıdır.")
                .OverridePropertyName("value")
                .When(x => adjustmentType(x) == AdjustmentTypeMultiplier);

            validator.RuleFor(x => value(x))
                .GreaterThan(-100).WithMessage("Yüzde değeri -100'den küçük olamaz.")
                .OverridePropertyName("value")
                .When(x => adjustmentType(x) == AdjustmentTypePercent);

            // Sabit tutar ve sabit değer para birimine bağlıdır: kapsamda TRY ve USD
            // fiyatlar birlikte varsa "5 ekle" ifadesi anlamsızlaşır.
            validator.RuleFor(x => currencyCode(x))
                .NotEmpty()
                .WithMessage("Tutar bazlı zam için para birimi filtresi zorunludur.")
                .OverridePropertyName("currencyCode")
                .When(x => adjustmentType(x) is AdjustmentTypeAmount or AdjustmentTypeSetValue);

            validator.RuleFor(x => currencyCode(x))
                .Length(3).WithMessage("Para birimi 3 karakter olmalıdır.")
                .OverridePropertyName("currencyCode")
                .When(x => !string.IsNullOrWhiteSpace(currencyCode(x)));
        }
    }
}
