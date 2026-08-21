using ProductManagement.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Domain.Entities.Product
{
    /// <summary>
    /// Satış bölgesi tanımı (ör. Türkiye, Almanya, Marmara). Merkezi olarak bir kez
    /// tanımlanır, ürünlere <see cref="ProductRegion"/> ile bağlanır.
    /// </summary>
    [Table("Regions", Schema = "Product")]
    public class Region : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }

        public ICollection<ProductRegion> ProductRegions { get; set; } = new List<ProductRegion>();
    }

    /// <summary>
    /// Bir ürünün belirli bir bölgedeki satış koşulları: bölgeye özel para birimi
    /// ve KDV oranı. Ürün birden fazla bölgede satılabilir.
    /// </summary>
    [Table("ProductRegions", Schema = "Product")]
    public class ProductRegion : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid RegionId { get; set; }
        public Region? Region { get; set; }

        /// <summary>Bölgenin fiyat birimi (ISO 4217, ör. TRY, EUR).</summary>
        public string CurrencyCode { get; set; } = "TRY";

        /// <summary>Bölgeye özel KDV oranı (%). Boşsa ürünün <see cref="Product.TaxRate"/> değeri kullanılır.</summary>
        public decimal? TaxRate { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
