using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;

namespace ProductManager.Service.Concrete
{
    public sealed class ProductOperationsService : IProductOperationsService
    {
        private readonly IProductOperationsRepository _repository;

        public ProductOperationsService(IProductOperationsRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<ProductDto>> GetProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetProductsAsync(filter, cancellationToken);

        public Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductByIdAsync(productId, cancellationToken);

        public Task<ProductDto> CreateProductAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateProductAsync(request, cancellationToken);

        public Task<bool> UpdateProductAsync(Guid productId, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateProductAsync(productId, request, cancellationToken);

        public Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.DeleteProductAsync(productId, cancellationToken);

        public Task<IReadOnlyList<ProductAttributeDefinitionDto>> GetAttributeDefinitionsAsync(CancellationToken cancellationToken = default)
            => _repository.GetAttributeDefinitionsAsync(cancellationToken);

        public Task<ProductAttributeDefinitionDto?> GetAttributeDefinitionByIdAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default)
            => _repository.GetAttributeDefinitionByIdAsync(attributeDefinitionId, cancellationToken);

        public Task<ProductAttributeDefinitionDto> CreateAttributeDefinitionAsync(CreateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateAttributeDefinitionAsync(request, cancellationToken);

        public Task<bool> UpdateAttributeDefinitionAsync(Guid attributeDefinitionId, UpdateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateAttributeDefinitionAsync(attributeDefinitionId, request, cancellationToken);

        public Task<bool> DeleteAttributeDefinitionAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default)
            => _repository.DeleteAttributeDefinitionAsync(attributeDefinitionId, cancellationToken);

        public Task<IReadOnlyList<ProductAttributeValueDto>> GetProductAttributeValuesAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductAttributeValuesAsync(productId, cancellationToken);

        public Task<ProductAttributeValueDto?> GetAttributeValueByIdAsync(Guid attributeValueId, CancellationToken cancellationToken = default)
            => _repository.GetAttributeValueByIdAsync(attributeValueId, cancellationToken);

        public Task<ProductAttributeValueDto> CreateAttributeValueAsync(CreateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateAttributeValueAsync(request, cancellationToken);

        public Task<bool> UpdateAttributeValueAsync(Guid attributeValueId, UpdateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateAttributeValueAsync(attributeValueId, request, cancellationToken);

        public Task<bool> DeleteAttributeValueAsync(Guid attributeValueId, CancellationToken cancellationToken = default)
            => _repository.DeleteAttributeValueAsync(attributeValueId, cancellationToken);

        public Task<IReadOnlyList<ProductCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
            => _repository.GetCategoriesAsync(cancellationToken);

        public Task<ProductCategoryDto?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
            => _repository.GetCategoryByIdAsync(categoryId, cancellationToken);

        public Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateCategoryAsync(request, cancellationToken);

        public Task<bool> UpdateCategoryAsync(Guid categoryId, UpdateProductCategoryRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateCategoryAsync(categoryId, request, cancellationToken);

        public Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
            => _repository.DeleteCategoryAsync(categoryId, cancellationToken);

        public Task<IReadOnlyList<ProductCategoryMapDto>> GetProductCategoryMapsAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductCategoryMapsAsync(productId, cancellationToken);

        public Task<ProductCategoryMapDto?> GetCategoryMapByIdAsync(Guid categoryMapId, CancellationToken cancellationToken = default)
            => _repository.GetCategoryMapByIdAsync(categoryMapId, cancellationToken);

        public Task<ProductCategoryMapDto> CreateCategoryMapAsync(CreateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateCategoryMapAsync(request, cancellationToken);

        public Task<bool> UpdateCategoryMapAsync(Guid categoryMapId, UpdateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateCategoryMapAsync(categoryMapId, request, cancellationToken);

        public Task<bool> DeleteCategoryMapAsync(Guid categoryMapId, CancellationToken cancellationToken = default)
            => _repository.DeleteCategoryMapAsync(categoryMapId, cancellationToken);

        public Task<IReadOnlyList<ProductMediaDto>> GetProductMediaAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductMediaAsync(productId, cancellationToken);

        public Task<ProductMediaDto?> GetMediaByIdAsync(Guid mediaId, CancellationToken cancellationToken = default)
            => _repository.GetMediaByIdAsync(mediaId, cancellationToken);

        public Task<ProductMediaDto> CreateMediaAsync(CreateProductMediaRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateMediaAsync(request, cancellationToken);

        public Task<bool> UpdateMediaAsync(Guid mediaId, UpdateProductMediaRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateMediaAsync(mediaId, request, cancellationToken);

        public Task<bool> DeleteMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
            => _repository.DeleteMediaAsync(mediaId, cancellationToken);

        public Task<IReadOnlyList<ProductBundleItemDto>> GetBundleItemsAsync(Guid bundleProductId, CancellationToken cancellationToken = default)
            => _repository.GetBundleItemsAsync(bundleProductId, cancellationToken);

        public Task<ProductBundleItemDto?> GetBundleItemByIdAsync(Guid bundleItemId, CancellationToken cancellationToken = default)
            => _repository.GetBundleItemByIdAsync(bundleItemId, cancellationToken);

        public Task<ProductBundleItemDto> CreateBundleItemAsync(CreateProductBundleItemRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateBundleItemAsync(request, cancellationToken);

        public Task<bool> UpdateBundleItemAsync(Guid bundleItemId, UpdateProductBundleItemRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateBundleItemAsync(bundleItemId, request, cancellationToken);

        public Task<bool> DeleteBundleItemAsync(Guid bundleItemId, CancellationToken cancellationToken = default)
            => _repository.DeleteBundleItemAsync(bundleItemId, cancellationToken);

        public Task<IReadOnlyList<ProductVariantDto>> GetProductVariantsAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductVariantsAsync(productId, cancellationToken);

        public Task<ProductVariantDto?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
            => _repository.GetVariantByIdAsync(variantId, cancellationToken);

        public Task<ProductVariantDto> CreateVariantAsync(CreateProductVariantRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateVariantAsync(request, cancellationToken);

        public Task<bool> UpdateVariantAsync(Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateVariantAsync(variantId, request, cancellationToken);

        public Task<bool> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
            => _repository.DeleteVariantAsync(variantId, cancellationToken);

        public Task<IReadOnlyList<ProductPriceDto>> GetProductPricesAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductPricesAsync(productId, cancellationToken);

        public Task<ProductPriceDto?> GetPriceByIdAsync(Guid priceId, CancellationToken cancellationToken = default)
            => _repository.GetPriceByIdAsync(priceId, cancellationToken);

        public Task<ProductPriceDto> CreatePriceAsync(CreateProductPriceRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreatePriceAsync(request, cancellationToken);

        public Task<bool> UpdatePriceAsync(Guid priceId, UpdateProductPriceRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdatePriceAsync(priceId, request, cancellationToken);

        public Task<bool> DeletePriceAsync(Guid priceId, CancellationToken cancellationToken = default)
            => _repository.DeletePriceAsync(priceId, cancellationToken);

        public Task<IReadOnlyList<ProductInventoryDto>> GetProductInventoriesAsync(ProductInventoryFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetProductInventoriesAsync(filter, cancellationToken);

        public Task<ProductInventoryDto?> GetInventoryByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
            => _repository.GetInventoryByIdAsync(inventoryId, cancellationToken);

        public Task<ProductInventoryDto> CreateInventoryAsync(CreateProductInventoryRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateInventoryAsync(request, cancellationToken);

        public Task<bool> UpdateInventoryAsync(Guid inventoryId, UpdateProductInventoryRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateInventoryAsync(inventoryId, request, cancellationToken);

        public Task<bool> DeleteInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default)
            => _repository.DeleteInventoryAsync(inventoryId, cancellationToken);

        public Task<ProductPhysicalProfileDto?> GetPhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetPhysicalProfileAsync(productId, cancellationToken);

        public Task<ProductPhysicalProfileDto> UpsertPhysicalProfileAsync(Guid productId, UpsertProductPhysicalProfileRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpsertPhysicalProfileAsync(productId, request, cancellationToken);

        public Task<bool> DeletePhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.DeletePhysicalProfileAsync(productId, cancellationToken);

        public Task<ProductSoftwareProfileDto?> GetSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetSoftwareProfileAsync(productId, cancellationToken);

        public Task<ProductSoftwareProfileDto> UpsertSoftwareProfileAsync(Guid productId, UpsertProductSoftwareProfileRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpsertSoftwareProfileAsync(productId, request, cancellationToken);

        public Task<bool> DeleteSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.DeleteSoftwareProfileAsync(productId, cancellationToken);

        public Task<ProductServiceProfileDto?> GetServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetServiceProfileAsync(productId, cancellationToken);

        public Task<ProductServiceProfileDto> UpsertServiceProfileAsync(Guid productId, UpsertProductServiceProfileRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpsertServiceProfileAsync(productId, request, cancellationToken);

        public Task<bool> DeleteServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.DeleteServiceProfileAsync(productId, cancellationToken);

        public Task<ProductSubscriptionProfileDto?> GetSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetSubscriptionProfileAsync(productId, cancellationToken);

        public Task<ProductSubscriptionProfileDto> UpsertSubscriptionProfileAsync(Guid productId, UpsertProductSubscriptionProfileRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpsertSubscriptionProfileAsync(productId, request, cancellationToken);

        public Task<bool> DeleteSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.DeleteSubscriptionProfileAsync(productId, cancellationToken);

        public Task<IReadOnlyList<ProductSupplierDto>> GetSuppliersAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetSuppliersAsync(includeInactive, cancellationToken);

        public Task<ProductSupplierDto?> GetSupplierByIdAsync(Guid supplierId, CancellationToken cancellationToken = default)
            => _repository.GetSupplierByIdAsync(supplierId, cancellationToken);

        public Task<ProductSupplierDto> CreateSupplierAsync(CreateProductSupplierRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateSupplierAsync(request, cancellationToken);

        public Task<bool> UpdateSupplierAsync(Guid supplierId, UpdateProductSupplierRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateSupplierAsync(supplierId, request, cancellationToken);

        public Task<bool> DeleteSupplierAsync(Guid supplierId, CancellationToken cancellationToken = default)
            => _repository.DeleteSupplierAsync(supplierId, cancellationToken);

        public Task<IReadOnlyList<ProductSupplierMapDto>> GetProductSupplierMapsAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductSupplierMapsAsync(productId, cancellationToken);

        public Task<ProductSupplierMapDto?> GetSupplierMapByIdAsync(Guid supplierMapId, CancellationToken cancellationToken = default)
            => _repository.GetSupplierMapByIdAsync(supplierMapId, cancellationToken);

        public Task<ProductSupplierMapDto> CreateSupplierMapAsync(CreateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateSupplierMapAsync(request, cancellationToken);

        public Task<bool> UpdateSupplierMapAsync(Guid supplierMapId, UpdateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateSupplierMapAsync(supplierMapId, request, cancellationToken);

        public Task<bool> DeleteSupplierMapAsync(Guid supplierMapId, CancellationToken cancellationToken = default)
            => _repository.DeleteSupplierMapAsync(supplierMapId, cancellationToken);

        public Task<IReadOnlyList<WarehouseDto>> GetWarehousesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetWarehousesAsync(includeInactive, cancellationToken);

        public Task<WarehouseDto?> GetWarehouseByIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
            => _repository.GetWarehouseByIdAsync(warehouseId, cancellationToken);

        public Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateWarehouseAsync(request, cancellationToken);

        public Task<bool> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateWarehouseAsync(warehouseId, request, cancellationToken);

        public Task<bool> DeleteWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default)
            => _repository.DeleteWarehouseAsync(warehouseId, cancellationToken);

        public Task<IReadOnlyList<InventoryTransactionDto>> GetInventoryTransactionsAsync(InventoryTransactionFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetInventoryTransactionsAsync(filter, cancellationToken);

        public Task<InventoryTransactionDto> CreateInventoryTransactionAsync(CreateInventoryTransactionRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateInventoryTransactionAsync(request, cancellationToken);

        public Task<IReadOnlyList<InventoryReservationDto>> GetInventoryReservationsAsync(InventoryReservationFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetInventoryReservationsAsync(filter, cancellationToken);

        public Task<InventoryReservationDto> CreateInventoryReservationAsync(CreateInventoryReservationRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreateInventoryReservationAsync(request, cancellationToken);

        public Task<bool> UpdateInventoryReservationStatusAsync(Guid reservationId, UpdateInventoryReservationStatusRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdateInventoryReservationStatusAsync(reservationId, request, cancellationToken);

        public Task<bool> DeleteInventoryReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
            => _repository.DeleteInventoryReservationAsync(reservationId, cancellationToken);

        public Task<IReadOnlyList<ProductPriceListDto>> GetPriceListsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetPriceListsAsync(includeInactive, cancellationToken);

        public Task<ProductPriceListDto?> GetPriceListByIdAsync(Guid priceListId, CancellationToken cancellationToken = default)
            => _repository.GetPriceListByIdAsync(priceListId, cancellationToken);

        public Task<ProductPriceListDto> CreatePriceListAsync(CreateProductPriceListRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreatePriceListAsync(request, cancellationToken);

        public Task<bool> UpdatePriceListAsync(Guid priceListId, UpdateProductPriceListRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdatePriceListAsync(priceListId, request, cancellationToken);

        public Task<bool> DeletePriceListAsync(Guid priceListId, CancellationToken cancellationToken = default)
            => _repository.DeletePriceListAsync(priceListId, cancellationToken);

        public Task<IReadOnlyList<ProductPriceListItemDto>> GetPriceListItemsAsync(Guid priceListId, CancellationToken cancellationToken = default)
            => _repository.GetPriceListItemsAsync(priceListId, cancellationToken);

        public Task<ProductPriceListItemDto?> GetPriceListItemByIdAsync(Guid priceListItemId, CancellationToken cancellationToken = default)
            => _repository.GetPriceListItemByIdAsync(priceListItemId, cancellationToken);

        public Task<ProductPriceListItemDto> CreatePriceListItemAsync(CreateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default)
            => _repository.CreatePriceListItemAsync(request, cancellationToken);

        public Task<bool> UpdatePriceListItemAsync(Guid priceListItemId, UpdateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default)
            => _repository.UpdatePriceListItemAsync(priceListItemId, request, cancellationToken);

        public Task<bool> DeletePriceListItemAsync(Guid priceListItemId, CancellationToken cancellationToken = default)
            => _repository.DeletePriceListItemAsync(priceListItemId, cancellationToken);
    }
}
