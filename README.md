# ProductManager

Bu doküman, **ProductManager** API’sindeki endpoint’lerin nasıl kullanılacağını ve temel DTO modellerini özetler.

## İçerik
- [Genel Bilgi](#genel-bilgi)
- [API Erişimi ve Dokümantasyon](#api-erişimi-ve-dokümantasyon)
- [Kimlik Doğrulama (Auth)](#kimlik-doğrulama-auth)
- [Enum Değerleri](#enum-değerleri)
- [Endpoint Rehberi](#endpoint-rehberi)
- [Temel Model Özetleri (DTO)](#temel-model-özetleri-dto)
- [Örnek Akış](#örnek-akış)

## Genel Bilgi
Proje .NET 10 tabanlı bir Web API içerir. API tarafında ürün yönetimi için şu alanlar bulunur:
- Authentication
- Product CRUD
- Catalog (Category, Supplier, Warehouse)
- Attribute tanımları ve değerleri
- Commerce (Variant, Price)
- Inventory (Stok, hareket, rezervasyon)
- Price list
- Product profiles (physical/software/service/subscription)

## API Erişimi ve Dokümantasyon
Uygulama çalıştığında:
- OpenAPI JSON: `/openapi/v1.json`
- Scalar UI: `/scalar` (uygulama konfigürasyonuna göre)

> Base URL örneği: `https://localhost:{port}`

## Kimlik Doğrulama (Auth)
JWT Bearer kullanılmaktadır.

Authorization header örneği:
```http
Authorization: Bearer {access_token}
```

Auth endpointleri:
- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/auth/refresh`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/auth/change-password` (**Authorize**)
- `POST /api/auth/logout` (**Authorize**)
- `POST /api/auth/logout-all` (**Authorize**)
- `GET /api/auth/me` (**Authorize**)
- `GET /api/auth/confirm-email?userId={guid}&token={token}`

### Auth örnek request
`POST /api/auth/login`
```json
{
  "email": "user@example.com",
  "password": "Password1",
  "rememberMe": true
}
```

Başarılı response (`AuthResponseDto`):
```json
{
  "succeeded": true,
  "user": {
    "id": "00000000-0000-0000-0000-000000000000",
    "email": "user@example.com",
    "firstName": "Ali",
    "lastName": "Deniz",
    "fullName": "Ali Deniz",
    "phoneNumber": null,
    "emailConfirmed": true,
    "isActive": true,
    "roles": ["User"],
    "createdAt": "2026-01-01T00:00:00Z"
  },
  "token": {
    "accessToken": "...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "expiresAt": "2026-01-01T01:00:00Z"
  },
  "errors": []
}
```

## Enum Değerleri

### ProductKind
- `1`: Physical
- `2`: Software
- `3`: Service
- `4`: Subscription
- `5`: Bundle
- `6`: DigitalAsset
- `99`: Other

### ProductStatus
- `1`: Draft
- `2`: Active
- `3`: Passive
- `4`: Archived

### PriceType
- `1`: Sale
- `2`: List
- `3`: Cost
- `4`: Campaign
- `5`: Wholesale

### InventoryPolicy
- `1`: TrackAndBlockWhenNegative
- `2`: TrackAndAllowNegative
- `3`: DoNotTrack

### InventoryTransactionType
- `1`: PurchaseReceipt
- `2`: SaleIssue
- `3`: ReturnIn
- `4`: ReturnOut
- `5`: TransferIn
- `6`: TransferOut
- `7`: Reservation
- `8`: ReservationRelease
- `9`: Adjustment

### InventoryReservationStatus
- `1`: Active
- `2`: Released
- `3`: Converted
- `4`: Expired
- `5`: Cancelled

### MediaType
- `1`: Image
- `2`: Video
- `3`: Document
- `4`: Url

### ServiceDeliveryMode
- `1`: OnSite
- `2`: Remote
- `3`: Hybrid

### SoftwareLicenseModel
- `1`: Perpetual
- `2`: Subscription
- `3`: UsageBased
- `4`: SeatBased

### BillingPeriodUnit
- `1`: Day
- `2`: Week
- `3`: Month
- `4`: Year

## Endpoint Rehberi
Aşağıda endpointler modül bazında listelenmiştir.

### 1) Products
Base: `/api/products`

- `GET /api/products` → ürün listesi (`ProductFilterDto` query)
- `GET /api/products/{productId}` → ürün detayı
- `POST /api/products` → ürün oluştur (`CreateProductRequestDto`)
- `PUT /api/products/{productId}` → ürün güncelle (`UpdateProductRequestDto`)
- `DELETE /api/products/{productId}` → ürün sil

### 2) Attributes
Base: `/api/attributes`

- `GET /api/attributes`
- `GET /api/attributes/{attributeDefinitionId}`
- `POST /api/attributes` (`CreateProductAttributeDefinitionRequestDto`)
- `PUT /api/attributes/{attributeDefinitionId}` (`UpdateProductAttributeDefinitionRequestDto`)
- `DELETE /api/attributes/{attributeDefinitionId}`

### 3) Catalog
Base: `/api/catalog`

**Categories**
- `GET /api/catalog/categories`
- `GET /api/catalog/categories/{categoryId}`
- `POST /api/catalog/categories`
- `PUT /api/catalog/categories/{categoryId}`
- `DELETE /api/catalog/categories/{categoryId}`

**Suppliers**
- `GET /api/catalog/suppliers?includeInactive=false`
- `GET /api/catalog/suppliers/{supplierId}`
- `POST /api/catalog/suppliers`
- `PUT /api/catalog/suppliers/{supplierId}`
- `DELETE /api/catalog/suppliers/{supplierId}`

**Warehouses**
- `GET /api/catalog/warehouses?includeInactive=false`
- `GET /api/catalog/warehouses/{warehouseId}`
- `POST /api/catalog/warehouses`
- `PUT /api/catalog/warehouses/{warehouseId}`
- `DELETE /api/catalog/warehouses/{warehouseId}`

### 4) Product Commerce
Base: `/api/products`

**Variants**
- `GET /api/products/{productId}/variants`
- `GET /api/products/variants/{variantId}`
- `POST /api/products/{productId}/variants`
- `PUT /api/products/variants/{variantId}`
- `DELETE /api/products/variants/{variantId}`

**Prices**
- `GET /api/products/{productId}/prices`
- `GET /api/products/prices/{priceId}`
- `POST /api/products/{productId}/prices`
- `PUT /api/products/prices/{priceId}`
- `DELETE /api/products/prices/{priceId}`

### 5) Product Profiles
Base: `/api/products/{productId}/profiles`

- `GET /physical` / `PUT /physical` / `DELETE /physical`
- `GET /software` / `PUT /software` / `DELETE /software`
- `GET /service` / `PUT /service` / `DELETE /service`
- `GET /subscription` / `PUT /subscription` / `DELETE /subscription`

Tam URL örneği:
- `GET /api/products/{productId}/profiles/physical`

### 6) Product Relations
Base: `/api/products`

**Attribute Values**
- `GET /api/products/{productId}/attribute-values`
- `GET /api/products/attribute-values/{attributeValueId}`
- `POST /api/products/{productId}/attribute-values`
- `PUT /api/products/attribute-values/{attributeValueId}`
- `DELETE /api/products/attribute-values/{attributeValueId}`

**Category Maps**
- `GET /api/products/{productId}/category-maps`
- `GET /api/products/category-maps/{categoryMapId}`
- `POST /api/products/{productId}/category-maps`
- `PUT /api/products/category-maps/{categoryMapId}`
- `DELETE /api/products/category-maps/{categoryMapId}`

**Media**
- `GET /api/products/{productId}/media`
- `GET /api/products/media/{mediaId}`
- `POST /api/products/{productId}/media`
- `PUT /api/products/media/{mediaId}`
- `DELETE /api/products/media/{mediaId}`

**Bundle Items**
- `GET /api/products/{productId}/bundle-items`
- `GET /api/products/bundle-items/{bundleItemId}`
- `POST /api/products/{productId}/bundle-items`
- `PUT /api/products/bundle-items/{bundleItemId}`
- `DELETE /api/products/bundle-items/{bundleItemId}`

**Supplier Maps**
- `GET /api/products/{productId}/supplier-maps`
- `GET /api/products/supplier-maps/{supplierMapId}`
- `POST /api/products/{productId}/supplier-maps`
- `PUT /api/products/supplier-maps/{supplierMapId}`
- `DELETE /api/products/supplier-maps/{supplierMapId}`

### 7) Inventory
Base: `/api/inventory`

**Inventories**
- `GET /api/inventory/inventories` (`ProductInventoryFilterDto` query)
- `GET /api/inventory/inventories/{inventoryId}`
- `POST /api/inventory/inventories`
- `PUT /api/inventory/inventories/{inventoryId}`
- `DELETE /api/inventory/inventories/{inventoryId}`

**Transactions**
- `GET /api/inventory/transactions` (`InventoryTransactionFilterDto` query)
- `GET /api/inventory/transactions/{transactionId}`
- `POST /api/inventory/transactions`

**Reservations**
- `GET /api/inventory/reservations` (`InventoryReservationFilterDto` query)
- `GET /api/inventory/reservations/{reservationId}`
- `POST /api/inventory/reservations`
- `PATCH /api/inventory/reservations/{reservationId}/status`
- `DELETE /api/inventory/reservations/{reservationId}`

### 8) Price Lists
Base: `/api/pricelists`

- `GET /api/pricelists?includeInactive=false`
- `GET /api/pricelists/{priceListId}`
- `POST /api/pricelists`
- `PUT /api/pricelists/{priceListId}`
- `DELETE /api/pricelists/{priceListId}`

**Items**
- `GET /api/pricelists/{priceListId}/items`
- `GET /api/pricelists/items/{priceListItemId}`
- `POST /api/pricelists/items`
- `PUT /api/pricelists/items/{priceListItemId}`
- `DELETE /api/pricelists/items/{priceListItemId}`

## Temel Model Özetleri (DTO)
Aşağıda en çok kullanılan modellerin kısa özeti verilmiştir.

### ProductDto
- `id`, `productCode`, `name`
- `shortDescription`, `description`
- `kind`, `status`
- `brand`, `manufacturer`, `barcode`
- `isActive`, `isSellable`, `isPurchasable`, `trackInventory`
- `defaultCurrencyCode`, `unitOfMeasure`
- `taxRate`, `taxCode`, `tags`, `metadataJson`
- `createdAt`, `updatedAt`

### ProductCategoryDto
- `id`, `code`, `name`, `description`, `parentCategoryId`, `createdAt`, `updatedAt`

### ProductAttributeDefinitionDto
- `id`, `key`, `displayName`, `dataType`
- `isRequired`, `isFilterable`, `isVariantAxis`
- `allowedValuesJson`, `validationRuleJson`

### ProductVariantDto
- `id`, `productId`, `sku`, `barcode`, `name`
- `optionValuesJson`, `additionalPrice`, `additionalCost`, `isActive`

### ProductPriceDto
- `id`, `productId`, `productVariantId`
- `priceType`, `amount`, `compareAtAmount`, `currencyCode`
- `minQuantity`, `maxQuantity`, `validFrom`, `validTo`

### ProductInventoryDto
- `id`, `productId`, `productVariantId`, `warehouseId`, `warehouseCode`
- `quantityOnHand`, `quantityReserved`, `quantityAvailable`
- `reorderPoint`, `reorderQuantity`, `inventoryPolicy`

### InventoryTransactionDto
- `id`, `productId`, `transactionType`, `quantity`
- `unitCost`, `referenceType`, `referenceNumber`, `note`, `occurredAt`

### InventoryReservationDto
- `id`, `productId`, `quantity`, `reservationCode`
- `reservedUntil`, `status`, `sourceType`, `sourceId`

### ProductPriceListDto / ProductPriceListItemDto
- PriceList: `code`, `name`, `currencyCode`, `isActive`, `validFrom`, `validTo`
- Item: `productPriceListId`, `productId`, `productVariantId`, `amount`

## Örnek Akış
1. `POST /api/auth/login` ile token al.
2. `POST /api/products` ile ürün oluştur.
3. `POST /api/catalog/categories` ile kategori oluştur.
4. `POST /api/products/{productId}/category-maps` ile ürünü kategoriye bağla.
5. `POST /api/inventory/inventories` ile stok kartı aç.
6. `POST /api/inventory/transactions` ile stok hareketi oluştur.

---

İstersen bir sonraki adımda bu dokümana **örnek cURL komutlarını** da endpoint bazında ekleyebilirim.