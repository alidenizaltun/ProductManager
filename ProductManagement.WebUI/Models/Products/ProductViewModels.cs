using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.WebUI.Models.Products
{
    public sealed class ProductIndexViewModel
    {
        [Display(Name = "Arama")]
        public string? Search { get; set; }

        [Display(Name = "Ürün Türü")]
        public int? Kind { get; set; }

        [Display(Name = "Durum")]
        public int? Status { get; set; }

        [Display(Name = "Aktiflik")]
        public bool? IsActive { get; set; }

        [Range(1, 200)]
        [Display(Name = "Kayıt Limiti")]
        public int Take { get; set; } = 50;

        public IReadOnlyList<ProductDto> Products { get; init; } = [];

        public IReadOnlyDictionary<int, string> KindLabels { get; init; } = new Dictionary<int, string>();
        public IReadOnlyDictionary<int, string> StatusLabels { get; init; } = new Dictionary<int, string>();

        public IReadOnlyList<SelectListItem> KindOptions { get; init; } = [];
        public IReadOnlyList<SelectListItem> StatusOptions { get; init; } = [];
        public IReadOnlyList<SelectListItem> ActivityOptions { get; init; } = [];
    }

    public sealed class ProductFormViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Ürün Kodu")]
        public string ProductCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Kısa Açıklama")]
        public string? ShortDescription { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Range(1, 99)]
        [Display(Name = "Ürün Türü")]
        public int Kind { get; set; } = 1;

        [Range(1, 99)]
        [Display(Name = "Durum")]
        public int Status { get; set; } = 1;

        [StringLength(100)]
        [Display(Name = "Marka")]
        public string? Brand { get; set; }

        [StringLength(100)]
        [Display(Name = "Üretici")]
        public string? Manufacturer { get; set; }

        [StringLength(64)]
        [Display(Name = "Barkod")]
        public string? Barcode { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Satılabilir")]
        public bool IsSellable { get; set; } = true;

        [Display(Name = "Satın Alınabilir")]
        public bool IsPurchasable { get; set; } = true;

        [Display(Name = "Stok Takibi")]
        public bool TrackInventory { get; set; } = true;

        [Required]
        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Para birimi 3 harfli kod olmalıdır (örn: TRY, USD).")]
        [Display(Name = "Varsayılan Para Birimi")]
        public string DefaultCurrencyCode { get; set; } = "TRY";

        [Range(typeof(decimal), "0", "100")]
        [Display(Name = "Vergi Oranı")]
        public decimal? TaxRate { get; set; }

        [StringLength(32)]
        [Display(Name = "Vergi Kodu")]
        public string? TaxCode { get; set; }

        [StringLength(512)]
        [Display(Name = "Etiketler")]
        public string? Tags { get; set; }

        [Display(Name = "Ek Teknik Metadata")]
        public string? MetadataJson { get; set; }

        public IReadOnlyList<SelectListItem> KindOptions { get; set; } = [];
        public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
        public IReadOnlyList<SelectListItem> CurrencyOptions { get; set; } = [];
    }
}
