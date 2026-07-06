# Frontend change guide: sales plans and pricing rules with multiple product units

This document is for applying the latest `ProductManager` backend change to the React frontend at:

```txt
D:\Projects\React\product-management
```

The frontend already has the first ProductUnits migration in place: `ProductUnitsTab`, product-unit endpoints, single product-unit fields on sales plans and pricing rules, and product-unit selectors in `LicenseOfferingsTab` / `ProductPricingRulesPanel`. This change is not a fresh ProductUnits implementation. It replaces the existing single product-unit binding with multi-select binding.

## Backend contract

`Product` no longer has a direct `UnitDefinitionId` / `UnitDefinitionName` field. A unit definition is now reached through product-scoped units:

```txt
Product -> ProductUnits -> UnitDefinition
```

So product create/update/detail payloads should not send or read product-level `unitDefinitionId` anymore. Unit selection for a product belongs to the ProductUnits tab.

`ProductLicenseOffering` and `ProductPricingRule` can now be assigned to more than one `ProductUnit`.

New database tables:

```txt
Product.ProductLicenseOfferingUnits
Product.ProductPricingRuleUnits
```

Removed top-level single-unit fields:

```ts
productUnitId?: string | null;
productUnitCode?: string | null;
productUnitName?: string | null;
productUnitTempId?: string | null;
```

Do not use top-level `productUnit*` single fields or top-level `unitDefinition*` fields on product, license offering, or pricing rule DTOs. Unit metadata belongs inside `ProductUnitDto`.

New preferred read fields:

```ts
productUnitIds: string[];
productUnits: ProductUnitDto[];
```

New preferred create/full-save fields:

```ts
productUnitIds?: string[];
productUnitTempIds?: string[];
```

Standalone update uses saved IDs:

```ts
productUnitIds?: string[];
```

Do not send a "first selected" legacy unit field. New UI state and payloads should be driven by `productUnitIds`, `productUnitTempIds`, and `productUnits`.

## Target files in the frontend

Update these files first:

```txt
src/shared/types/productOperations.types.ts
src/modules/products/types/productEditor.types.ts
src/modules/products/pages/ProductFormPage.tsx
src/modules/products/components/editor/GeneralInfoTab.tsx
src/modules/products/components/editor/LicenseOfferingsTab.tsx
src/modules/products/components/pricing-rules/ProductPricingRulesPanel.tsx
src/modules/products/components/detail/ProductDetailTabs.tsx
src/tests/mocks/fixtures.ts
src/modules/products/__tests__/productFormMapping.test.ts
```

The following files should already exist and should only need small adjustments if type errors surface:

```txt
src/modules/products/api/products.api.ts
src/modules/products/hooks/useProductUnits.ts
src/modules/products/hooks/useProductPricingRules.ts
src/services/query/queryKeys.ts
src/shared/config/apiEndpoints.ts
```

## Type changes

In `src/shared/types/productOperations.types.ts`, remove product-level unit fields from product DTO/request types if they exist:

```ts
// Remove from ProductDto / ProductDetailDto / CreateProductRequestDto / UpdateProductRequestDto
// Also remove from ProductLicenseOfferingDto and ProductPricingRuleDto
unitDefinitionId?: Uuid | null;
unitDefinitionCode?: string | null;
unitDefinitionName?: string | null;
```

Keep `ProductUnitDto.unitDefinitionId`, `ProductUnitDto.unitDefinitionCode`, and `ProductUnitDto.unitDefinitionName`; those are still correct.

Extend `ProductPricingRuleDto`:

```ts
productUnitIds?: Uuid[];
productUnits?: ProductUnitDto[];
```

Remove these from `ProductPricingRuleDto` and pricing-rule create/update request types:

```ts
productUnitId?: Uuid | null;
productUnitCode?: string | null;
productUnitName?: string | null;
productUnitTempId?: string | null;
```

Extend `UpsertProductPricingRuleRequestDto`:

```ts
productUnitIds?: Uuid[];
productUnitTempIds?: string[];
```

Extend `ProductLicenseOfferingDto`:

```ts
productUnitIds?: Uuid[];
productUnits?: ProductUnitDto[];
productUnitTempIds?: string[];
```

