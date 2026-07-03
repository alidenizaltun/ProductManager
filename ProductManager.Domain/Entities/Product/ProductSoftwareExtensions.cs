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

        public bool IsOptional { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public ICollection<ProductModuleOfferingPrice> OfferingPrices { get; set; } = new List<ProductModuleOfferingPrice>();
    }

    [Table("ProductModuleOfferingPrices", Schema = "Product")]
    public class ProductModuleOfferingPrice : BaseEntity
    {
        public Guid ProductModuleId { get; set; }
        public ProductModule? ProductModule { get; set; }

        public Guid ProductLicenseOfferingId { get; set; }
        public ProductLicenseOffering? ProductLicenseOffering { get; set; }

        public decimal Price { get; set; }

        public string CurrencyCode { get; set; } = "TRY";

        public bool IsActive { get; set; } = true;
    }

}
