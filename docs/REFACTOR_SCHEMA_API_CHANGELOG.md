# Şema refaktörü: API ve veritabanı değişiklik özeti

Bu doküman **migration `20260514115757_RefactorDatabaseSchema`** ile gelen veritabanı ve HTTP API uyumluluk değişikliklerini özetler.

## Veritabanı

### Yeni tablo

| Tablo | Açıklama |
|-------|-----------|
| `Product.UnitDefinitions` | Ölçü birimi lookup kayıtları (`Code`, `Name`, `Description`, `IsActive`, `SortOrder`, soft delete alanları). `Code` benzersiz. |

### `Product.Products`

| Değişiklik | Detay |
|------------|--------|
| **Kaldırıldı** | `UnitOfMeasure` (nvarchar) |
| **Eklendi** | `UnitDefinitionId` (uniqueidentifier, NULL), FK → `UnitDefinitions` (`Restrict`) |

### `Product.ProductSoftwareProfiles`

| Değişiklik | Detay |
|------------|--------|
| **Kaldırıldı** | `LicenseModel`, `SeatCount` |

Lisans modeli ve koltuk bilgisi yalnızca **`ProductLicenseOfferings`** üzerinden yönetilir.

### `Product.SoftwarePricingTiers`

`SoftwarePricingTiers` tamamen kaldırıldı. Yazılım fiyatlandırması artık `ProductPricingRules` üzerinden yönetilir; lisans teklifi taban fiyatı `ProductLicenseOfferings.BasePrice` alanında kalır.

---

## HTTP endpoint değişiklikleri

### Yeni endpoint’ler

| Method | Route | Açıklama |
|--------|-------|----------|
| GET | `/api/unit-definitions` | Tüm birim tanımları (`includeInactive` query). |
| GET | `/api/unit-definitions/{id}` | Tek kayıt. |
| POST | `/api/unit-definitions` | `CreateUnitDefinitionRequestDto`. |
| PUT | `/api/unit-definitions/{id}` | `UpdateUnitDefinitionRequestDto`. |
| DELETE | `/api/unit-definitions/{id}` | Soft delete (repository mantığına uygun). |
| GET | `/api/lookups/unit-definitions` | Lookup listesi (`LookupItemDto`: Id + Name). |

### Güncellenen endpoint’ler (davranış / gövde şeması)

| Endpoint | Ne değişti |
|----------|------------|
| **GET** `/api/lookups` (`ProductReferenceLookupsDto`) | Yanıtta **`unitDefinitions`** listesi eklendi (`LookupItemDto[]`). |
| **GET** `/api/products` | Liste öğelerinde **`unitOfMeasure` yok**; **`unitDefinitionId`**, okumada **`unitDefinitionName`** (JOIN ile). |
| **GET** `/api/products/{productId}` | Aynı ürün alanları. |
| **GET** ürün detayı (projede kullanılan detay endpoint’i) | **`ProductDetailDto`**: üst düzey ürün için **`unitDefinitionId`** / **`unitDefinitionName`**; fiyatlandırma kuralları `pricingRules` üzerinden döner. |
| **POST** `/api/products`, **PUT** `/api/products/{id}` | İstek gövdelerinde **`unitOfMeasure` yok**; **`unitDefinitionId`** (nullable Guid). |
| **POST** tam ürün oluşturma / **PUT** tam güncelleme | İç **`product`** nesnesinde **`unitDefinitionId`**; fiyat kuralları için **`pricingRules`** kullanılır. |
| **GET** `/api/products/{productId}/profiles/software` | **`licenseModel`**, **`seatCount` kaldırıldı**. |
| **PUT** `/api/products/{productId}/profiles/software` | **`UpsertProductSoftwareProfileRequestDto`**: yalnızca `version`, `downloadUrl`, `supportedPlatformsJson`, `systemRequirementsJson`, `releaseNotes`. |

`ProductLicenseOffering` CRUD endpoint’leri (**`/api/products/{productId}/license-offerings`**) aynı kalır; **`licenseModel`** burada durmaya devam eder.

---

## DTO alan özeti

### Ürün (`ProductDto`, `ProductDetailDto`, `CreateProductRequestDto`, `UpdateProductRequestDto`)

| Eski | Yeni |
|------|------|
| `unitOfMeasure` (string?) | `unitDefinitionId` (Guid?) |
| — | Okuma tarafında: `unitDefinitionName` (string?, liste/detayda JOIN) |

### Yazılım profili (`ProductSoftwareProfileDto`, `UpsertProductSoftwareProfileRequestDto`)

| Kaldırılan |
|------------|
| `licenseModel` |
| `seatCount` |

### Lookup aggregate (`ProductReferenceLookupsDto`)

- **`unitDefinitions`**: `LookupItemDto[]`

### Yeni DTO’lar (birim tanımı)

- `UnitDefinitionDto`
- `CreateUnitDefinitionRequestDto` (`code`, `name`, …)
- `UpdateUnitDefinitionRequestDto`

---

## İstemci tarafı (Web UI)

- Ürün formlarında metin **`UnitOfMeasure`** yerine **`UnitDefinitionId`** (select); seçenekler API lookup/crud ile doldurulmalı (`/api/lookups/unit-definitions` veya tam liste).
- Detay sayfasında birim gösterimi **`unitDefinitionName`** üzerinden.

---

## Komutlar

```bash
dotnet ef database update --project ProductManagement.EFCore --startup-project ProductManagement.API
```

Migration ilk denemede FK hatası verdiyse, güncel migration dosyasında tier satırlarını düzelten SQL ile yeniden **`database update`** çalıştırılmalıdır (bu repoda düzeltme işlenmiştir).

---

## Geriye dönük uyumluluk

- Eski istemciler **`unitOfMeasure`** gönderiyorsa API artık bu alanı kabul etmez; **`unitDefinitionId`** kullanılmalıdır.
- Yazılım profili için **`licenseModel`** / **`seatCount`** kullanımı kaldırıldı; lisans bilgisi **`license-offerings`** ile devam eder.
- Tier oluştururken önce ilgili ürün için en az bir **`ProductLicenseOffering`** tanımlı olmalıdır; aksi halde tier oluşturulamaz.
