using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.SystemManagement
{
    [Table("Integrations", Schema = "System")]
    public class Integration : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public string? ConfigJson { get; set; }
        public string? CredentialsProtected { get; set; }
        public bool IsSystemManaged { get; set; }
        public string? Description { get; set; }
        public DateTime? LastTestedAt { get; set; }
        public bool? LastTestSucceeded { get; set; }
        public string? LastTestMessage { get; set; }
    }
}
