using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    [Table("Products", Schema = "Product")]
    public class Product : BaseEntity
    {
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        public ProductKind Kind { get; set; } = ProductKind.Physical;
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public string? Brand { get; set; }
        public string? Manufacturer { get; set; }
        public string? Barcode { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsSellable { get; set; } = true;
        public bool IsPurchasable { get; set; } = true;
        public bool TrackInventory { get; set; } = true;

        public string DefaultCurrencyCode { get; set; } = "TRY";
        public string? UnitOfMeasure { get; set; }

        public decimal? TaxRate { get; set; }
        public string? TaxCode { get; set; }

        public string? Tags { get; set; }
        public string? MetadataJson { get; set; }

        public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
        public ICollection<ProductInventory> Inventories { get; set; } = new List<ProductInventory>();
        public ICollection<ProductMedia> MediaItems { get; set; } = new List<ProductMedia>();
        public ICollection<ProductCategoryMap> CategoryMaps { get; set; } = new List<ProductCategoryMap>();
        public ICollection<ProductBundleItem> BundleItems { get; set; } = new List<ProductBundleItem>();

        public ProductPhysicalProfile? PhysicalProfile { get; set; }
        public ProductSoftwareProfile? SoftwareProfile { get; set; }
        public ProductServiceProfile? ServiceProfile { get; set; }
        public ProductSubscriptionProfile? SubscriptionProfile { get; set; }
    }
}
