# Module Offering Prices API

Bu belge, modüllerin lisans paketine göre fiyatlandırılması özelliği için eklenen yeni endpoint'leri ve veri modellerini açıklar.

---

## Genel Bakış

Her `ProductModule` (modül) artık her `ProductLicenseOffering` (lisans paketi) için ayrı bir fiyata sahip olabilir. Bu fiyatlar `ProductModuleOfferingPrice` kaydı olarak tutulur.

**Temel ilişki:**
```
Product → ProductModule → ProductModuleOfferingPrice ← ProductLicenseOffering
```

---

## Base URL

```
/api/products/{productId}/modules/{moduleId}/offering-prices
```

---

## Endpoint'ler

### 1. Modülün tüm lisans paket fiyatlarını listele

```
GET /api/products/{productId}/modules/{moduleId}/offering-prices
```

**Response:** `200 OK`

```json
[
  {
    "id": "uuid",
    "productModuleId": "uuid",
    "moduleCode": "MUHASEBE",
    "moduleName": "Muhasebe Modülü",
    "productLicenseOfferingId": "uuid",
    "licenseOfferingName": "Standart Abonelik - Aylık",
    "price": 250.00,
    "currencyCode": "TRY",
    "isActive": true,
    "createdAt": "2026-06-15T10:00:00Z",
    "updatedAt": null
  }
]
```

---

### 2. Tek bir fiyat kaydını getir

```
GET /api/products/{productId}/modules/{moduleId}/offering-prices/{priceId}
```

**Response:** `200 OK` — yukarıdaki nesne formatında tek kayıt  
**Response:** `404 Not Found` — kayıt bulunamadığında

---

### 3. Yeni fiyat kaydı oluştur

```
POST /api/products/{productId}/modules/{moduleId}/offering-prices
```

**Request Body:**

```json
{
  "productLicenseOfferingId": "uuid",
  "price": 250.00,
  "currencyCode": "TRY",
  "isActive": true
}
```

> `productModuleId` URL'deki `moduleId` parametresinden otomatik alınır, body'de gönderilmesine gerek yoktur.

**Response:** `201 Created` — oluşturulan kayıt  
**Not:** Aynı modül + lisans paketi çifti için yalnızca bir kayıt oluşturulabilir (unique constraint).

---

### 4. Fiyat kaydını güncelle

```
PUT /api/products/{productId}/modules/{moduleId}/offering-prices/{priceId}
```

**Request Body:**

```json
{
  "price": 300.00,
  "currencyCode": "TRY",
  "isActive": true
}
```

**Response:** `204 No Content`  
**Response:** `404 Not Found`

---

### 5. Fiyat kaydını sil (soft delete)

```
DELETE /api/products/{productId}/modules/{moduleId}/offering-prices/{priceId}
```

**Response:** `204 No Content`  
**Response:** `404 Not Found`

---

## TypeScript Model Önerileri

```typescript
// Listeleme/okuma için
export interface ProductModuleOfferingPriceDto {
  id: string;
  productModuleId: string;
  moduleCode: string | null;
  moduleName: string | null;
  productLicenseOfferingId: string;
  licenseOfferingName: string | null;
  price: number;
  currencyCode: string;
  isActive: boolean;
  createdAt: string; // ISO 8601
  updatedAt: string | null;
}

// Oluşturma için
export interface CreateProductModuleOfferingPriceRequest {
  productLicenseOfferingId: string;
  price: number;
  currencyCode: string; // varsayılan: "TRY"
  isActive: boolean;    // varsayılan: true
}

// Güncelleme için
export interface UpdateProductModuleOfferingPriceRequest {
  price: number;
  currencyCode: string;
  isActive: boolean;
}
```

---

## Tipik Kullanım Akışı

1. **Lisans paketlerini al:**  
   `GET /api/products/{productId}/license-offerings`  
   → Dropdown için `{ id, name }` listesi.

2. **Modülü oluştur veya mevcut modülü seç:**  
   `GET /api/products/{productId}/modules`

3. **Her lisans paketi için fiyat tanımla:**  
   `POST /api/products/{productId}/modules/{moduleId}/offering-prices`  
   Body'de `productLicenseOfferingId` ve `price` gönder.

4. **Modülün tüm paket fiyatlarını görüntüle:**  
   `GET /api/products/{productId}/modules/{moduleId}/offering-prices`

---

## Notlar

- `ProductModule.additionalPrice` alanı hâlâ mevcuttur. Bu alan "genel/varsayılan ek ücret" olarak kalabilir. Lisans paketine özel fiyat tanımlanmışsa, frontend tarafında hangisinin kullanılacağına iş mantığına göre karar verilmelidir.
- `currencyCode` şu an için `"TRY"` varsayılanı ile gelir; ileride çoklu para birimi desteği eklenebilir.
- Silme işlemi fiziksel silme değil, soft delete (`IsDeleted = 1`) olarak çalışır.
