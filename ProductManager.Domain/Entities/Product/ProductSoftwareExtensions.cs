using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    [Table("ProductModules", Schema = "Product")]
    public class ProductModule : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public string ModuleCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal AdditionalPrice { get; set; }

        public string CurrencyCode { get; set; } = "TRY";

        public bool IsOptional { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }

    [Table("SoftwarePricingTiers", Schema = "Product")]
    public class SoftwarePricingTier : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public SoftwareLicenseModel LicenseModel { get; set; } = SoftwareLicenseModel.SeatBased;

        public string Unit { get; set; } = "user";

        public int MinUnits { get; set; }

        public int? MaxUnits { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal FlatFee { get; set; } = 0;

        public string CurrencyCode { get; set; } = "TRY";

        public bool IsActive { get; set; } = true;
    }
}
