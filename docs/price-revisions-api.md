# Price Revisions API (Zam Yönetimi)

Toplu fiyat değişikliği doğrudan bir `UPDATE` değil, bir **belge**dir: kapsamı seçilir,
önizlenir, satır satır düzeltilir, onaylanır, uygulanır ve gerekirse geri alınır.

## Akış

```
Taslak → Önizlendi → Onay Bekliyor → Onaylandı → Uygulandı → (Geri Alındı)
                          ↓
                      Reddedildi → Önizlendi
```

| `status` | Anlam | Bu durumda ne yapılabilir |
| --- | --- | --- |
| `1` Taslak | Yeni oluşturuldu | Düzenle, kapsam ekle, önizle, iptal et, sil |
| `2` Önizlendi | Satırlar üretildi | Satır düzenle, onaya gönder, yeniden önizle, sil |
| `3` Onay Bekliyor | Onaycıda | Onayla, reddet, iptal et, satır düzenle |
| `4` Onaylandı | Uygulanmayı bekliyor | Uygula |
| `5` Uygulandı | Fiyatlar değişti | Geri al |
| `6` Geri Alındı | Eski fiyatlara dönüldü | — |
| `7` Reddedildi | Onaycı geri gönderdi | Düzenle, yeniden önizle, onaya gönder |
| `8` İptal | Vazgeçildi | — |

Durum geçişleri servis katmanında denetlenir; yanlış sıradaki bir istek `409 Conflict`
döner. Revizyonun başlığı ya da kapsamı değiştirilirse önizleme satırları silinir ve
durum taslağa döner — eski satırlar artık doğru olmadığı için.

## Endpoints

| Metot | Yol | İzin |
| --- | --- | --- |
| GET | `/api/price-revisions?status=` | `Pricing.Revisions.View` |
| GET | `/api/price-revisions/{id}` | `Pricing.Revisions.View` |
| POST | `/api/price-revisions` | `Pricing.Revisions.Manage` |
| PUT · DELETE | `/api/price-revisions/{id}` | `Pricing.Revisions.Manage` |
| POST | `/api/price-revisions/{id}/scopes` | `Pricing.Revisions.Manage` |
| DELETE | `/api/price-revisions/{id}/scopes/{scopeId}` | `Pricing.Revisions.Manage` |
| POST | `/api/price-revisions/{id}/preview` | `Pricing.Revisions.Manage` |
| GET | `/api/price-revisions/{id}/lines` | `Pricing.Revisions.View` |
| PATCH | `/api/price-revisions/{id}/lines/{lineId}` | `Pricing.Revisions.Manage` |
| POST | `/api/price-revisions/{id}/submit` | `Pricing.Revisions.Manage` |
| POST | `/api/price-revisions/{id}/approve` | `Pricing.Revisions.Approve` |
| POST | `/api/price-revisions/{id}/reject` | `Pricing.Revisions.Approve` |
| POST | `/api/price-revisions/{id}/cancel` | `Pricing.Revisions.Manage` |
| POST | `/api/price-revisions/{id}/apply` | `Pricing.Revisions.Apply` |
| POST | `/api/price-revisions/{id}/rollback` | `Pricing.Revisions.Apply` |

`Manage`, `Approve` ve `Apply` bilinçli olarak ayrıdır: hazırlayan, onaylayan ve uygulayan
çoğu kurumda aynı kişi değildir.

## Revizyon oluşturma

`POST /api/price-revisions`

```json
{
  "name": "2026 Temmuz genel zam",
  "adjustmentType": 1,
  "value": 15,
  "roundingMode": 2,
  "roundingStep": 0.01,
  "currencyCode": "TRY"
}
```

| `adjustmentType` | Hesap |
| --- | --- |
| `1` Yüzde | `eski * (1 + değer / 100)` |
| `2` Sabit tutar | `eski + değer` |
| `3` Sabit değere çek | `değer` |
| `4` Çarpan | `eski * değer` |

| `roundingMode` | Davranış |
| --- | --- |
| `1` Yok | 4 basamağa yuvarlanır |
| `2` Yuvarla · `3` Yukarı · `4` Aşağı | `roundingStep` adımına göre (0,01 / 0,5 / 1 / 10) |

> **Para birimi kuralı.** `adjustmentType` `2` ya da `3` ise `currencyCode` zorunludur.
> Kapsamda hem TRY hem USD fiyat varken "5 ekle" demek anlamsızdır; oransal türlerde
> (yüzde, çarpan) bu kısıt yoktur.

## Kapsam

`POST /api/price-revisions/{id}/scopes`

```json
{ "scopeType": 3, "targetId": "…", "isExclude": false }
```

Kapsam satırları **iki role** ayrılır:

| Rol | `scopeType` | Ne yapar |
| --- | --- | --- |
| Ürün filtresi | `1` Ürün · `2` Kategori · `7` Ürün tipi · `8` Bölge | Hangi ürünlerin ele alınacağını belirler |
| Hedef filtresi | `3` Fiyat şablonu · `4` Birim tanımı · `5` Satış planı · `6` Fiyat listesi | Hangi fiyat satırlarının ele alınacağını belirler |

