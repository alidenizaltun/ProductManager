# Product Unit Conversions API

## Genel Bakış

`ProductUnitConversions` özelliği, bir ürünün birden fazla ölçü birimine sahip olmasına olanak tanır. Örneğin aynı ürün:

- Tedarikçiden **Koli** olarak alınabilir (Purchase)
- Depoda **Adet** olarak takip edilebilir (Stock)
- Müşteriye **Kutu** olarak satılabilir (Sales)

Bu dönüşümler `ProductUnitConversions` tablosunda tanımlanır ve `ConversionFactor` ile birimler arası oran tutulur.

**Base URL:** `/api/products/{productId}/unit-conversions`

> **NOT:** Bu endpoint henüz controller katmanına eklenmemiştir. Aşağıdaki dokümantasyon, entity/DTO yapısını ve önerilen API tasarımını açıklar. Backend entegrasyonu tamamlandığında güncellenecektir.

---

## Enum Değerleri

### `UnitRole` — Birim Rolü

| Değer | Ad | Açıklama |
|-------|-----|----------|
| `1` | `Sales` | Müşteriye satış birimi (ör: Kutu, Adet) |
| `2` | `Stock` | Depoda stok takip birimi (ör: Adet, Kg) |
| `3` | `Purchase` | Tedarikçiden alım birimi (ör: Koli, Palet) |

---

## Modeller

### `ProductUnitConversionDto` — Sunucudan dönen model

```json
{
 "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
 "productId": "a1b2c3d4-1234-5678-abcd-ef0123456789",
 "fromUnitDefinitionId": "unit-guid-1",
 "fromUnitDefinitionCode": "KOLI",
 "fromUnitDefinitionName": "Koli",
 "toUnitDefinitionId": "unit-guid-2",
 "toUnitDefinitionCode": "ADET",
 "toUnitDefinitionName": "Adet",
 "conversionFactor": 24,
 "fromUnitRole": 3,
 "isActive": true,
 "createdAt": "2026-05-18T10:00:00Z",
 "updatedAt": null
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `id` | `guid` | Dönüşüm kaydının benzersiz ID'si |
| `productId` | `guid` | Hangi ürüne ait olduğu |
| `fromUnitDefinitionId` | `guid` | Kaynak birim ID'si (ör: Koli'nin ID'si) |
| `fromUnitDefinitionCode` | `string` | Kaynak birimin kodu |
| `fromUnitDefinitionName` | `string` | Kaynak birimin adı |
| `toUnitDefinitionId` | `guid` | Hedef birim ID'si (ör: Adet'in ID'si) |
| `toUnitDefinitionCode` | `string` | Hedef birimin kodu |
| `toUnitDefinitionName` | `string` | Hedef birimin adı |
| `conversionFactor` | `number` | 1 FromUnit = kaç ToUnit (ör: 24) |
| `fromUnitRole` | `number` | Kaynak birimin rolü (1=Sales, 2=Stock, 3=Purchase) |
| `isActive` | `boolean` | Aktif mi? |
| `createdAt` | `datetime` | Oluşturulma tarihi (UTC) |
| `updatedAt` | `datetime?` | Son güncelleme tarihi (UTC) |

---

### `CreateProductUnitConversionRequestDto` — Oluşturma isteği

```json
{
 "fromUnitDefinitionId": "unit-guid-1",
 "toUnitDefinitionId": "unit-guid-2",
 "conversionFactor": 24,
 "fromUnitRole": 3,
 "isActive": true
}
```

| Alan | Tip | Zorunlu | Açıklama |
|------|-----|---------|----------|
| `fromUnitDefinitionId` | `guid` | ✅ | Kaynak birim (dönüşümün başladığı birim) |
| `toUnitDefinitionId` | `guid` | ✅ | Hedef birim (dönüşümün çevrildiği birim) |
| `conversionFactor` | `number` | ✅ | Oran (ör: 1 Koli = 24 Adet → `24`) |
| `fromUnitRole` | `number` | ✅ | 1=Sales, 2=Stock, 3=Purchase |
| `isActive` | `boolean` | ❌ | Varsayılan: `true` |

---

## Dönüşüm Mantığı

`conversionFactor`: **1 FromUnit = kaç ToUnit**

### Örnek Senaryo

Ürün: Kalem 
Birimler: Koli (24 Adet), Kutu (6 Adet)

| FromUnit | ToUnit | ConversionFactor | FromUnitRole |
|----------|--------|-----------------|--------------|
| Koli | Adet | 24 | 3 (Purchase) |
| Kutu | Adet | 6 | 1 (Sales) |

Yani:
- 1 Koli = 24 Adet
- 1 Kutu = 6 Adet
- Stok birimi (Stock) = Adet → `Products.UnitDefinitionId`'e atanır

---

## Mevcut API Yapısı (Unit Definitions)

Birim tanımlarını önce bu endpoint'ten almalısınız:

```
GET /api/unit-definitions
```

Dönen liste ürün formlarında dropdown olarak kullanılır.

### TypeScript Tipler

```ts
// types/unitConversion.ts

export type UnitRole = 1 | 2 | 3; // 1=Sales, 2=Stock, 3=Purchase

