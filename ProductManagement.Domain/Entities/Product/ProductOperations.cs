using ProductManagement.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Domain.Entities.Product
{
    [Table("ProductSuppliers", Schema = "Product")]
    public class ProductSupplier : BaseEntity
    {
        public string SupplierCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ProductSupplierMap> ProductMaps { get; set; } = new List<ProductSupplierMap>();
    }

    [Table("ProductSupplierMaps", Schema = "Product")]
    public class ProductSupplierMap : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid ProductSupplierId { get; set; }
        public ProductSupplier? ProductSupplier { get; set; }

        public string? SupplierProductCode { get; set; }
        public decimal? SupplierCost { get; set; }
        public int? LeadTimeInDays { get; set; }
        public decimal? MinOrderQuantity { get; set; }
        public bool IsPreferred { get; set; }
    }

    [Table("Warehouses", Schema = "Product")]
    public class Warehouse : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ProductInventory> Inventories { get; set; } = new List<ProductInventory>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
    }

    [Table("InventoryTransactions", Schema = "Product")]
    public class InventoryTransaction : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public Guid? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public InventoryTransactionType TransactionType { get; set; } = InventoryTransactionType.Adjustment;
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Note { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    [Table("InventoryReservations", Schema = "Product")]
    public class InventoryReservation : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public Guid? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }

        public decimal Quantity { get; set; }
        public string ReservationCode { get; set; } = string.Empty;
        public DateTime? ReservedUntil { get; set; }
        public InventoryReservationStatus Status { get; set; } = InventoryReservationStatus.Active;
        public string? SourceType { get; set; }
        public string? SourceId { get; set; }
    }

    [Table("ProductPriceLists", Schema = "Product")]
    public class ProductPriceList : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CurrencyCode { get; set; } = "TRY";
        public bool IsActive { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string? SalesChannel { get; set; }
        public string? CustomerGroupCode { get; set; }

        public ICollection<ProductPriceListItem> Items { get; set; } = new List<ProductPriceListItem>();
    }

    [Table("ProductPriceListItems", Schema = "Product")]
    public class ProductPriceListItem : BaseEntity
    {
        public Guid ProductPriceListId { get; set; }
        public ProductPriceList? ProductPriceList { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public decimal Amount { get; set; }
        public decimal? CompareAtAmount { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
    }
}
