using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    [Table("ProductLicenseOfferings", Schema = "Product")]
    public class ProductLicenseOffering : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public SoftwareLicenseModel LicenseModel { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // --- Fiyat ---

        public decimal BasePrice { get; set; }

        public string CurrencyCode { get; set; } = "TRY";

        // --- Abonelik parametreleri (LicenseModel = Subscription ise doldurulur) ---

        public BillingPeriodUnit? BillingPeriodUnit { get; set; }
        public int? BillingPeriodValue { get; set; }
        public bool AutoRenew { get; set; } = true;
        public int? GracePeriodDays { get; set; }

        // --- Trial parametreleri (LicenseModel = Trial ise doldurulur) ---

        public int? TrialDays { get; set; }
        public Guid? ConvertToOfferingId { get; set; }
        public ProductLicenseOffering? ConvertToOffering { get; set; }

        // --- Seat / kullanıcı parametreleri ---

        public int? MaxSeats { get; set; }

        // --- Geçerlilik ---

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
