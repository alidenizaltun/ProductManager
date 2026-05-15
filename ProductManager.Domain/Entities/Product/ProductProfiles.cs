using ProductManager.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManager.Domain.Entities.Product
{
    [Table("ProductPhysicalProfiles", Schema = "Product")]
    public class ProductPhysicalProfile : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public decimal? Weight { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Length { get; set; }

        public bool RequiresShipping { get; set; } = true;
        public bool IsFragile { get; set; }
        public bool IsHazardous { get; set; }
        public bool RequiresSerialNumber { get; set; }

        public int? WarrantyInMonths { get; set; }
    }

    [Table("ProductSoftwareProfiles", Schema = "Product")]
    public class ProductSoftwareProfile : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public string? Version { get; set; }
        public string? DownloadUrl { get; set; }
        public string? SupportedPlatformsJson { get; set; }
        public string? SystemRequirementsJson { get; set; }
        public string? ReleaseNotes { get; set; }
    }

    [Table("ProductServiceProfiles", Schema = "Product")]
    public class ProductServiceProfile : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public ServiceDeliveryMode DeliveryMode { get; set; } = ServiceDeliveryMode.Remote;
        public int? DurationInMinutes { get; set; }
        public int? MaxConcurrentBooking { get; set; }

        public string? ServiceAreaJson { get; set; }
        public string? ServiceLevelAgreementJson { get; set; }
        public string? CapacityRuleJson { get; set; }
    }

    [Table("ProductSubscriptionProfiles", Schema = "Product")]
    public class ProductSubscriptionProfile : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public BillingPeriodUnit BillingPeriodUnit { get; set; } = BillingPeriodUnit.Month;
        public int BillingPeriodValue { get; set; } = 1;

        public int? TrialDays { get; set; }
        public bool AutoRenew { get; set; } = true;
        public int? GracePeriodDays { get; set; }

        public string? CancellationPolicy { get; set; }
        public string? SubscriptionRulesJson { get; set; }
    }
}
