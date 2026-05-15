# Unit Definitions API

Birim tanımları (ör. Adet, Kg, Litre) oluşturmak, güncellemek, listelemek ve silmek için kullanılan endpoint grubudur.

**Base URL:** `/api/unit-definitions`

---

## Modeller

### `UnitDefinitionDto` — Sunucudan dönen model

```json
{
 "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
 "code": "KG",
 "name": "Kilogram",
 "description": "Ağırlık birimi",
 "isActive": true,
 "sortOrder": 1,
 "createdAt": "2024-01-15T10:00:00Z",
 "updatedAt": null
}
```

| Alan | Tip | Açıklama |
|------|-----|----------|
| `id` | `guid` | Benzersiz kimlik |
| `code` | `string` | Kısa kod (ör. `KG`, `ADET`) |
| `name` | `string` | Görünen ad |
| `description` | `string?` | Opsiyonel açıklama |
| `isActive` | `boolean` | Aktif/pasif durumu |
| `sortOrder` | `number` | Sıralama önceliği |
| `createdAt` | `datetime` | Oluşturulma tarihi (UTC) |
| `updatedAt` | `datetime?` | Son güncelleme tarihi (UTC), henüz güncellenmemişse `null` |

---

### `CreateUnitDefinitionRequestDto` — Oluşturma isteği

```json
{
 "code": "KG",
 "name": "Kilogram",
 "description": "Ağırlık birimi",
 "isActive": true,
 "sortOrder": 1
}
```

| Alan | Tip | Zorunlu | Açıklama |
|------|-----|---------|----------|
| `code` | `string` | ✅ | Kısa kod |
| `name` | `string` | ✅ | Görünen ad |
| `description` | `string?` | ❌ | Açıklama |
| `isActive` | `boolean` | ❌ | Varsayılan: `true` |
| `sortOrder` | `number` | ❌ | Varsayılan: `0` |

---

### `UpdateUnitDefinitionRequestDto` — Güncelleme isteği

Alanlar `CreateUnitDefinitionRequestDto` ile aynıdır. `code` ve `name` zorunludur.

```json
{
 "code": "KG",
 "name": "Kilogram",
 "description": "Güncellenmiş açıklama",
 "isActive": false,
 "sortOrder": 5
}
```

---

## Endpointler

### `GET /api/unit-definitions` — Tüm birimleri listele

Aktif birim tanımlarını döner. `includeInactive=true` ile pasif kayıtlar da dahil edilebilir.

**Query Parameters:**

| Parametre | Tip | Varsayılan | Açıklama |
|-----------|-----|------------|----------|
| `includeInactive` | `boolean` | `false` | Pasif kayıtları da getir |

**Örnek İstek:**
```
GET /api/unit-definitions
GET /api/unit-definitions?includeInactive=true
```

**Başarılı Yanıt — `200 OK`:**
```json
[
 {
 "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
 "code": "ADET",
 "name": "Adet",
 "description": null,
 "isActive": true,
 "sortOrder": 0,
 "createdAt": "2024-01-01T00:00:00Z",
 "updatedAt": null
 },
 {
 "id": "7cb96a12-3348-4891-c4fd-8e174g77bgb7",
 "code": "KG",
 "name": "Kilogram",
 "description": "Ağırlık birimi",
 "isActive": true,
 "sortOrder": 1,
 "createdAt": "2024-01-01T00:00:00Z",
 "updatedAt": null
 }
]
```

---

### `GET /api/unit-definitions/{id}` — Tekil birim getir

**Path Parameters:**

| Parametre | Tip | Açıklama |
|-----------|-----|----------|
| `id` | `guid` | Birimin benzersiz ID'si |

**Örnek İstek:**
```
GET /api/unit-definitions/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Başarılı Yanıt — `200 OK`:** `UnitDefinitionDto` objesi döner.

**Hata Yanıtı — `404 Not Found`:** Kayıt bulunamadığında.

---

### `POST /api/unit-definitions` — Yeni birim oluştur

**Request Body:** `CreateUnitDefinitionRequestDto`

**Örnek İstek:**
```http
POST /api/unit-definitions
Content-Type: application/json

