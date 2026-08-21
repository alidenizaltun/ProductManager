# Pricing Templates API

Ürün bağımsız, yeniden kullanılabilir fiyatlandırma tanımları. Bir üründe kurulmuş bir
fiyatlandırma kuralı (ör. SMS birim fiyatı) şablona alınır ve başka ürünlere tek istekle
uygulanır.

## Model

Şablon, ürüne uygulandığında **kopyalanır**: değerler hedef ürünün kendi
`ProductPricingRules` satırına yazılır, satıra kaynağın izi düşülür.

| Alan | Açıklama |
| --- | --- |
| `code` | Boş bırakılırsa sistem üretir (`TPL-000001`) |
| `templateKind` | `1` PricingRule (şu an desteklenen tek tür) |
| `unitDefinitionId` | Şablonun bağlı olduğu global birim (SMS). Uygulama anında hedef ürünün birimi buradan çözülür |
| `payloadJson` | Kuralın `priceAdjustment` gövdesi — `ProductPricingRules` ile birebir aynı format |
| `version` | Payload her değiştiğinde artar. Kopyaya iz olarak yazılır |
| `usageCount` | Bu şablondan türemiş, silinmemiş kural sayısı |

Kopyalanan kuralda iki yeni alan bulunur: `sourceTemplateId` ve `sourceTemplateVersion`.
Kural DTO'su ayrıca şablonun güncel sürümünü `templateCurrentVersion` ile döner; ikisi
farklıysa kural şablonun gerisinde kalmıştır.

> Kopya yaklaşımı bilinçli bir tercihtir. Fiyat motoru (`PriceEngineCalculator`,
> `PricingRuleAdjustmentEvaluator`) hiç değişmez, ürün bazında istisna yapmak doğal kalır,
> buna karşılık zam kapsamı `sourceTemplateId` izini takip ederek bulunabilir.

## Endpoints

- `GET /api/pricing-templates?templateKind=&unitDefinitionId=&includeInactive=`
- `GET /api/pricing-templates/{pricingTemplateId}`
- `GET /api/pricing-templates/{pricingTemplateId}/usages`
- `POST /api/pricing-templates`
- `PUT /api/pricing-templates/{pricingTemplateId}`
- `DELETE /api/pricing-templates/{pricingTemplateId}`
- `POST /api/pricing-templates/{pricingTemplateId}/apply`
- `POST /api/pricing-templates/{pricingTemplateId}/apply-bulk`
- `POST /api/products/pricing-rules/{pricingRuleId}/save-as-template`

İzinler: okuma uçları `Pricing.Templates.View`, yazma uçları `Pricing.Templates.Manage`.

## Şablon oluşturma

`POST /api/pricing-templates`

```json
{
  "name": "SMS Birim Fiyatı 2026",
  "description": "Kademeli SMS fiyatlandırması",
  "templateKind": 1,
  "unitDefinitionId": "00000000-0000-0000-0000-000000000000",
  "currencyCode": "TRY",
  "payload": {
    "mode": "unit",
    "type": "fixed",
    "applyOn": "currentPrice",
    "unit": { "field": "feature.smsCount", "rounding": "ceil" },
    "tiers": [
      { "from": 0, "to": 1000, "type": "fixed", "value": 0.24 },
      { "from": 1001, "to": null, "type": "fixed", "value": 0.20 }
    ]
  }
}
```

`payload` yerine `payloadJson` (aynı nesnenin serileştirilmiş hâli) de gönderilebilir.

## Var olan kuraldan şablon üretme

`POST /api/products/pricing-rules/{pricingRuleId}/save-as-template`

```json
{ "name": "SMS Birim Fiyatı 2026" }
```

Kuralın `priceAdjustmentJson` içeriği payload olarak, atanmış ürün biriminin
`unitDefinitionId` değeri de şablonun birimi olarak kopyalanır. `name` boş bırakılırsa
kuralın adı kullanılır.

## Şablonu ürüne uygulama

`POST /api/pricing-templates/{pricingTemplateId}/apply`

```json
{
  "productId": "00000000-0000-0000-0000-000000000000",
  "licenseOfferingId": null,
  "priority": 20,
  "isActive": true,
  "overrideValue": null
}
```

Uygulama tek transaction içinde şunları yapar:

1. Şablonu ve hedef ürünü doğrular.
2. **Birimi çözer.** Hedef üründe şablonun `unitDefinitionId` değerine bağlı bir
   `ProductUnit` yoksa, birim tanımının kod ve adıyla oluşturur. Birim ardından hem
   **kurala** (`ProductPricingRuleUnits`) hem de **satış planlarına**
   (`ProductLicenseOfferingUnits`) bağlanır.
