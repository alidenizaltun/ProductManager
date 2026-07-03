# Yazılım Ürünü — Lisanslama Altyapısı Değişiklikleri

## Özet

Bu doküman, yazılım ürünleri için eklenen lisans, modül ve fiyatlandırma kademesi altyapısının
`POST /api/products/full` endpoint'ini nasıl etkilediğini açıklar.

---

## Değişiklikten Önce — `CreateProductFullRequestDto`

```json
{
 "product": { ... },
 "attributeValues": [],
 "variants": [],
 "prices": [],
 "inventories": [],
 "mediaItems": [],
 "categoryMaps": [],
 "bundleItems": [],
 "supplierMaps": [],
 "inventoryTransactions": [],
 "inventoryReservations": [],
 "priceListItems": [],
 "physicalProfile": null,
 "softwareProfile": null,
 "serviceProfile": null,
 "subscriptionProfile": null
}
```

**Eksikler:**

- Ürünün modülleri (CRM modülü, Raporlama modülü vb.) tanımlanamıyordu.
- Kullanıcı sayısına göre kademeli fiyat (1-10 kullanıcı = 50 TL/kullanıcı, 11-50 = 40 TL) eklenemiyordu.
- Tek seferlik / abonelik / trial gibi farklı satış tipleri (license offering) aynı üründe birlikte modellenemiyordu.

---

## Değişiklikten Sonra — Eklenen Alanlar

```json
{
 "modules": [],
 "licenseOfferings": []
}
```

Bunlar `CreateProductFullRequestDto`'nun yeni opsiyonel alanlarıdır. Mevcut alanlar değişmemiştir.

---

## Yeni Tablo / Endpoint Özeti

| Tablo | Endpoint | Açıklama |
|---|---|---|
| `Product.ProductModules` | `GET/POST/PUT/DELETE /api/products/{id}/modules` | Ürün modülleri ve ek fiyatları |
| `Product.ProductLicenseOfferings` | `GET/POST/PUT/DELETE /api/products/{id}/license-offerings` | Satış tipine göre teklifler |

### `LicenseModel` Enum Değerleri

| Değer | Ad | Kullanım |
|---|---|---|
| 1 | Perpetual | Tek seferlik lisans |
| 2 | Subscription | Periyodik abonelik |
| 3 | UsageBased | Kullanım / istek başına ücret |
| 4 | SeatBased | Kullanıcı (seat) başına ücret |
| 5 | Trial | Deneme süresi |

---

## Örnek — `POST /api/products/full`

Aşağıdaki örnek bir proje yönetim yazılımını; tek seferlik, yıllık abonelik ve ücretsiz trial
seçenekleriyle, modülleriyle ve kullanıcı sayısına göre artan fiyat kademeleriyle oluşturur.