{
 "code": "LT",
 "name": "Litre",
 "description": "Hacim birimi",
 "isActive": true,
 "sortOrder": 2
}
```

**Başarılı Yanıt — `201 Created`:**

`Location` header'ında yeni kaydın URL'i döner, body'de oluşturulan `UnitDefinitionDto` bulunur.

```
Location: /api/unit-definitions/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

### `PUT /api/unit-definitions/{id}` — Birim güncelle

Kaydın tüm alanları güncellenir (full replace).

**Path Parameters:**

| Parametre | Tip | Açıklama |
|-----------|-----|----------|
| `id` | `guid` | Güncellenecek birimin ID'si |

**Request Body:** `UpdateUnitDefinitionRequestDto`

**Örnek İstek:**
```http
PUT /api/unit-definitions/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
 "code": "LT",
 "name": "Litre",
 "description": "Güncellendi",
 "isActive": false,
 "sortOrder": 10
}
```

**Başarılı Yanıt — `204 No Content`**

**Hata Yanıtı — `404 Not Found`:** Kayıt bulunamadığında.

---

### `DELETE /api/unit-definitions/{id}` — Birim sil

**Path Parameters:**

| Parametre | Tip | Açıklama |
|-----------|-----|----------|
| `id` | `guid` | Silinecek birimin ID'si |

**Örnek İstek:**
```
DELETE /api/unit-definitions/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Başarılı Yanıt — `204 No Content`**

**Hata Yanıtı — `404 Not Found`:** Kayıt bulunamadığında.

---

## Frontend Kullanım Rehberi

### API Service (örnek)

```ts
// services/unitDefinitionService.ts

import axios from 'axios';

const BASE = '/api/unit-definitions';

export interface UnitDefinitionDto {
 id: string;
 code: string;
 name: string;
 description?: string | null;
 isActive: boolean;
 sortOrder: number;
 createdAt: string;
 updatedAt?: string | null;
}

export interface CreateUnitDefinitionRequest {
 code: string;
 name: string;
 description?: string;
 isActive?: boolean;
 sortOrder?: number;
}

export type UpdateUnitDefinitionRequest = CreateUnitDefinitionRequest;

export const unitDefinitionService = {
 getAll: (includeInactive = false) =>
 axios.get<UnitDefinitionDto[]>(BASE, { params: { includeInactive } }),

 getById: (id: string) =>
 axios.get<UnitDefinitionDto>(`${BASE}/${id}`),

 create: (data: CreateUnitDefinitionRequest) =>
 axios.post<UnitDefinitionDto>(BASE, data),

 update: (id: string, data: UpdateUnitDefinitionRequest) =>
 axios.put(`${BASE}/${id}`, data),

 delete: (id: string) =>
 axios.delete(`${BASE}/${id}`),
};
```

### Dropdown / Select için kullanım

Ürün formlarında birim seçimi için `/api/unit-definitions` endpoint'i kullanılır. Yalnızca aktif kayıtlar getirilir (varsayılan davranış).

```ts
const { data } = await unitDefinitionService.getAll();
// data: UnitDefinitionDto[]
// Dropdown için: { value: item.id, label: item.name }
```

### Pasif kayıtları da görmek gerektiğinde

Yönetim ekranlarında tüm kayıtları listelemek için:

```ts
const { data } = await unitDefinitionService.getAll(true);
```

### Ürün / Lisans modellerinde referans

`UnitDefinitionId` alanı `ProductDto`, `SoftwareLicenseDto` gibi modellerde kullanılır. Bu alana atanacak değer, `UnitDefinitionDto.id` (guid) olmalıdır.

```ts
// Örnek: Ürün oluştururken birim seçimi
const payload = {
 name: 'Yeni Ürün',
 unitDefinitionId: selectedUnit.id, // guid
 // ...diğer alanlar
};
```

---

## HTTP Status Kodları Özeti

| Kod | Anlamı | Hangi endpoint |
|-----|--------|----------------|
| `200 OK` | Başarılı listeleme/getirme | GET |
| `201 Created` | Kayıt oluşturuldu | POST |
| `204 No Content` | Güncelleme/silme başarılı | PUT, DELETE |
| `404 Not Found` | Kayıt bulunamadı | GET (id), PUT, DELETE |
