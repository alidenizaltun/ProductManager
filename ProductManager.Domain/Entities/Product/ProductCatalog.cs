using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    [Table("ProductCategories", Schema = "Product")]
    public class ProductCategory : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public ProductCategory? ParentCategory { get; set; }

        public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>();
        public ICollection<ProductCategoryMap> ProductMaps { get; set; } = new List<ProductCategoryMap>();
    }

    [Table("ProductCategoryMaps", Schema = "Product")]
    public class ProductCategoryMap : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid ProductCategoryId { get; set; }
        public ProductCategory? ProductCategory { get; set; }

        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    [Table("ProductMediaItems", Schema = "Product")]
    public class ProductMedia : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public MediaType MediaType { get; set; } = MediaType.Image;
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? MimeType { get; set; }
        public string? AltText { get; set; }

        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    [Table("ProductBundleItems", Schema = "Product")]
    public class ProductBundleItem : BaseEntity
    {
        public Guid BundleProductId { get; set; }
        public Product? BundleProduct { get; set; }

        public Guid ChildProductId { get; set; }
        public Product? ChildProduct { get; set; }

        public Guid? ChildVariantId { get; set; }
        public ProductVariant? ChildVariant { get; set; }

        public decimal Quantity { get; set; } = 1;
        public bool IsOptional { get; set; }
        public string? RuleJson { get; set; }
    }
}