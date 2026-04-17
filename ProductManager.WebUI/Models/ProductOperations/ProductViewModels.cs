using System.ComponentModel.DataAnnotations;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.WebUI.Models.ProductOperations;

public sealed class ProductFilterInput
{
    [Display(Name = "Arama")]
    public string? Search { get; set; }

    [Display(Name = "Tür")]
    public int? Kind { get; set; }

    [Display(Name = "Durum")]
    public int? Status { get; set; }

    [Display(Name = "Aktiflik")]
    public bool? IsActive { get; set; }

    [Display(Name = "Kayıt Sayısı")]
    [Range(1, 500)]
    public int Take { get; set; } = 100;
}

public sealed class ProductListPageViewModel
{
    public ProductFilterInput Filter { get; init; } = new();
    public IReadOnlyList<ProductDto> Products { get; init; } = Array.Empty<ProductDto>();
    public ProductFormViewModel CreateModal { get; init; } = new();
    public ProductFormViewModel EditModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
    public bool OpenEditModal { get; init; }
}

public sealed class ProductFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Ürün Kodu")]
    [Required]
    [StringLength(64)]
    public string ProductCode { get; set; } = string.Empty;

    [Display(Name = "Ürün Adı")]
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Kısa Açıklama")]
    [StringLength(512)]
    public string? ShortDescription { get; set; }

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Tür")]
    public int Kind { get; set; } = 1;

    [Display(Name = "Durum")]
    public int Status { get; set; } = 1;

    [Display(Name = "Marka")]
    [StringLength(128)]
    public string? Brand { get; set; }

    [Display(Name = "Üretici")]
    [StringLength(128)]
    public string? Manufacturer { get; set; }

    [Display(Name = "Barkod")]
    [StringLength(128)]
    public string? Barcode { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Satılabilir")]
    public bool IsSellable { get; set; } = true;

    [Display(Name = "Satın Alınabilir")]
    public bool IsPurchasable { get; set; } = true;

    [Display(Name = "Stok Takibi")]
    public bool TrackInventory { get; set; } = true;

    [Display(Name = "Varsayılan Para Birimi")]
    [Required]
    [StringLength(8)]
    public string DefaultCurrencyCode { get; set; } = "TRY";

    [Display(Name = "Ölçü Birimi")]
    [StringLength(32)]
    public string? UnitOfMeasure { get; set; }

    [Display(Name = "Vergi Oranı (%)")]
    [Range(0, 100)]
    public decimal? TaxRate { get; set; }

    [Display(Name = "Vergi Kodu")]
    [StringLength(64)]
    public string? TaxCode { get; set; }

    [Display(Name = "Etiketler")]
    [StringLength(512)]
    public string? Tags { get; set; }

    [Display(Name = "Ek Teknik Metadata")]
    public string? MetadataJson { get; set; }
}

public sealed class ProductDetailsViewModel
{
    public ProductDto Product { get; init; } = new();
    public IReadOnlyList<ProductCategoryDto> Categories { get; init; } = Array.Empty<ProductCategoryDto>();
    public IReadOnlyList<ProductCategoryMapDto> CategoryMaps { get; init; } = Array.Empty<ProductCategoryMapDto>();
    public IReadOnlyList<ProductAttributeDefinitionDto> AttributeDefinitions { get; init; } = Array.Empty<ProductAttributeDefinitionDto>();
    public IReadOnlyList<ProductAttributeValueDto> AttributeValues { get; init; } = Array.Empty<ProductAttributeValueDto>();
    public IReadOnlyList<ProductMediaDto> Media { get; init; } = Array.Empty<ProductMediaDto>();
    public IReadOnlyList<ProductBundleItemDto> BundleItems { get; init; } = Array.Empty<ProductBundleItemDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; init; } = Array.Empty<ProductVariantDto>();
    public IReadOnlyList<ProductPriceDto> Prices { get; init; } = Array.Empty<ProductPriceDto>();
    public IReadOnlyList<ProductInventoryDto> Inventories { get; init; } = Array.Empty<ProductInventoryDto>();
    public IReadOnlyList<ProductSupplierDto> Suppliers { get; init; } = Array.Empty<ProductSupplierDto>();
    public IReadOnlyList<ProductSupplierMapDto> SupplierMaps { get; init; } = Array.Empty<ProductSupplierMapDto>();
    public ProductPhysicalProfileDto? PhysicalProfile { get; init; }
    public ProductSoftwareProfileDto? SoftwareProfile { get; init; }
    public ProductServiceProfileDto? ServiceProfile { get; init; }
    public ProductSubscriptionProfileDto? SubscriptionProfile { get; init; }
}
