using ProductManagement.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Domain.Entities.Product
{
    [Table("UnitDefinitions", Schema = "Product")]
    public class UnitDefinition : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
