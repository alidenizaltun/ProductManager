using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.SystemManagement
{
    [Table("SystemSettings", Schema = "System")]
    public class SystemSetting : BaseEntity
    {
        public string Category { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string DataType { get; set; } = "String";
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEditable { get; set; } = true;
        public int SortOrder { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
