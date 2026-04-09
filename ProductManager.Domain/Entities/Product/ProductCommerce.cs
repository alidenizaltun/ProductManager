using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    [Table("ProductVariants", Schema = "Product")]
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public string Sku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Name { get; set; }
        public string? OptionValuesJson { get; set; }

        public decimal? AdditionalPrice { get; set; }
        public decimal? AdditionalCost { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [Table("ProductPrices", Schema = "Product")]
    public class ProductPrice : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public PriceType PriceType { get; set; } = PriceType.Sale;
        public decimal Amount { get; set; }
        public decimal? CompareAtAmount { get; set; }
        public string CurrencyCode { get; set; } = "TRY";

        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public string? SalesChannel { get; set; }
        public string? CustomerGroupCode { get; set; }
    }

    [Table("ProductInventories", Schema = "Product")]
    public class ProductInventory : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public string WarehouseCode { get; set; } = string.Empty;
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }

        public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

        public decimal? ReorderPoint { get; set; }
        public decimal? ReorderQuantity { get; set; }

        public InventoryPolicy InventoryPolicy { get; set; } = InventoryPolicy.TrackAndBlockWhenNegative;
    }
}