export const UNIT_ROLE_LABELS: Record<UnitRole, string> = {
 1: 'Satış Birimi',
 2: 'Stok Birimi',
 3: 'Satın Alma Birimi',
};

export interface ProductUnitConversionDto {
 id: string;
 productId: string;
 fromUnitDefinitionId: string;
 fromUnitDefinitionCode: string;
 fromUnitDefinitionName: string;
 toUnitDefinitionId: string;
 toUnitDefinitionCode: string;
 toUnitDefinitionName: string;
 conversionFactor: number;
 fromUnitRole: UnitRole;
 isActive: boolean;
 createdAt: string;
 updatedAt?: string | null;
}

export interface CreateProductUnitConversionRequest {
 fromUnitDefinitionId: string;
 toUnitDefinitionId: string;
 conversionFactor: number;
 fromUnitRole: UnitRole;
 isActive?: boolean;
}
```

---

## Ürün Formu Entegrasyonu

### `Products.UnitDefinitionId` vs `ProductUnitConversions`

| Alan | Kullanım | Açıklama |
|------|----------|----------|
| `Products.UnitDefinitionId` | Varsayılan stok/satış birimi | Tek birim kullanan ürünler için |
| `ProductUnitConversions` | Çoklu birim dönüşümü | Purchase/Sales/Stock farkı olan ürünler için |

### Ürün Tiplerine Göre Öneri

**Fiziksel ürünler (Physical):**
- `UnitDefinitionId` → stok takip birimi (ör: Adet)
- `UnitConversions` → alım birimi ve satış birimi tanımla (ör: Koli→Adet, Kutu→Adet)

**Yazılım/Hizmet/Abonelik:**
- `UnitDefinitionId` → opsiyonel (ör: Lisans, Kullanıcı)
- `UnitConversions` → genellikle gerekmez

---

## `ProductDetailDto`'daki Yeni Alan

`GET /api/products/{id}` endpoint'i `ProductDetailDto` döndürür. Bu modele ileride `unitConversions` alanı eklenecektir:

```json
{
 "id": "...",
 "productCode": "PRD-001",
 "name": "Kalem",
 "unitDefinitionId": "adet-guid",
 "unitDefinitionName": "Adet",
 "unitConversions": [
 {
 "id": "conv-guid-1",
 "fromUnitDefinitionName": "Koli",
 "toUnitDefinitionName": "Adet",
 "conversionFactor": 24,
 "fromUnitRole": 3
 },
 {
 "id": "conv-guid-2",
 "fromUnitDefinitionName": "Kutu",
 "toUnitDefinitionName": "Adet",
 "conversionFactor": 6,
 "fromUnitRole": 1
 }
 ]
}
```

---

## Frontend Bileşen Önerisi

### Ürün Formunda Birim Dönüşümü Bölümü

```tsx
// components/ProductUnitConversionForm.tsx

interface UnitConversionRow {
 fromUnitDefinitionId: string;
 toUnitDefinitionId: string;
 conversionFactor: number;
 fromUnitRole: 1 | 2 | 3;
}

// Kullanıcı bu tabloyu doldurur:
// | Kaynak Birim | Hedef Birim | Oran | Rol |
// |--------------|-------------|------|------------|
// | Koli (seç) | Adet (seç) | 24 | Alım Birimi|
// | Kutu (seç) | Adet (seç) | 6 | Satış Bir. |
```

### Birim Seçimi için Dropdown Hazırlama

```ts
import { unitDefinitionService } from '@/services/unitDefinitionService';

const units = await unitDefinitionService.getAll();
const unitOptions = units.data.map(u => ({
 value: u.id,
 label: `${u.name} (${u.code})`,
}));
// → [{ value: 'guid', label: 'Adet (ADET)' }, ...]
```

---

## Değişen / Eklenen Şema Özeti

| Değişiklik | Açıklama |
|-----------|----------|
| Yeni tablo: `Product.ProductUnitConversions` | Ürün başına n adet birim dönüşümü |
| Yeni enum: `UnitRole` (1/2/3) | Birimin rolünü tanımlar |
| Migration: `AddProductUnitConversions` | Tablo ve indeksler oluşturuldu |
| `Product` entity: `UnitConversions` navigation | Backend ilişkisi kuruldu |

---

## Önemli Kısıtlamalar

1. Aynı ürün için `FromUnit → ToUnit` çifti **benzersiz** olmalıdır (unique index).
2. `FromUnit` ve `ToUnit` farklı birimler olmalıdır (aynı birime dönüşüm anlamsızdır).
3. `ConversionFactor` pozitif bir sayı olmalıdır (0 veya negatif olamaz).

---

## HTTP Status Kodları

| Kod | Anlamı | Hangi endpoint |
|-----|--------|----------------|
| `200 OK` | Başarılı listeleme/getirme | GET |
| `201 Created` | Dönüşüm oluşturuldu | POST |
| `204 No Content` | Güncelleme/silme başarılı | PUT, DELETE |
| `404 Not Found` | Kayıt bulunamadı | GET (id), PUT, DELETE |
| `409 Conflict` | Aynı From→To çifti zaten var | POST |
