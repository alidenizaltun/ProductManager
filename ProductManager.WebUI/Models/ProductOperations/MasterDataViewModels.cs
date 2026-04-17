using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.WebUI.Models.ProductOperations;

public sealed class CategoryListPageViewModel
{
    public IReadOnlyList<ProductCategoryDto> Categories { get; init; } = Array.Empty<ProductCategoryDto>();
    public CategoryFormViewModel CreateModal { get; init; } = new();
    public CategoryFormViewModel EditModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
    public bool OpenEditModal { get; init; }
}

public sealed class CategoryFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Kategori Kodu")]
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Kategori Adı")]
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Üst Kategori")]
    public Guid? ParentCategoryId { get; set; }

    public IReadOnlyList<SelectListItem> ParentCategoryOptions { get; set; } = [];
}

public sealed class AttributeDefinitionListPageViewModel
{
    public IReadOnlyList<ProductAttributeDefinitionDto> Attributes { get; init; } = Array.Empty<ProductAttributeDefinitionDto>();
    public AttributeDefinitionFormViewModel CreateModal { get; init; } = new();
    public AttributeDefinitionFormViewModel EditModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
    public bool OpenEditModal { get; init; }
}

public sealed class AttributeDefinitionFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Anahtar")]
    [Required]
    [StringLength(64)]
    public string Key { get; set; } = string.Empty;

    [Display(Name = "Görünen Ad")]
    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [Display(Name = "Veri Tipi")]
    public int DataType { get; set; } = 1;

    [Display(Name = "Zorunlu")]
    public bool IsRequired { get; set; }

    [Display(Name = "Filtrelenebilir")]
    public bool IsFilterable { get; set; }

    [Display(Name = "Varyant Ekseni")]
    public bool IsVariantAxis { get; set; }

    [Display(Name = "İzinli Değerler")]
    public string? AllowedValuesJson { get; set; }

    [Display(Name = "Doğrulama Kuralı")]
    public string? ValidationRuleJson { get; set; }
}

public sealed class SupplierListPageViewModel
{
    public bool IncludeInactive { get; set; }
    public IReadOnlyList<ProductSupplierDto> Suppliers { get; init; } = Array.Empty<ProductSupplierDto>();
    public SupplierFormViewModel CreateModal { get; init; } = new();
    public SupplierFormViewModel EditModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
    public bool OpenEditModal { get; init; }
}

public sealed class SupplierFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Tedarikçi Kodu")]
    [Required]
    [StringLength(64)]
    public string SupplierCode { get; set; } = string.Empty;

    [Display(Name = "Tedarikçi Adı")]
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Vergi Numarası")]
    [StringLength(64)]
    public string? TaxNumber { get; set; }

    [Display(Name = "E-posta")]
    [StringLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Telefon")]
    [StringLength(64)]
    public string? Phone { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public sealed class WarehouseListPageViewModel
{
    public bool IncludeInactive { get; set; }
    public IReadOnlyList<WarehouseDto> Warehouses { get; init; } = Array.Empty<WarehouseDto>();
    public WarehouseFormViewModel CreateModal { get; init; } = new();
    public WarehouseFormViewModel EditModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
    public bool OpenEditModal { get; init; }
}

public sealed class WarehouseFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Depo Kodu")]
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Depo Adı")]
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Şehir")]
    [StringLength(128)]
    public string? City { get; set; }

    [Display(Name = "Ülke")]
    [StringLength(128)]
    public string? Country { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
