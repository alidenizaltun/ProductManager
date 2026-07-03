using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Repository.Shared.Abstract
{
    public interface IProductOperationsRepository
    {
        Task<IReadOnlyList<ProductDto>> GetProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LookupItemDto>> GetProductLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductDetailDto?> GetProductDetailByIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductDto> CreateProductAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default);
        Task<ProductDto> CreateProductFullAsync(CreateProductFullRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductAsync(Guid productId, UpdateProductRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductFullAsync(Guid productId, UpdateProductFullRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductAttributeDefinitionDto>> GetAttributeDefinitionsAsync(CancellationToken cancellationToken = default);
        Task<ProductAttributeDefinitionDto?> GetAttributeDefinitionByIdAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default);
        Task<ProductAttributeDefinitionDto> CreateAttributeDefinitionAsync(CreateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAttributeDefinitionAsync(Guid attributeDefinitionId, UpdateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAttributeDefinitionAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductAttributeValueDto>> GetProductAttributeValuesAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductAttributeValueDto?> GetAttributeValueByIdAsync(Guid attributeValueId, CancellationToken cancellationToken = default);
        Task<ProductAttributeValueDto> CreateAttributeValueAsync(CreateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAttributeValueAsync(Guid attributeValueId, UpdateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAttributeValueAsync(Guid attributeValueId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LookupItemDto>> GetCategoryLookupsAsync(CancellationToken cancellationToken = default);
        Task<ProductCategoryDto?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateCategoryAsync(Guid categoryId, UpdateProductCategoryRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductCategoryMapDto>> GetProductCategoryMapsAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductCategoryMapDto?> GetCategoryMapByIdAsync(Guid categoryMapId, CancellationToken cancellationToken = default);
        Task<ProductCategoryMapDto> CreateCategoryMapAsync(CreateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateCategoryMapAsync(Guid categoryMapId, UpdateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteCategoryMapAsync(Guid categoryMapId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductMediaDto>> GetProductMediaAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductMediaDto?> GetMediaByIdAsync(Guid mediaId, CancellationToken cancellationToken = default);
        Task<ProductMediaDto> CreateMediaAsync(CreateProductMediaRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateMediaAsync(Guid mediaId, UpdateProductMediaRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteMediaAsync(Guid mediaId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductBundleItemDto>> GetBundleItemsAsync(Guid bundleProductId, CancellationToken cancellationToken = default);
        Task<ProductBundleItemDto?> GetBundleItemByIdAsync(Guid bundleItemId, CancellationToken cancellationToken = default);
        Task<ProductBundleItemDto> CreateBundleItemAsync(CreateProductBundleItemRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateBundleItemAsync(Guid bundleItemId, UpdateProductBundleItemRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteBundleItemAsync(Guid bundleItemId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductVariantDto>> GetProductVariantsAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductVariantDto?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default);
        Task<ProductVariantDto> CreateVariantAsync(CreateProductVariantRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateVariantAsync(Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductPriceDto>> GetProductPricesAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductPriceDto?> GetPriceByIdAsync(Guid priceId, CancellationToken cancellationToken = default);
        Task<ProductPriceDto> CreatePriceAsync(CreateProductPriceRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdatePriceAsync(Guid priceId, UpdateProductPriceRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeletePriceAsync(Guid priceId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductPricingRuleDto>> GetProductPricingRulesAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductPricingRuleDto?> GetPricingRuleByIdAsync(Guid pricingRuleId, CancellationToken cancellationToken = default);
        Task<ProductPricingRuleDto> CreatePricingRuleAsync(CreateProductPricingRuleRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdatePricingRuleAsync(Guid pricingRuleId, UpdateProductPricingRuleRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeletePricingRuleAsync(Guid pricingRuleId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductInventoryDto>> GetProductInventoriesAsync(ProductInventoryFilterDto filter, CancellationToken cancellationToken = default);
        Task<ProductInventoryDto?> GetInventoryByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);
        Task<ProductInventoryDto> CreateInventoryAsync(CreateProductInventoryRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateInventoryAsync(Guid inventoryId, UpdateProductInventoryRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        Task<ProductPhysicalProfileDto?> GetPhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductPhysicalProfileDto> UpsertPhysicalProfileAsync(Guid productId, UpsertProductPhysicalProfileRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeletePhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<ProductSoftwareProfileDto?> GetSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductSoftwareProfileDto> UpsertSoftwareProfileAsync(Guid productId, UpsertProductSoftwareProfileRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<ProductServiceProfileDto?> GetServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductServiceProfileDto> UpsertServiceProfileAsync(Guid productId, UpsertProductServiceProfileRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<ProductSubscriptionProfileDto?> GetSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductSubscriptionProfileDto> UpsertSubscriptionProfileAsync(Guid productId, UpsertProductSubscriptionProfileRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductSupplierDto>> GetSuppliersAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LookupItemDto>> GetSupplierLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<ProductSupplierDto?> GetSupplierByIdAsync(Guid supplierId, CancellationToken cancellationToken = default);
        Task<ProductSupplierDto> CreateSupplierAsync(CreateProductSupplierRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateSupplierAsync(Guid supplierId, UpdateProductSupplierRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteSupplierAsync(Guid supplierId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductSupplierMapDto>> GetProductSupplierMapsAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductSupplierMapDto?> GetSupplierMapByIdAsync(Guid supplierMapId, CancellationToken cancellationToken = default);
        Task<ProductSupplierMapDto> CreateSupplierMapAsync(CreateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateSupplierMapAsync(Guid supplierMapId, UpdateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteSupplierMapAsync(Guid supplierMapId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WarehouseDto>> GetWarehousesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LookupItemDto>> GetWarehouseLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<WarehouseDto?> GetWarehouseByIdAsync(Guid warehouseId, CancellationToken cancellationToken = default);
        Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InventoryTransactionDto>> GetInventoryTransactionsAsync(InventoryTransactionFilterDto filter, CancellationToken cancellationToken = default);
        Task<InventoryTransactionDto?> GetInventoryTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
        Task<InventoryTransactionDto> CreateInventoryTransactionAsync(CreateInventoryTransactionRequestDto request, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<InventoryReservationDto>> GetInventoryReservationsAsync(InventoryReservationFilterDto filter, CancellationToken cancellationToken = default);
        Task<InventoryReservationDto?> GetInventoryReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
        Task<InventoryReservationDto> CreateInventoryReservationAsync(CreateInventoryReservationRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateInventoryReservationStatusAsync(Guid reservationId, UpdateInventoryReservationStatusRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteInventoryReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductPriceListDto>> GetPriceListsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LookupItemDto>> GetPriceListLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<ProductPriceListDto?> GetPriceListByIdAsync(Guid priceListId, CancellationToken cancellationToken = default);
        Task<ProductPriceListDto> CreatePriceListAsync(CreateProductPriceListRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdatePriceListAsync(Guid priceListId, UpdateProductPriceListRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeletePriceListAsync(Guid priceListId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductPriceListItemDto>> GetPriceListItemsAsync(Guid priceListId, CancellationToken cancellationToken = default);
        Task<ProductPriceListItemDto?> GetPriceListItemByIdAsync(Guid priceListItemId, CancellationToken cancellationToken = default);
        Task<ProductPriceListItemDto> CreatePriceListItemAsync(CreateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdatePriceListItemAsync(Guid priceListItemId, UpdateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeletePriceListItemAsync(Guid priceListItemId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductModuleDto>> GetProductModulesAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductModuleDto?> GetProductModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken = default);
        Task<ProductModuleDto> CreateProductModuleAsync(CreateProductModuleRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductModuleAsync(Guid moduleId, UpdateProductModuleRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductModuleAsync(Guid moduleId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductModuleOfferingPriceDto>> GetModuleOfferingPricesAsync(Guid moduleId, CancellationToken cancellationToken = default);
        Task<ProductModuleOfferingPriceDto?> GetModuleOfferingPriceByIdAsync(Guid priceId, CancellationToken cancellationToken = default);
        Task<ProductModuleOfferingPriceDto> CreateModuleOfferingPriceAsync(CreateProductModuleOfferingPriceRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateModuleOfferingPriceAsync(Guid priceId, UpdateProductModuleOfferingPriceRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteModuleOfferingPriceAsync(Guid priceId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProductLicenseOfferingDto>> GetProductLicenseOfferingsAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductLicenseOfferingDto?> GetProductLicenseOfferingByIdAsync(Guid offeringId, CancellationToken cancellationToken = default);
        Task<ProductLicenseOfferingDto> CreateProductLicenseOfferingAsync(CreateProductLicenseOfferingRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductLicenseOfferingAsync(Guid offeringId, UpdateProductLicenseOfferingRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductLicenseOfferingAsync(Guid offeringId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UnitDefinitionDto>> GetUnitDefinitionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LookupItemDto>> GetUnitDefinitionLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<UnitDefinitionDto?> GetUnitDefinitionByIdAsync(Guid unitDefinitionId, CancellationToken cancellationToken = default);
        Task<UnitDefinitionDto> CreateUnitDefinitionAsync(CreateUnitDefinitionRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateUnitDefinitionAsync(Guid unitDefinitionId, UpdateUnitDefinitionRequestDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteUnitDefinitionAsync(Guid unitDefinitionId, CancellationToken cancellationToken = default);
    }
}