Remove these from `ProductLicenseOfferingDto` and license-offering create/update request types:

```ts
productUnitId?: Uuid | null;
productUnitCode?: string | null;
productUnitName?: string | null;
productUnitTempId?: string | null;
```

In `CreateFullProductRequestDto.licenseOfferings[]`, add:

```ts
productUnitIds?: Uuid[];
productUnitTempIds?: string[];
```

In `CreateFullProductRequestDto.pricingRules[]`, add:

```ts
productUnitIds?: Uuid[];
productUnitTempIds?: string[];
```

In `src/modules/products/types/productEditor.types.ts`, remove the existing singular fields and keep only arrays:

```ts
export interface LicenseOfferingForm {
  productUnitIds?: string[];
  productUnitTempIds?: string[];
  // existing fields...
}

export interface ProductPricingRuleForm {
  productUnitIds?: string[];
  productUnitTempIds?: string[];
  // existing fields...
}
```

Also remove `unitDefinitionId` from the product/general-info form model. Product-level unit selection should not exist anymore.

## GeneralInfoTab and ProductUnitsTab

In `src/modules/products/components/editor/GeneralInfoTab.tsx`, remove the old single product unit-definition selector and any validation/payload logic around `unitDefinitionId`.

Keep `ProductUnitsTab` as the place where product units are created and mapped to unit definitions. The flow should be:

```txt
1. Create/select unit definitions inside ProductUnitsTab records.
2. Sales plans select one or more ProductUnit records.
3. Pricing rules select one or more ProductUnit records for rule scope.
```

In `ProductFormPage.tsx`, remove product-level mapping like:

```ts
unitDefinitionId: detail.product.unitDefinitionId
```

and remove product payload fields like:

```ts
unitDefinitionId: values.unitDefinitionId
```

## Shared helper shape

Both sales plans and pricing rules currently use option values like:

```ts
id:{guid}
temp:{_tempId}
```

Use the same convention for multi-select values.

Suggested helpers:

```ts
const toUnitScopeValues = (
  ids?: Array<string | null | undefined>,
  tempIds?: Array<string | null | undefined>
) => [
  ...(ids ?? []).filter(Boolean).map((id) => `id:${id}`),
  ...(tempIds ?? []).filter(Boolean).map((id) => `temp:${id}`),
];

const splitUnitScopeValues = (values: string[]) => {
  const productUnitIds = values
    .filter((value) => value.startsWith("id:"))
    .map((value) => value.replace("id:", ""))
    .filter(Boolean);

  const productUnitTempIds = values
    .filter((value) => value.startsWith("temp:"))
    .map((value) => value.replace("temp:", ""))
    .filter(Boolean);

  return {
    productUnitIds,
    productUnitTempIds,
  };
};
```

Use a checkbox list or a native `<select multiple>`. A checkbox list is usually clearer for this app because product units are short operational records.

## ProductFormPage mapping

When mapping backend detail to form values, read arrays directly:

```ts
const mapProductUnitIds = (item: {
  productUnitIds?: string[];
}) => item.productUnitIds?.filter(Boolean) ?? [];
```

For license offerings:

```ts
productUnitIds: mapProductUnitIds(lo),
```

For pricing rules:

```ts
productUnitIds: mapProductUnitIds(rule),
```

When building the full product payload, send only arrays:

```ts
const productUnitIds = offering.productUnitIds?.filter(Boolean) ?? [];
const productUnitTempIds = offering.productUnitTempIds?.filter(Boolean) ?? [];

return {
  // existing offering fields...
  productUnitIds: productUnitIds.length ? productUnitIds : undefined,
  productUnitTempIds: productUnitTempIds.length ? productUnitTempIds : undefined,
};
```

Apply the same payload rule for `pricingRules`.

## LicenseOfferingsTab

Current state:

```txt
src/modules/products/components/editor/LicenseOfferingsTab.tsx
```

It currently watches `productUnits`, stores one selected unit, and disables standalone save when the selected unit is unsaved.

Change it to multi selection:

