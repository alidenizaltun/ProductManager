using System.ComponentModel.DataAnnotations;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.WebUI.Models.ProductOperations;

public sealed class InventoryTransactionFilterInput
{
    [Display(Name = "Ürün")]
    public Guid? ProductId { get; set; }

    [Display(Name = "Varyant")]
    public Guid? ProductVariantId { get; set; }

    [Display(Name = "Depo")]
    public Guid? WarehouseId { get; set; }

    [Display(Name = "Hareket Tipi")]
    public int? TransactionType { get; set; }

    [Display(Name = "Tarih (Başlangıç)")]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "Tarih (Bitiş)")]
    public DateTime? DateTo { get; set; }

    [Display(Name = "Kayıt Sayısı")]
    [Range(1, 500)]
    public int Take { get; set; } = 100;
}

public sealed class InventoryTransactionListPageViewModel
{
    public InventoryTransactionFilterInput Filter { get; init; } = new();
    public IReadOnlyList<InventoryTransactionDto> Transactions { get; init; } = Array.Empty<InventoryTransactionDto>();
    public InventoryTransactionFormViewModel CreateModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
}

public sealed class InventoryTransactionFormViewModel
{
    [Display(Name = "Ürün")]
    [Required]
    public Guid ProductId { get; set; }

    [Display(Name = "Varyant")]
    public Guid? ProductVariantId { get; set; }

    [Display(Name = "Depo")]
    public Guid? WarehouseId { get; set; }

    [Display(Name = "Hareket Tipi")]
    public int TransactionType { get; set; } = 1;

    [Display(Name = "Miktar")]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Display(Name = "Birim Maliyet")]
    public decimal? UnitCost { get; set; }

    [Display(Name = "Referans Tipi")]
    [StringLength(64)]
    public string? ReferenceType { get; set; }

    [Display(Name = "Referans Numarası")]
    [StringLength(128)]
    public string? ReferenceNumber { get; set; }

    [Display(Name = "Not")]
    public string? Note { get; set; }

    [Display(Name = "İşlem Zamanı")]
    public DateTime? OccurredAt { get; set; }

    public IReadOnlyList<LookupItemDto> Products { get; init; } = Array.Empty<LookupItemDto>();
    public IReadOnlyList<LookupItemDto> Warehouses { get; init; } = Array.Empty<LookupItemDto>();
}

public sealed class InventoryReservationFilterInput
{
    [Display(Name = "Ürün")]
    public Guid? ProductId { get; set; }

    [Display(Name = "Varyant")]
    public Guid? ProductVariantId { get; set; }

    [Display(Name = "Depo")]
    public Guid? WarehouseId { get; set; }

    [Display(Name = "Durum")]
    public int? Status { get; set; }

    [Display(Name = "Rezerve Bitiş (Min)")]
    public DateTime? ReservedUntilMin { get; set; }

    [Display(Name = "Rezerve Bitiş (Maks)")]
    public DateTime? ReservedUntilMax { get; set; }

    [Display(Name = "Kayıt Sayısı")]
    [Range(1, 500)]
    public int Take { get; set; } = 100;
}

public sealed class InventoryReservationListPageViewModel
{
    public InventoryReservationFilterInput Filter { get; init; } = new();
    public IReadOnlyList<InventoryReservationDto> Reservations { get; init; } = Array.Empty<InventoryReservationDto>();
    public InventoryReservationFormViewModel CreateModal { get; init; } = new();
    public InventoryReservationStatusFormViewModel UpdateStatusModal { get; init; } = new();
    public bool OpenCreateModal { get; init; }
    public bool OpenUpdateStatusModal { get; init; }
}

public sealed class InventoryReservationFormViewModel
{
    [Display(Name = "Ürün")]
    [Required]
    public Guid ProductId { get; set; }

    [Display(Name = "Varyant")]
    public Guid? ProductVariantId { get; set; }

    [Display(Name = "Depo")]
    public Guid? WarehouseId { get; set; }

    [Display(Name = "Miktar")]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Display(Name = "Rezervasyon Kodu")]
    [Required]
    [StringLength(64)]
    public string ReservationCode { get; set; } = string.Empty;

    [Display(Name = "Rezerve Bitiş")]
    public DateTime? ReservedUntil { get; set; }

    [Display(Name = "Durum")]
    public int Status { get; set; } = 1;

    [Display(Name = "Kaynak Tipi")]
    [StringLength(64)]
    public string? SourceType { get; set; }

    [Display(Name = "Kaynak No")]
    [StringLength(128)]
    public string? SourceId { get; set; }

    public IReadOnlyList<LookupItemDto> Products { get; init; } = Array.Empty<LookupItemDto>();
    public IReadOnlyList<LookupItemDto> Warehouses { get; init; } = Array.Empty<LookupItemDto>();
}

public sealed class InventoryReservationStatusFormViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Display(Name = "Durum")]
    public int Status { get; set; }

    [Display(Name = "Rezerve Bitiş")]
    public DateTime? ReservedUntil { get; set; }
}
