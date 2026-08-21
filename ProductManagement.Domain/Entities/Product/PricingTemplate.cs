using ProductManagement.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Domain.Entities.Product
{
    /// <summary>
    /// Ürün bağımsız, yeniden kullanılabilir fiyatlandırma tanımı. Bir kez kurulan
    /// fiyatlandırma (ör. SMS birim fiyatı) şablon olarak saklanır ve başka ürünlere
    /// kopyalanarak uygulanır. Kopyalanan kayıt şablonu <c>SourceTemplateId</c> ile
    /// işaret eder; zam kapsamı bu iz üzerinden bulunur.
    /// </summary>
    [Table("PricingTemplates", Schema = "Product")]
    public class PricingTemplate : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public PricingTemplateKind TemplateKind { get; set; } = PricingTemplateKind.PricingRule;

        /// <summary>Şablonun bağlı olduğu global birim (ör. SMS). Uygulama anında hedef ürünün birimi buradan çözülür.</summary>
        public Guid? UnitDefinitionId { get; set; }
        public UnitDefinition? UnitDefinition { get; set; }

        public string CurrencyCode { get; set; } = "TRY";

        /// <summary>Kuralın <c>priceAdjustment</c> gövdesi. ProductPricingRules ile birebir aynı format.</summary>
        public string PayloadJson { get; set; } = string.Empty;

        /// <summary>Payload her değiştiğinde artar; kopyaya iz olarak yazılır.</summary>
        public int Version { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