1. Build selected option values from `productUnitIds` and `productUnitTempIds`.
2. Render active product units as checkboxes or a multi-select.
3. On change, write only `productUnitIds` and `productUnitTempIds`.
4. Disable standalone `Plan Ekle` / `Planı Güncelle` if any selected value starts with `temp:`. Standalone APIs can only link saved product units.
5. In `buildOfferingPayload`, send `productUnitIds`.

Payload rule:

```ts
const productUnitIds = offering.productUnitIds?.filter(Boolean) ?? [];

const payload = {
  productUnitIds: productUnitIds.length ? productUnitIds : undefined,
  // existing fields...
};
```

Do not send `productUnitTempIds` to standalone create/update. Unsaved unit references belong to the full product save flow.

## ProductPricingRulesPanel

Current state:

```txt
src/modules/products/components/pricing-rules/ProductPricingRulesPanel.tsx
```

It already accepts `productUnits`, has one selected unit, and shows the unit scope select. Upgrade that scope to multi.

Important distinction:

```txt
productUnitIds       = which product units this rule applies to
priceAdjustment.unit = which quantity/feature field is used for the formula
```

Do not merge these concepts.

Required changes:

1. Extend `RuleFormState` with `productUnitIds: string[]` and `productUnitTempIds: string[]`.
2. `emptyForm` should default both arrays to `[]`.
3. `ruleToForm` should read `rule.productUnitIds`.
4. `buildPayload` should send `productUnitIds` and, only in full-product draft save, `productUnitTempIds`.
5. Draft rules for new products must preserve `productUnitTempIds`.
6. Rule list display should show all selected unit names, not only one.

For standalone create/update:

```ts
const savedUnitIds = form.productUnitIds.filter(Boolean);

return {
  // existing rule payload...
  productUnitIds: savedUnitIds.length ? savedUnitIds : undefined,
};
```

If selected units include temp values and `productId` exists, block standalone save with a Turkish warning such as:

```txt
Önce ürün birimini kaydedin, sonra kuralı kaydedebilirsiniz.
```

For new unsaved products, draft rules may carry `productUnitTempIds` and the full save payload will resolve them.

## Detail screens

In `src/modules/products/components/detail/ProductDetailTabs.tsx`:

For sales plans, replace single unit display with a joined list:

```ts
const unitNames = lo.productUnits?.length
  ? lo.productUnits.map((unit) => unit.name).join(", ")
  : "-";
```

For pricing rules, do the same:

```ts
const unitNames = rule.productUnits?.length
  ? rule.productUnits.map((unit) => unit.name).join(", ")
  : "-";
```

Keep the existing ProductUnits tab.

## Mock and tests

Update fixtures so at least one offering and one pricing rule include the new array fields:

```ts
productUnitIds: ["product-unit-user", "product-unit-branch"],
productUnits: [
  { id: "product-unit-user", name: "Kullanıcı", code: "USER", /* existing fields */ },
  { id: "product-unit-branch", name: "Şube", code: "BRANCH", /* existing fields */ },
],
```

Update mapping tests to assert:

```ts
expect(form.licenseOfferings[0].productUnitIds).toEqual(["product-unit-user", "product-unit-branch"]);
expect(payload.licenseOfferings[0].productUnitIds).toEqual(["product-unit-user", "product-unit-branch"]);
expect(payload.pricingRules[0].productUnitIds).toEqual(["product-unit-user", "product-unit-branch"]);
```

Also add a draft/new product case:

```ts
productUnitTempIds: ["product-unit-temp-user", "product-unit-temp-branch"]
```

and assert the full save payload keeps those temp IDs.

## Acceptance criteria

- A sales plan can be assigned to more than one product unit.
- A pricing rule can be assigned to more than one product unit.
- Top-level `productUnitId`, `productUnitCode`, `productUnitName`, and `productUnitTempId` are removed from sales plan/pricing rule form state and payloads.
- Full product create can link new product units to new sales plans/rules through `productUnitTempIds`.
- Standalone create/update only sends saved `productUnitIds`.
- Product detail refresh keeps all selected units visible.
- Pricing rule formula unit fields remain independent from rule scope product units.
- `npm.cmd run build` succeeds. If ESLint is blocked by the repo's TypeScript ESLint setup, use `npm.cmd run build` as the validation fallback.