Kural şudur: **hiç ürün filtresi yoksa bütün ürünler, hiç hedef filtresi yoksa altı fiyat
alanının tamamı** kapsama girer. Ayrım şarttır — "SMS şablonuna %15" dendiğinde ürünün
paket taban fiyatına dokunulmamalıdır.

`isExclude: true` olan satır kapsamdan çıkarır: "tüm yazılım ürünleri, ama X hariç".

`scopeType: 7` (ürün tipi) için `targetId` yerine `targetValue` kullanılır (`"2"` = Software).

## Önizleme

`POST /api/price-revisions/{id}/preview`

Kapsamı tarar, etkilenecek her fiyat için bir satır üretir ve özet döner:

```json
{
  "lineCount": 6,
  "excludedLineCount": 0,
  "productCount": 2,
  "totalOldValue": 1.32,
  "totalNewValue": 1.53,
  "totalDifference": 0.21,
  "breakdown": [
    { "targetType": 4, "lineCount": 6, "totalOldValue": 1.32, "totalNewValue": 1.53 }
  ],
  "skippedRules": []
}
```

Önizleme her çağrıldığında satırlar silinip yeniden üretilir. **Kullanıcının hariç
tuttukları korunur** — `targetType + targetId + targetPath` üçlüsüyle eşleştirilerek.
Satır kimlikleri ise değişir, bu yüzden istemci tazeleme sonrası listeyi yeniden okumalıdır.

### Hedef türleri

| `targetType` | Güncellenen alan | `targetPath` |
| --- | --- | --- |
| `1` | `ProductLicenseOfferings.BasePrice` | boş |
| `2` | `ProductModuleOfferingPrices.Price` | boş |
| `3` | `ProductPricingRules` → tek değer | `$.value` ya da `$.amount` |
| `4` | `ProductPricingRules` → kademe | `$.tiers[N].value` |
| `5` | `ProductPrices.Amount` | boş |
| `6` | `ProductPriceListItems.Amount` | boş |

Bir kuraldan birden çok satır çıkabilir: her kademe ayrı bir satırdır.

### Zam uygulanamayan kurallar

Her `ProductPricingRule` bir fiyat taşımaz. Aşağıdakiler **kapsam dışı bırakılır** ve
`skippedRules` içinde gerekçesiyle listelenir:

- `type` değeri `percent`, `percentage` ya da `multiplier` olanlar — bunlar bir oran ifade eder,
- `operation: "subtract"` olanlar — bunlar indirimdir,
- kademelerinin tamamı oran tipli olanlar,
- içeriği okunamayan ya da zamlanabilir bir tutar barındırmayanlar.

Bunlara zam uygulamak fiyatı bozardı.

### Diğer kapsam kısıtları

- **Ürün fiyatlarında** yalnızca bugün geçerli satırlar taranır (`ValidFrom`/`ValidTo`);
  geçmiş dönem fiyatları zamlanmaz.
- Pasif satış planları, pasif modül fiyatları, pasif kurallar ve pasif fiyat listeleri
  kapsama girmez.

## Satır düzenleme

`PATCH /api/price-revisions/{id}/lines/{lineId}`

```json
{ "isExcluded": true }
```

ya da önizlenen değeri elle düzeltmek için:

```json
{ "newValue": 0.30 }
```

## Uygulama ve geri alma

`POST /api/price-revisions/{id}/apply` — onaylı revizyonu tek transaction içinde uygular.

- Her satır yazılmadan önce hedefteki **güncel değerin beklenen değerle aynı olduğu
  doğrulanır**. Arada elle değişmiş bir fiyat sessizce ezilmez: satır atlanır, `skipReason`
  ile işaretlenir ve sonuçta döner.
- Bir kuralın bütün satırları (taban değer ve kademeler) **tek okuma-yazma turunda**
  işlenir. Aksi hâlde her kademe JSON'un tamamını geri yazar ve bir öncekini silerdi.
- Kuralın `conditions`, `limits`, `unit`, `mode` gövdesine dokunulmaz; yalnızca
  `targetPath` alanı değişir.
- İleri tarihli revizyonlar (`effectiveDate` gelecekte) erken uygulanamaz; belge o tarihe
  kadar onaylı durumda bekler.

`POST /api/price-revisions/{id}/rollback` — uygulanmış satırları `oldValue` değerine
döndürür. Aynı doğrulama burada da geçerlidir.

Her iki uç da aynı sonucu döner:

```json
{
  "priceRevisionId": "…",
  "status": 5,
  "affectedLineCount": 6,
  "skippedLineCount": 0,
  "skippedLines": []
}
```

> **Sayı biçimi notu.** Geri alma fiyatı birebir eski **değerine** döndürür, ama JSON'daki
> sayı biçimini normalleştirir: `0.20` olarak yazılmış bir tutar geri alındığında `0.2`
> olur. Aynı sayıdır; fiyat motoru ve tüm JSON okuyucuları için fark yoktur.
