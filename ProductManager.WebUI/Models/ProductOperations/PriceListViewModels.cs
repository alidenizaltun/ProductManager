using System.ComponentModel.DataAnnotations;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.WebUI.Models.ProductOperations;

public sealed class PriceListListPageViewModel
{
    public bool IncludeInactive { get; set; }
    public IReadOnlyList<ProductPriceListDto> PriceLists { get; init; } = Array.Empty<ProductPriceListDto>();
    public PriceListFormViewModel CreateModal { get; init; } = new();
    public PriceListFormViewModel EditModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
    public bool OpenEditModal { get; init; }
}

public sealed class PriceListFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Fiyat Listesi Kodu")]
    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Fiyat Listesi Adı")]
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Para Birimi")]
    [Required]
    [StringLength(8)]
    public string CurrencyCode { get; set; } = "TRY";

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Geçerlilik Başlangıcı")]
    public DateTime? ValidFrom { get; set; }

    [Display(Name = "Geçerlilik Bitişi")]
    public DateTime? ValidTo { get; set; }

    [Display(Name = "Satış Kanalı")]
    [StringLength(64)]
    public string? SalesChannel { get; set; }

    [Display(Name = "Müşteri Grubu Kodu")]
    [StringLength(64)]
    public string? CustomerGroupCode { get; set; }
}

public sealed class PriceListDetailsPageViewModel
{
    public ProductPriceListDto PriceList { get; init; } = new();
    public IReadOnlyList<ProductPriceListItemDto> Items { get; init; } = Array.Empty<ProductPriceListItemDto>();
    public IReadOnlyList<ProductDto> Products { get; init; } = Array.Empty<ProductDto>();
    public PriceListItemFormViewModel NewItem { get; set; } = new();
    public bool OpenNewItemModal { get; init; }
}

public sealed class PriceListItemFormViewModel
{
    [Display(Name = "Ürün")]
    [Required]
    public Guid ProductId { get; set; }

    [Display(Name = "Varyant")]
    public Guid? ProductVariantId { get; set; }

    [Display(Name = "Tutar")]
    [Range(0.0001, double.MaxValue)]
    public decimal Amount { get; set; }

    [Display(Name = "Karşılaştırma Tutarı")]
    public decimal? CompareAtAmount { get; set; }

    [Display(Name = "Minimum Miktar")]
    public int? MinQuantity { get; set; }

    [Display(Name = "Maksimum Miktar")]
    public int? MaxQuantity { get; set; }
}