3. **Birimi satış planlarına bağlar.** Plan verilmişse yalnızca ona, verilmemişse ürünün
   tüm aktif planlarına. Plana ilk birim eklenirken, o ana kadar örtük olarak gelen
   varsayılan birimler (ör. "Kullanıcı") önce kalıcı hâle getirilir — aksi hâlde fiyat
   motorundaki yedek devre dışı kalır ve o birim sessizce düşerdi.
4. Kuralı ekler; `sourceTemplateId` ve `sourceTemplateVersion` alanlarını doldurur.
5. **Kod çakışmasını çözer.** `IX_ProductPricingRules_ProductId_Code` benzersiz olduğu için
   aynı şablon aynı ürüne ikinci kez uygulanırsa kod `TPL-000004-2` biçiminde artırılır.

Dönen sonuç:

```json
{
  "productId": "…",
  "productName": "HRM Pro",
  "succeeded": true,
  "pricingRuleId": "…",
  "pricingRuleCode": "TPL-000004",
  "createdProductUnitId": "…",
  "linkedProductUnitId": "…",
  "linkedOfferingCount": 2,
  "message": null
}
```

`createdProductUnitId` doluysa hedef üründe birim yoktu ve bu istekte oluşturuldu.
`linkedOfferingCount`, birimin kaç satış planına bağlandığını söyler.

### Birim kapsamı nasıl çalışır

Kuraldaki birim ataması bir **kapsam** ifadesidir: "bu kural bu birim için geçerlidir".
Kuralın **miktarı** ise birimden değil, `priceAdjustment.unit.field` alanından
(ör. `feature.smsCount`) gelir.

Kapsam eşleşmesi (`PricingRuleAdjustmentEvaluator.RuleProductUnitsMatch`) iki yoldan olur:

1. İstek `offeringUnits` taşıyorsa (seat/usage planları) — kural, listede kendi birimini
   arar.
2. İstek `offeringUnits` taşımıyorsa — **satış planının** birimlerine bakılır.

İkinci yol şart: `pricing-parameters` yalnızca seat/usage planları için birim sorar, bu
yüzden abonelik ve kalıcı lisans planlarında `offeringUnits` her zaman boş gelir. Bu yol
olmadan, birim atanmış her kural bu planlarda **sessizce hiç çalışmazdı** — bu yüzden
şablon uygulaması birimi kural ile planın ikisine birden bağlar.

Zam tarafında "birim tanımı" kapsamı da kuralları iki yoldan bulur: doğrudan atanmış ürün
birimi ya da geldiği şablonun `unitDefinitionId` izi.

### `overrideValue`

Tek seferlik değer farkı için kullanılır ve yalnızca **kademesiz** şablonlarda geçerlidir.
Kademeli bir şablonda tek bir değeri değiştirmek anlamsız olduğu için istek
`400 ValidationException` ile reddedilir; kademeleri değiştirmek için kural uygulandıktan
sonra düzenlenmelidir.

## Toplu uygulama

`POST /api/pricing-templates/{pricingTemplateId}/apply-bulk`

```json
{
  "productIds": ["…", "…", "…"],
  "priority": 20,
  "isActive": true
}
```

Her ürün **kendi transaction'ında** işlenir: bir üründeki hata diğerlerini geri almaz.
Sonuç, ürün başına bir `ApplyPricingTemplateResultDto` listesidir; başarısız olanlarda
`succeeded: false` ve `message` dolu gelir.

## Kullanım listesi

`GET /api/pricing-templates/{pricingTemplateId}/usages`

```json
[
  {
    "pricingRuleId": "…",
    "pricingRuleCode": "TPL-000004",
    "pricingRuleName": "SMS Birim Fiyatı 2026",
    "productId": "…",
    "productCode": "PRD-000012",
    "productName": "HRM Pro",
    "productLicenseOfferingId": "…",
    "licenseOfferingName": "Standart",
    "sourceTemplateVersion": 2,
    "templateVersion": 3,
    "isOutdated": true,
    "isActive": true
  }
]
```

`isOutdated` alanı, kuralın şablonun güncel sürümünün gerisinde kaldığını gösterir.
Bu, kopya yaklaşımının doğal sonucudur; geride kalan kuralları şablona eşitlemek ayrı
bir aksiyondur ve otomatik yapılmaz.

## Sürüm davranışı

`PUT /api/pricing-templates/{id}` yalnızca `payloadJson` değiştiğinde `version` değerini
artırır. Ad, açıklama ya da sıra değişikliği sürümü etkilemez — sürüm "fiyat değişti mi"
sorusunun cevabıdır.
