using ProductManagement.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Domain.Entities.Product
{
    /// <summary>
    /// Toplu fiyat değişikliği belgesi ("zam"). Doğrudan bir UPDATE değildir:
    /// kapsamı seçilir, önizlenir, satır satır düzeltilir, onaylanır, uygulanır ve
    /// gerekirse geri alınır. Eski fiyatlar <see cref="PriceRevisionLine.OldValue"/>
    /// içinde saklandığı için ayrı bir fiyat geçmişi tablosuna ihtiyaç yoktur.
    /// </summary>
    [Table("PriceRevisions", Schema = "Product")]
    public class PriceRevision : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public PriceAdjustmentType AdjustmentType { get; set; } = PriceAdjustmentType.Percent;

        /// <summary>%15 için 15, 5 TL için 5.</summary>
        public decimal Value { get; set; }

        public PriceRoundingMode RoundingMode { get; set; } = PriceRoundingMode.None;

        /// <summary>Yuvarlama adımı (0,01 / 0,5 / 1 / 10). RoundingMode None ise kullanılmaz.</summary>
        public decimal? RoundingStep { get; set; }

        /// <summary>Doluysa yalnızca bu para birimindeki fiyatlar kapsama girer.</summary>
        public string? CurrencyCode { get; set; }

        public PriceRevisionStatus Status { get; set; } = PriceRevisionStatus.Draft;

        /// <summary>Yeni fiyatın geçerli olacağı tarih. Boşsa uygulama anında geçerlidir.</summary>
        public DateTime? EffectiveDate { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovalNote { get; set; }

        public DateTime? AppliedAt { get; set; }
        public Guid? AppliedByUserId { get; set; }

        public DateTime? RolledBackAt { get; set; }
        public Guid? RolledBackByUserId { get; set; }

        public ICollection<PriceRevisionScope> Scopes { get; set; } = new List<PriceRevisionScope>();
        public ICollection<PriceRevisionLine> Lines { get; set; } = new List<PriceRevisionLine>();
    }

    /// <summary>Revizyonun neye uygulanacağını belirleyen filtre satırı.</summary>
    [Table("PriceRevisionScopes", Schema = "Product")]
    public class PriceRevisionScope : BaseEntity
    {
        public Guid PriceRevisionId { get; set; }
        public PriceRevision? PriceRevision { get; set; }

        public PriceRevisionScopeType ScopeType { get; set; }

        public Guid? TargetId { get; set; }

        /// <summary>Guid olmayan hedefler (ör. ProductKind = 2).</summary>
        public string? TargetValue { get; set; }

        /// <summary>true ise bu satır kapsamdan çıkarır ("tüm yazılımlar, X ürünü hariç").</summary>
        public bool IsExclude { get; set; }
    }

    /// <summary>
    /// Önizleme çıktısı ve geri alma kaydı. Aynı satır iki iş görür:
    /// uygulanmadan önce "ne olacak", uygulandıktan sonra "ne idi".
    /// </summary>
    [Table("PriceRevisionLines", Schema = "Product")]
    public class PriceRevisionLine : BaseEntity
    {
        public Guid PriceRevisionId { get; set; }
        public PriceRevision? PriceRevision { get; set; }

        public PriceRevisionTargetType TargetType { get; set; }

        /// <summary>Güncellenecek satırın Id'si.</summary>
        public Guid TargetId { get; set; }

        /// <summary>
        /// JSON içi konum: <c>$.value</c> ya da <c>$.tiers[2].value</c>. Yalnızca kural
        /// hedeflerinde doludur; diğerlerinde boş string kalır — null olsaydı hem benzersiz
        /// indeks filtrelenirdi hem de SQL'de <c>= @TargetPath</c> karşılaştırması eşleşmezdi.
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;

        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        /// <summary>Önizleme ekranındaki açıklama ("SMS kuralı · kademe 2").</summary>
        public string TargetLabel { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = "TRY";

        public decimal OldValue { get; set; }
        public decimal NewValue { get; set; }

        public bool IsExcluded { get; set; }
        public bool IsApplied { get; set; }

        /// <summary>Uygulama ya da geri alma sırasında atlandıysa sebebi.</summary>
        public string? SkipReason { get; set; }
    }
}
