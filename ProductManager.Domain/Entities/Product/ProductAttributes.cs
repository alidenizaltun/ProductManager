using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    [Table("ProductAttributeDefinitions", Schema = "Product")]
    public class ProductAttributeDefinition : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public ProductAttributeDataType DataType { get; set; } = ProductAttributeDataType.Text;
        public bool IsRequired { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsVariantAxis { get; set; }
        public string? AllowedValuesJson { get; set; }
        public string? ValidationRuleJson { get; set; }
    }

    [Table("ProductAttributeValues", Schema = "Product")]
    public class ProductAttributeValue : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid AttributeDefinitionId { get; set; }
        public ProductAttributeDefinition? AttributeDefinition { get; set; }

        public string? ValueText { get; set; }
        public decimal? ValueNumber { get; set; }
        public bool? ValueBool { get; set; }
        public DateTime? ValueDate { get; set; }
        public string? ValueJson { get; set; }
    }
}