```json
{
 "product": {
 "productCode": "SW-PROJMGR-001",
 "name": "ProjeYönet Yazılımı",
 "shortDescription": "Kurumsal proje yönetim platformu",
 "description": "On-prem veya cloud kurulumlu, modüler yapıda proje yönetim yazılımı.",
 "kind": 3,
 "status": 1,
 "isActive": true,
 "isSellable": true,
 "isPurchasable": false,
 "trackInventory": false,
 "defaultCurrencyCode": "TRY",
 "tags": "proje,yönetim,saas,on-prem"
 },

 "softwareProfile": {
 "version": "3.2.1",
 "licenseModel": 4,
 "seatCount": null,
 "downloadUrl": "https://cdn.example.com/projeyonet/v3.2.1/setup.exe",
 "supportedPlatformsJson": "[\"Windows\",\"Linux\",\"Docker\"]",
 "systemRequirementsJson": "{\"minRam\":\"8GB\",\"minCpu\":\"4 core\",\"db\":\"MSSQL 2019+\"}",
 "releaseNotes": "v3.2.1 — Performans iyileştirmeleri ve API güvenlik güncellemeleri."
 },

 "licenseOfferings": [
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "licenseModel": 1,
 "name": "Kalıcı Lisans",
 "description": "Tek seferlik ödeme, ömür boyu kullanım. Yıllık bakım opsiyoneldir.",
 "basePrice": 25000.00,
 "currencyCode": "TRY",
 "autoRenew": false,
 "isActive": true,
 "sortOrder": 1
 },
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "licenseModel": 2,
 "name": "Yıllık Abonelik",
 "description": "Yıllık ödeme, her zaman güncel sürüm.",
 "basePrice": 8500.00,
 "currencyCode": "TRY",
 "billingPeriodUnit": 3,
 "billingPeriodValue": 1,
 "autoRenew": true,
 "gracePeriodDays": 14,
 "isActive": true,
 "sortOrder": 2
 },
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "licenseModel": 5,
 "name": "30 Günlük Ücretsiz Deneme",
 "description": "Kredi kartı gerekmez. Tüm özelliklere erişim.",
 "basePrice": 0.00,
 "currencyCode": "TRY",
 "trialDays": 30,
 "convertToOfferingId": null,
 "isActive": true,
 "sortOrder": 3
 }
 ],

 "modules": [
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "moduleCode": "MOD-GANTT",
 "name": "Gantt & Zaman Çizelgesi",
 "description": "Sürükle-bırak Gantt grafikleri, bağımlılık yönetimi.",
 "additionalPrice": 1500.00,
 "currencyCode": "TRY",
 "isOptional": true,
 "isActive": true,
 "sortOrder": 1
 },
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "moduleCode": "MOD-RAPORLAMA",
 "name": "Gelişmiş Raporlama",
 "description": "Özelleştirilebilir dashboard, PDF/Excel export.",
 "additionalPrice": 2000.00,
 "currencyCode": "TRY",
 "isOptional": true,
 "isActive": true,
 "sortOrder": 2
 },
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "moduleCode": "MOD-CLOUD",
 "name": "Cloud Kurulum (SaaS)",
 "description": "Yönetilen altyapı, otomatik yedekleme, %99.9 SLA.",
 "additionalPrice": 3000.00,
 "currencyCode": "TRY",
 "isOptional": true,
 "isActive": true,
 "sortOrder": 3
 },
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "moduleCode": "MOD-ONPREM",
 "name": "On-Premise Kurulum",
 "description": "Müşteri sunucusuna kurulum, kaynak kod teslim edilmez.",
 "additionalPrice": 0.00,
 "currencyCode": "TRY",
 "isOptional": false,
 "isActive": true,
 "sortOrder": 4
 }
 ],

 "prices": [
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "productVariantId": null,
 "priceType": 1,
 "amount": 8500.00,
 "compareAtAmount": 10000.00,
 "currencyCode": "TRY",
 "salesChannel": "web"
 }
 ],

 "attributeValues": [
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "attributeDefinitionId": "{{DEPLOYMENT_ATTR_DEF_ID}}",
 "valueText": "Cloud, On-Premise, Hybrid"
 },
 {
 "productId": "00000000-0000-0000-0000-000000000000",
 "attributeDefinitionId": "{{LANGUAGE_ATTR_DEF_ID}}",
 "valueText": "Türkçe, İngilizce"
 }
 ],

 "inventories": [],
 "inventoryTransactions": [],
 "inventoryReservations": [],
 "mediaItems": [],
 "categoryMaps": [],
 "bundleItems": [],
 "supplierMaps": [],
 "priceListItems": [],
 "physicalProfile": null,
 "serviceProfile": null,
 "subscriptionProfile": null
}
```

---

## Ayrı Endpoint Üzerinden Modül Ekleme

Ürün oluşturulduktan sonra tek tek de eklenebilir:

```
POST /api/products/{productId}/modules
```

```json
{
 "moduleCode": "MOD-SSO",
 "name": "SSO & LDAP Entegrasyonu",
 "description": "Active Directory, Okta ve Azure AD desteği.",
 "additionalPrice": 2500.00,
 "currencyCode": "TRY",
 "isOptional": true,
 "isActive": true,
 "sortOrder": 5
}
```

---

## Ayrı Endpoint Üzerinden License Offering Ekleme

```
POST /api/products/{productId}/license-offerings
```

