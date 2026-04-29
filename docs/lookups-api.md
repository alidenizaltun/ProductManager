# Lookup API — Kullanım Özeti

Ürün, kategori, depo, tedarikçi ve fiyat listesi seçimleri için hafif liste döner. Tüm yanıtlarda öğe şekli aynıdır: `id` (GUID) ve `name` (görünen metin).

## Temel adres

API kökünüz (ör. `https://localhost:7001`) ile birleştirin:

| Yöntem | Yol | Açıklama |
|--------|-----|----------|
| `GET` | `/api/lookups` | Tüm lookup’ları tek istekte döner |
| `GET` | `/api/lookups/products` | Ürün listesi |
| `GET` | `/api/lookups/categories` | Kategori listesi |
| `GET` | `/api/lookups/warehouses` | Depo listesi |
| `GET` | `/api/lookups/suppliers` | Tedarikçi listesi |
| `GET` | `/api/lookups/price-lists` | Fiyat listesi |

## Sorgu parametreleri

- **`includeInactive`** (`bool`, varsayılan `false`): Ürün, depo, tedarikçi ve fiyat listesi endpoint’lerinde geçerlidir. `true` iken pasif kayıtlar da listelenir. Kategorilerde bu parametre yoktur.

Örnek:

```http
GET /api/lookups?includeInactive=true
GET /api/lookups/warehouses?includeInactive=true
```

## Yanıt biçimleri

### Tek liste (`LookupItemDto[]`)

```json
[
  { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "URN-001 - Örnek Ürün" },
  { "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7", "name": "MERKEZ - Ana Depo" }
```

### Toplu (`GET /api/lookups`)

```json
{
  "products": [ { "id": "...", "name": "..." } ],
  "categories": [ { "id": "...", "name": "..." } ],
  "warehouses": [ { "id": "...", "name": "..." } ],
  "suppliers": [ { "id": "...", "name": "..." } ],
  "priceLists": [ { "id": "...", "name": "..." } ]
}
```

## Frontend’de kullanım (özet)

1. Sayfa açılışında veya form görünmeden önce ilgili `GET` isteğini atın (tercihen tek bir servis modülünden).
2. Gelen `id` / `name` ile `<select>` seçeneklerini doldurun; seçilen değer olarak yalnızca `id` saklayın.
3. Ana istekte (ör. ürün oluşturma, stok, kategori eşlemesi) API’nin beklediği alanlara bu GUID’leri yazın; `name` gönderilmez.

Örnek (React + servis katmanı fikri):

```ts
// lookupService.ts
const base = import.meta.env.VITE_API_URL;

export async function fetchWarehouses(includeInactive = false) {
  const q = includeInactive ? "?includeInactive=true" : "";
  const res = await fetch(`${base}/api/lookups/warehouses${q}`);
  if (!res.ok) throw new Error("Depo listesi alınamadı");
  return res.json() as Promise<{ id: string; name: string }[]>;
}
```

## cURL örnekleri

```bash
curl -s "https://localhost:7001/api/lookups/categories"
curl -s "https://localhost:7001/api/lookups/warehouses?includeInactive=false"
curl -s "https://localhost:7001/api/lookups"
```

Not: Kimlik doğrulama API’nizde açıksa, isteklere geçerli `Authorization` başlığını eklemeniz gerekir.
