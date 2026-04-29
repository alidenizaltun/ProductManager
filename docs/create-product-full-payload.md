# Örnek JSON Payload: Ürün (Full Create)

Aşağıdaki örnek, ürünün tüm ilişkili alanlarını tek istekte göndermek için hazırlanmış örnek payload’dır. Tüm koleksiyonlar ve profiller isteğe bağlıdır.

## Lookup endpointleri (dropdown verisi)

Formdaki kimlik alanlarını hardcode etmek yerine aşağıdaki endpointleri kullanın:

- `GET /api/lookups` (toplu lookup)
- `GET /api/lookups/products`
- `GET /api/lookups/categories`
- `GET /api/lookups/warehouses`
- `GET /api/lookups/suppliers`
- `GET /api/lookups/price-lists`

Örnek yanıt:

```json
[
  { "id": "f3471f8e-1f91-4c66-b6fb-6e9cb2666d8c", "name": "ELEKTRONIK" },
  { "id": "df8fc9d4-5f18-4b6e-8f9d-a4c5f46d0b4e", "name": "TEKSTIL" }
]
```

React tarafında servis katmanında bu endpointleri çağırıp state'e yazın, submit sırasında seçilen `id` değerlerini `categoryMaps`, `inventories`, `supplierMaps`, `priceListItems` gibi alanlara mapleyin.

```json
{
  "product": {
    "productCode": "URN-001",
    "name": "Örnek Ürün",
    "shortDescription": "Kısa açıklama",
    "description": "Detaylı açıklama",
    "kind": 1,
    "status": 1,
    "brand": "Marka",
    "manufacturer": "Üretici",
    "barcode": "1234567890123",
    "isActive": true,
    "isSellable": true,
    "isPurchasable": true,
    "trackInventory": true,
    "defaultCurrencyCode": "TRY",
    "unitOfMeasure": "Adet",
    "taxRate": 18,
    "taxCode": "KDV18",
    "tags": "etiket-1,etiket-2",
    "metadataJson": "{\"key\":\"value\"}"
  },
  "attributeValues": [
    {
      "attributeDefinitionId": "11111111-1111-1111-1111-111111111111",
      "valueText": "Kırmızı"
    }
  ],
  "variants": [
    {
      "sku": "URN-001-RED-M",
      "barcode": "1234567890124",
      "name": "Kırmızı - M",
      "optionValuesJson": "{\"color\":\"Kırmızı\",\"size\":\"M\"}",
      "additionalPrice": 25,
      "additionalCost": 10,
      "isActive": true
    }
  ],
  "prices": [
    {
      "priceType": 1,
      "amount": 199.90,
      "compareAtAmount": 249.90,
      "currencyCode": "TRY",
      "minQuantity": 1,
      "maxQuantity": 10,
      "salesChannel": "online",
      "customerGroupCode": "retail"
    }
  ],
  "inventories": [
    {
      "warehouseId": "22222222-2222-2222-2222-222222222222",
      "warehouseCode": "MERKEZ",
      "quantityOnHand": 100,
      "quantityReserved": 5,
      "reorderPoint": 10,
      "reorderQuantity": 50,
      "inventoryPolicy": 1
    }
  ],
  "mediaItems": [
    {
      "mediaType": 1,
      "url": "https://cdn.example.com/products/urn-001.jpg",
      "thumbnailUrl": "https://cdn.example.com/products/urn-001-thumb.jpg",
      "mimeType": "image/jpeg",
      "altText": "Ürün görseli",
      "isPrimary": true,
      "sortOrder": 1
    }
  ],
  "categoryMaps": [
    {
      "productCategoryId": "33333333-3333-3333-3333-333333333333",
      "isPrimary": true,
      "sortOrder": 1
    }
  ],
  "bundleItems": [
    {
      "bundleProductId": "44444444-4444-4444-4444-444444444444",
      "childProductId": "55555555-5555-5555-5555-555555555555",
      "childVariantId": "66666666-6666-6666-6666-666666666666",
      "quantity": 1,
      "isOptional": false,
      "ruleJson": "{\"min\":1}"
    }
  ],
  "supplierMaps": [
    {
      "productSupplierId": "77777777-7777-7777-7777-777777777777",
      "supplierProductCode": "SUP-001",
      "supplierCost": 120.50,
      "leadTimeInDays": 5,
      "minOrderQuantity": 10,
      "isPreferred": true
    }
  ],
  "inventoryTransactions": [
    {
      "transactionType": 1,
      "quantity": 100,
      "unitCost": 90,
      "referenceType": "purchase",
      "referenceNumber": "PO-1001",
      "note": "İlk giriş"
    }
  ],
  "inventoryReservations": [
    {
      "quantity": 2,
      "reservationCode": "RES-0001",
      "status": 1,
      "sourceType": "order",
      "sourceId": "ORD-0001"
    }
  ],
  "priceListItems": [
    {
      "productPriceListId": "88888888-8888-8888-8888-888888888888",
      "amount": 189.90,
      "compareAtAmount": 229.90,
      "minQuantity": 1,
      "maxQuantity": 5
    }
  ],
  "physicalProfile": {
    "weight": 1.2,
    "width": 10,
    "height": 5,
    "length": 20,
    "requiresShipping": true,
    "isFragile": false,
    "isHazardous": false,
    "requiresSerialNumber": false,
    "warrantyInMonths": 24
  },
  "softwareProfile": {
    "version": "1.0.0",
    "licenseModel": 1,
    "seatCount": 10,
    "downloadUrl": "https://download.example.com/app",
    "supportedPlatformsJson": "[\"Windows\",\"macOS\"]",
    "systemRequirementsJson": "{\"ram\":\"8GB\"}",
    "releaseNotes": "İlk sürüm"
  },
  "serviceProfile": {
    "deliveryMode": 2,
    "durationInMinutes": 60,
    "maxConcurrentBooking": 3,
    "serviceAreaJson": "{\"city\":\"İstanbul\"}",
    "serviceLevelAgreementJson": "{\"response\":\"24h\"}",
    "capacityRuleJson": "{\"daily\":10}"
  },
  "subscriptionProfile": {
    "billingPeriodUnit": 3,
    "billingPeriodValue": 1,
    "trialDays": 14,
    "autoRenew": true,
    "gracePeriodDays": 7,
    "cancellationPolicy": "İptal koşulu",
    "subscriptionRulesJson": "{\"maxUsers\":10}"
  }
}
```