```json
{
 "licenseModel": 2,
 "name": "Aylık Abonelik",
 "description": "Esnek aylık ödeme seçeneği.",
 "basePrice": 900.00,
 "currencyCode": "TRY",
 "billingPeriodUnit": 2,
 "billingPeriodValue": 1,
 "autoRenew": true,
 "gracePeriodDays": 7,
 "isActive": true,
 "sortOrder": 4
}
```

---

## `GET /api/products/{productId}/detail` — Değişen Response

`ProductDetailDto`'ya artık şu 3 alan da geliyor:

```json
{
 "id": "...",
 "productCode": "SW-PROJMGR-001",
 "...": "...",

 "modules": [
 {
 "id": "...",
 "moduleCode": "MOD-GANTT",
 "name": "Gantt & Zaman Çizelgesi",
 "additionalPrice": 1500.00,
 "currencyCode": "TRY",
 "isOptional": true,
 "isActive": true,
 "sortOrder": 1
 }
 ],

 "licenseOfferings": [
 {
 "id": "...",
 "licenseModel": 1,
 "name": "Kalıcı Lisans",
 "basePrice": 25000.00,
 "currencyCode": "TRY",
 "autoRenew": false,
 "isActive": true
 },
 {
 "id": "...",
 "licenseModel": 2,
 "name": "Yıllık Abonelik",
 "basePrice": 8500.00,
 "billingPeriodUnit": 3,
 "billingPeriodValue": 1,
 "autoRenew": true
 },
 {
 "id": "...",
 "licenseModel": 5,
 "name": "30 Günlük Ücretsiz Deneme",
 "basePrice": 0.00,
 "trialDays": 30
 }
 ]
}
```

---

## Veritabanı Migration

Aşağıdaki SQL scriptini çalıştırmanız gerekmektedir:

```sql
CREATE TABLE [Product].[ProductModules] (
 Id UNIQUEIDENTIFIER PRIMARY KEY,
 ProductId UNIQUEIDENTIFIER NOT NULL REFERENCES [Product].[Products](Id),
 ModuleCode NVARCHAR(100) NOT NULL,
 Name NVARCHAR(200) NOT NULL,
 Description NVARCHAR(MAX) NULL,
 AdditionalPrice DECIMAL(18,4) NOT NULL DEFAULT 0,
 CurrencyCode NVARCHAR(10) NOT NULL DEFAULT 'TRY',
 IsOptional BIT NOT NULL DEFAULT 1,
 IsActive BIT NOT NULL DEFAULT 1,
 SortOrder INT NOT NULL DEFAULT 0,
 CreatedAt DATETIME2 NOT NULL,
 UpdatedAt DATETIME2 NULL,
 IsDeleted BIT NOT NULL DEFAULT 0,
 DeletedAt DATETIME2 NULL
);

CREATE TABLE [Product].[ProductLicenseOfferings] (
 Id UNIQUEIDENTIFIER PRIMARY KEY,
 ProductId UNIQUEIDENTIFIER NOT NULL REFERENCES [Product].[Products](Id),
 LicenseModel INT NOT NULL,
 Name NVARCHAR(200) NOT NULL,
 Description NVARCHAR(MAX) NULL,
 BasePrice DECIMAL(18,4) NOT NULL DEFAULT 0,
 CurrencyCode NVARCHAR(10) NOT NULL DEFAULT 'TRY',
 BillingPeriodUnit INT NULL,
 BillingPeriodValue INT NULL,
 AutoRenew BIT NOT NULL DEFAULT 1,
 GracePeriodDays INT NULL,
 TrialDays INT NULL,
 ConvertToOfferingId UNIQUEIDENTIFIER NULL,
 MaxSeats INT NULL,
 ValidFrom DATETIME2 NULL,
 ValidTo DATETIME2 NULL,
 IsActive BIT NOT NULL DEFAULT 1,
 SortOrder INT NOT NULL DEFAULT 0,
 CreatedAt DATETIME2 NOT NULL,
 UpdatedAt DATETIME2 NULL,
 IsDeleted BIT NOT NULL DEFAULT 0,
 DeletedAt DATETIME2 NULL
);
