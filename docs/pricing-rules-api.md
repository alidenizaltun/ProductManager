# Pricing Rules API

Backend now supports product-level dynamic pricing rules through `ProductPricingRules`.

## Endpoints

- `GET /api/products/{productId}/pricing-rules`
- `GET /api/products/pricing-rules/{pricingRuleId}`
- `POST /api/products/{productId}/pricing-rules`
- `PUT /api/products/pricing-rules/{pricingRuleId}`
- `DELETE /api/products/pricing-rules/{pricingRuleId}`

## Rule Shape

```json
{
  "code": "USER_COUNT_TIERS",
  "name": "User count tier pricing",
  "priority": 10,
  "isActive": true,
  "validFrom": "2026-06-23T00:00:00Z",
  "validTo": null,
  "priceAdjustment": {
    "mode": "unit",
    "type": "fixed",
    "value": 50,
    "applyOn": "currentPrice",
    "unit": {
      "field": "feature.userCount",
      "freeUnits": 5,
      "rounding": "ceil"
    },
    "tiers": [
      { "from": 0, "to": 5, "type": "fixed", "value": 0 },
      { "from": 6, "to": 50, "type": "fixed", "value": 50 },
      { "from": 51, "to": null, "type": "fixed", "value": 35 }
    ],
    "limits": {
      "minAdjustment": null,
      "maxAdjustment": null,
      "minFinalPrice": null,
      "maxFinalPrice": null
    },
    "conditions": {
      "operator": "all",
      "items": [
        { "field": "feature.userCount", "operator": "gt", "value": 0 }
      ]
    }
  }
}
```

For backward compatibility, clients may also send the same object serialized as `priceAdjustmentJson`.

## Price Calculation Request

Dynamic feature values are passed to the existing price calculation endpoint:

```json
{
  "licenseOfferingId": "00000000-0000-0000-0000-000000000000",
  "featureValues": {
    "userCount": 75
  }
}
```

Rules are filtered by `isActive`, `validFrom`, `validTo`, optional sales channel, customer group, product variant, and license offering. Matching rules are applied by ascending `priority`.

## Pricing Parameters

`GET /api/products/{productId}/license-offerings/{offeringId}/pricing-parameters` now returns:

- `unitParameters`: seat/usage teklifleri için ürünün varsayılan biriminden türetilen miktar alanları.
- `ruleParameters`: dynamic `ProductPricingRules.priceAdjustment.unit.field` inputs such as `feature.userCount`.

Dealer portal order pricing sends the selected values through `POST /api/orders/calculate-order-price` item `features`. A feature with `"featureName": "userCount"` is forwarded to the pricing engine as `feature.userCount`.

## Supported Adjustment Fields

- `mode`: `unit` or default single adjustment
- `type`: `fixed`, `percent`, `percentage`, `multiplier`, `custom`
- `operation`/`direction`: use `subtract` to force a negative adjustment
- `applyOn`: `basePrice`, `currentPrice`, `previousResult`
- `unit.field`: supports request `featureValues` via `feature.<key>` and product attributes by key
- `unit.freeUnits`: ignored when `tiers` are present, because tiers are authoritative
- `unit.rounding`: `ceil`, `floor`, `round`, or `none`
- `conditions.operator`: `all` or `any`
- condition operators: `eq`, `neq`, `gt`, `gte`, `lt`, `lte`, `contains`, `in`, `exists`
- `limits`: `minAdjustment`, `maxAdjustment`, `minFinalPrice`, `maxFinalPrice`

Legacy adjustment JSON such as `{ "type": "percentage", "value": 10 }`, `{ "amount": 50 }`, or subtract variants with `{ "operation": "subtract" }` remains supported.
