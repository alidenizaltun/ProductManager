using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    public enum UnitRole
    {
        Sales = 1, // Müşteriye satış birimi (ör: Kutu)
        Stock = 2, // Depoda takip edilen birim (ör: Adet)
        Purchase = 3 // Tedarikçiden alım birimi (ör: Koli)
    }

    [Table("ProductUnitConversions", Schema = "Product")]
    public class ProductUnitConversion : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid FromUnitDefinitionId { get; set; }
        public UnitDefinition? FromUnitDefinition { get; set; }

        public Guid ToUnitDefinitionId { get; set; }
        public UnitDefinition? ToUnitDefinition { get; set; }

        public decimal ConversionFactor { get; set; }

        public UnitRole FromUnitRole { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [Table("ProductUnits", Schema = "Product")]
    public class ProductUnit : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid UnitDefinitionId { get; set; }
        public UnitDefinition? UnitDefinition { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public UnitRole Role { get; set; } = UnitRole.Sales;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
