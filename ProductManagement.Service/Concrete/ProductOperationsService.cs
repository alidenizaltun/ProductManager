using Microsoft.Data.SqlClient;
using ProductManagement.Repository.Shared.Abstract;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Exceptions;

namespace ProductManagement.Service.Concrete
{
    public sealed partial class ProductOperationsService : IProductOperationsService
    {
        private readonly IProductOperationsRepository _repository;

        public ProductOperationsService(IProductOperationsRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<ProductDto>> GetProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetProductsAsync(filter, cancellationToken);

        public Task<IReadOnlyList<LookupItemDto>> GetProductLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetProductLookupsAsync(includeInactive, cancellationToken);

        public async Task<ProductReferenceLookupsDto> GetReferenceLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var productsTask = _repository.GetProductLookupsAsync(includeInactive, cancellationToken);
            var categoriesTask = _repository.GetCategoryLookupsAsync(cancellationToken);
            var warehousesTask = _repository.GetWarehouseLookupsAsync(includeInactive, cancellationToken);
            var suppliersTask = _repository.GetSupplierLookupsAsync(includeInactive, cancellationToken);
            var priceListsTask = _repository.GetPriceListLookupsAsync(includeInactive, cancellationToken);
            var unitDefinitionsTask = _repository.GetUnitDefinitionLookupsAsync(includeInactive, cancellationToken);

            await Task.WhenAll(productsTask, categoriesTask, warehousesTask, suppliersTask, priceListsTask, unitDefinitionsTask);

            return new ProductReferenceLookupsDto
            {
                Products = await productsTask,
                Categories = await categoriesTask,
                Warehouses = await warehousesTask,
                Suppliers = await suppliersTask,
                PriceLists = await priceListsTask,
                UnitDefinitions = await unitDefinitionsTask
            };
        }

        public Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductByIdAsync(productId, cancellationToken);

        public Task<ProductDetailDto?> GetProductDetailByIdAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductDetailByIdAsync(productId, cancellationToken);

        public Task<ProductDto> CreateProductAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateProductAsync(request, cancellationToken));

        public Task<ProductDto> CreateProductFullAsync(CreateProductFullRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateProductFullAsync(request, cancellationToken));

        public Task<bool> UpdateProductAsync(Guid productId, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
        => ExecuteWithSqlMapping(() => _repository.UpdateProductAsync(productId, request, cancellationToken));

        public Task<bool> UpdateProductFullAsync(Guid productId, UpdateProductFullRequestDto request, CancellationToken cancellationToken = default)
        => ExecuteWithSqlMapping(() => _repository.UpdateProductFullAsync(productId, request, cancellationToken));

        public Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
                   => ExecuteWithSqlMapping(() => _repository.DeleteProductAsync(productId, cancellationToken));

        public Task<IReadOnlyList<ProductAttributeDefinitionDto>> GetAttributeDefinitionsAsync(CancellationToken cancellationToken = default)
            => _repository.GetAttributeDefinitionsAsync(cancellationToken);

        public Task<ProductAttributeDefinitionDto?> GetAttributeDefinitionByIdAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default)
            => _repository.GetAttributeDefinitionByIdAsync(attributeDefinitionId, cancellationToken);

        public Task<ProductAttributeDefinitionDto> CreateAttributeDefinitionAsync(CreateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateAttributeDefinitionAsync(request, cancellationToken));

        public Task<bool> UpdateAttributeDefinitionAsync(Guid attributeDefinitionId, UpdateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateAttributeDefinitionAsync(attributeDefinitionId, request, cancellationToken));

        public Task<bool> DeleteAttributeDefinitionAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteAttributeDefinitionAsync(attributeDefinitionId, cancellationToken));

        public Task<IReadOnlyList<ProductAttributeValueDto>> GetProductAttributeValuesAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductAttributeValuesAsync(productId, cancellationToken);

        public Task<ProductAttributeValueDto?> GetAttributeValueByIdAsync(Guid attributeValueId, CancellationToken cancellationToken = default)
            => _repository.GetAttributeValueByIdAsync(attributeValueId, cancellationToken);

        public Task<ProductAttributeValueDto> CreateAttributeValueAsync(CreateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateAttributeValueAsync(request, cancellationToken));

        public Task<bool> UpdateAttributeValueAsync(Guid attributeValueId, UpdateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateAttributeValueAsync(attributeValueId, request, cancellationToken));

        public Task<bool> DeleteAttributeValueAsync(Guid attributeValueId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteAttributeValueAsync(attributeValueId, cancellationToken));

        public Task<IReadOnlyList<ProductCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
            => _repository.GetCategoriesAsync(cancellationToken);

        public Task<IReadOnlyList<LookupItemDto>> GetCategoryLookupsAsync(CancellationToken cancellationToken = default)
            => _repository.GetCategoryLookupsAsync(cancellationToken);

        public Task<ProductCategoryDto?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
            => _repository.GetCategoryByIdAsync(categoryId, cancellationToken);

        public Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateCategoryAsync(request, cancellationToken));

        public Task<bool> UpdateCategoryAsync(Guid categoryId, UpdateProductCategoryRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateCategoryAsync(categoryId, request, cancellationToken));

        public Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteCategoryAsync(categoryId, cancellationToken));

        public Task<IReadOnlyList<ProductCategoryMapDto>> GetProductCategoryMapsAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductCategoryMapsAsync(productId, cancellationToken);

        public Task<ProductCategoryMapDto?> GetCategoryMapByIdAsync(Guid categoryMapId, CancellationToken cancellationToken = default)
            => _repository.GetCategoryMapByIdAsync(categoryMapId, cancellationToken);

        public Task<ProductCategoryMapDto> CreateCategoryMapAsync(CreateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateCategoryMapAsync(request, cancellationToken));

        public Task<bool> UpdateCategoryMapAsync(Guid categoryMapId, UpdateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateCategoryMapAsync(categoryMapId, request, cancellationToken));

        public Task<bool> DeleteCategoryMapAsync(Guid categoryMapId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteCategoryMapAsync(categoryMapId, cancellationToken));

        public Task<IReadOnlyList<ProductMediaDto>> GetProductMediaAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductMediaAsync(productId, cancellationToken);

        public Task<ProductMediaDto?> GetMediaByIdAsync(Guid mediaId, CancellationToken cancellationToken = default)
            => _repository.GetMediaByIdAsync(mediaId, cancellationToken);

        public Task<ProductMediaDto> CreateMediaAsync(CreateProductMediaRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateMediaAsync(request, cancellationToken));

        public Task<bool> UpdateMediaAsync(Guid mediaId, UpdateProductMediaRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateMediaAsync(mediaId, request, cancellationToken));

        public Task<bool> DeleteMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteMediaAsync(mediaId, cancellationToken));

        public Task<IReadOnlyList<ProductBundleItemDto>> GetBundleItemsAsync(Guid bundleProductId, CancellationToken cancellationToken = default)
            => _repository.GetBundleItemsAsync(bundleProductId, cancellationToken);

        public Task<ProductBundleItemDto?> GetBundleItemByIdAsync(Guid bundleItemId, CancellationToken cancellationToken = default)
            => _repository.GetBundleItemByIdAsync(bundleItemId, cancellationToken);

        public Task<ProductBundleItemDto> CreateBundleItemAsync(CreateProductBundleItemRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateBundleItemAsync(request, cancellationToken));

        public Task<bool> UpdateBundleItemAsync(Guid bundleItemId, UpdateProductBundleItemRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateBundleItemAsync(bundleItemId, request, cancellationToken));

        public Task<bool> DeleteBundleItemAsync(Guid bundleItemId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteBundleItemAsync(bundleItemId, cancellationToken));

        public Task<IReadOnlyList<ProductVariantDto>> GetProductVariantsAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductVariantsAsync(productId, cancellationToken);

        public Task<ProductVariantDto?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
            => _repository.GetVariantByIdAsync(variantId, cancellationToken);

        public Task<ProductVariantDto> CreateVariantAsync(CreateProductVariantRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateVariantAsync(request, cancellationToken));

        public Task<bool> UpdateVariantAsync(Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateVariantAsync(variantId, request, cancellationToken));

        public Task<bool> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteVariantAsync(variantId, cancellationToken));

        public Task<IReadOnlyList<ProductPriceDto>> GetProductPricesAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductPricesAsync(productId, cancellationToken);

        public Task<ProductPriceDto?> GetPriceByIdAsync(Guid priceId, CancellationToken cancellationToken = default)
            => _repository.GetPriceByIdAsync(priceId, cancellationToken);

        public Task<ProductPriceDto> CreatePriceAsync(CreateProductPriceRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreatePriceAsync(request, cancellationToken));

        public Task<bool> UpdatePriceAsync(Guid priceId, UpdateProductPriceRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdatePriceAsync(priceId, request, cancellationToken));

        public Task<bool> DeletePriceAsync(Guid priceId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeletePriceAsync(priceId, cancellationToken));

        public Task<IReadOnlyList<ProductPricingRuleDto>> GetProductPricingRulesAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductPricingRulesAsync(productId, cancellationToken);

        public Task<ProductPricingRuleDto?> GetPricingRuleByIdAsync(Guid pricingRuleId, CancellationToken cancellationToken = default)
            => _repository.GetPricingRuleByIdAsync(pricingRuleId, cancellationToken);

        public Task<ProductPricingRuleDto> CreatePricingRuleAsync(CreateProductPricingRuleRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreatePricingRuleAsync(request, cancellationToken));

        public Task<bool> UpdatePricingRuleAsync(Guid pricingRuleId, UpdateProductPricingRuleRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdatePricingRuleAsync(pricingRuleId, request, cancellationToken));

        public Task<bool> DeletePricingRuleAsync(Guid pricingRuleId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeletePricingRuleAsync(pricingRuleId, cancellationToken));

        public Task<bool> ReorderPricingRulesAsync(Guid productId, IReadOnlyList<Guid> orderedPricingRuleIds, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.ReorderPricingRulesAsync(productId, orderedPricingRuleIds, cancellationToken));

        public Task<IReadOnlyList<ProductUnitDto>> GetProductUnitsAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductUnitsAsync(productId, cancellationToken);

        public Task<ProductUnitDto?> GetProductUnitByIdAsync(Guid productUnitId, CancellationToken cancellationToken = default)
            => _repository.GetProductUnitByIdAsync(productUnitId, cancellationToken);

        public Task<ProductUnitDto> CreateProductUnitAsync(CreateProductUnitRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateProductUnitAsync(request, cancellationToken));

        public Task<bool> UpdateProductUnitAsync(Guid productUnitId, UpdateProductUnitRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateProductUnitAsync(productUnitId, request, cancellationToken));

        public Task<bool> DeleteProductUnitAsync(Guid productUnitId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteProductUnitAsync(productUnitId, cancellationToken));

        public Task<IReadOnlyList<ProductInventoryDto>> GetProductInventoriesAsync(ProductInventoryFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetProductInventoriesAsync(filter, cancellationToken);

        public Task<ProductInventoryDto?> GetInventoryByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
            => _repository.GetInventoryByIdAsync(inventoryId, cancellationToken);

        public Task<ProductInventoryDto> CreateInventoryAsync(CreateProductInventoryRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateInventoryAsync(request, cancellationToken));

        public Task<bool> UpdateInventoryAsync(Guid inventoryId, UpdateProductInventoryRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateInventoryAsync(inventoryId, request, cancellationToken));

        public Task<bool> DeleteInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteInventoryAsync(inventoryId, cancellationToken));

        public Task<ProductPhysicalProfileDto?> GetPhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetPhysicalProfileAsync(productId, cancellationToken);

        public Task<ProductPhysicalProfileDto> UpsertPhysicalProfileAsync(Guid productId, UpsertProductPhysicalProfileRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpsertPhysicalProfileAsync(productId, request, cancellationToken));

        public Task<bool> DeletePhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeletePhysicalProfileAsync(productId, cancellationToken));

        public Task<ProductSoftwareProfileDto?> GetSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetSoftwareProfileAsync(productId, cancellationToken);

        public Task<ProductSoftwareProfileDto> UpsertSoftwareProfileAsync(Guid productId, UpsertProductSoftwareProfileRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpsertSoftwareProfileAsync(productId, request, cancellationToken));

        public Task<bool> DeleteSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteSoftwareProfileAsync(productId, cancellationToken));

        public Task<ProductServiceProfileDto?> GetServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetServiceProfileAsync(productId, cancellationToken);

        public Task<ProductServiceProfileDto> UpsertServiceProfileAsync(Guid productId, UpsertProductServiceProfileRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpsertServiceProfileAsync(productId, request, cancellationToken));

        public Task<bool> DeleteServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteServiceProfileAsync(productId, cancellationToken));

        public Task<ProductSubscriptionProfileDto?> GetSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetSubscriptionProfileAsync(productId, cancellationToken);

        public Task<ProductSubscriptionProfileDto> UpsertSubscriptionProfileAsync(Guid productId, UpsertProductSubscriptionProfileRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpsertSubscriptionProfileAsync(productId, request, cancellationToken));

        public Task<bool> DeleteSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteSubscriptionProfileAsync(productId, cancellationToken));

        public Task<IReadOnlyList<ProductSupplierDto>> GetSuppliersAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetSuppliersAsync(includeInactive, cancellationToken);

        public Task<IReadOnlyList<LookupItemDto>> GetSupplierLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetSupplierLookupsAsync(includeInactive, cancellationToken);

        public Task<ProductSupplierDto?> GetSupplierByIdAsync(Guid supplierId, CancellationToken cancellationToken = default)
            => _repository.GetSupplierByIdAsync(supplierId, cancellationToken);

        public Task<ProductSupplierDto> CreateSupplierAsync(CreateProductSupplierRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateSupplierAsync(request, cancellationToken));

        public Task<bool> UpdateSupplierAsync(Guid supplierId, UpdateProductSupplierRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateSupplierAsync(supplierId, request, cancellationToken));

        public Task<bool> DeleteSupplierAsync(Guid supplierId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteSupplierAsync(supplierId, cancellationToken));

        public Task<IReadOnlyList<ProductSupplierMapDto>> GetProductSupplierMapsAsync(Guid productId, CancellationToken cancellationToken = default)
            => _repository.GetProductSupplierMapsAsync(productId, cancellationToken);

        public Task<ProductSupplierMapDto?> GetSupplierMapByIdAsync(Guid supplierMapId, CancellationToken cancellationToken = default)
            => _repository.GetSupplierMapByIdAsync(supplierMapId, cancellationToken);

        public Task<ProductSupplierMapDto> CreateSupplierMapAsync(CreateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateSupplierMapAsync(request, cancellationToken));

        public Task<bool> UpdateSupplierMapAsync(Guid supplierMapId, UpdateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateSupplierMapAsync(supplierMapId, request, cancellationToken));

        public Task<bool> DeleteSupplierMapAsync(Guid supplierMapId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteSupplierMapAsync(supplierMapId, cancellationToken));

        public Task<IReadOnlyList<WarehouseDto>> GetWarehousesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetWarehousesAsync(includeInactive, cancellationToken);

        public Task<IReadOnlyList<LookupItemDto>> GetWarehouseLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetWarehouseLookupsAsync(includeInactive, cancellationToken);

        public Task<WarehouseDto?> GetWarehouseByIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
            => _repository.GetWarehouseByIdAsync(warehouseId, cancellationToken);

        public Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateWarehouseAsync(request, cancellationToken));

        public Task<bool> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateWarehouseAsync(warehouseId, request, cancellationToken));

        public Task<bool> DeleteWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteWarehouseAsync(warehouseId, cancellationToken));

        public Task<IReadOnlyList<InventoryTransactionDto>> GetInventoryTransactionsAsync(InventoryTransactionFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetInventoryTransactionsAsync(filter, cancellationToken);

        public Task<InventoryTransactionDto?> GetInventoryTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
            => _repository.GetInventoryTransactionByIdAsync(transactionId, cancellationToken);

        public Task<InventoryTransactionDto> CreateInventoryTransactionAsync(CreateInventoryTransactionRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateInventoryTransactionAsync(request, cancellationToken));

        public Task<IReadOnlyList<InventoryReservationDto>> GetInventoryReservationsAsync(InventoryReservationFilterDto filter, CancellationToken cancellationToken = default)
            => _repository.GetInventoryReservationsAsync(filter, cancellationToken);

        public Task<InventoryReservationDto?> GetInventoryReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
            => _repository.GetInventoryReservationByIdAsync(reservationId, cancellationToken);

        public Task<InventoryReservationDto> CreateInventoryReservationAsync(CreateInventoryReservationRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateInventoryReservationAsync(request, cancellationToken));

        public Task<bool> UpdateInventoryReservationStatusAsync(Guid reservationId, UpdateInventoryReservationStatusRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateInventoryReservationStatusAsync(reservationId, request, cancellationToken));

        public Task<bool> DeleteInventoryReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteInventoryReservationAsync(reservationId, cancellationToken));

        public Task<IReadOnlyList<ProductPriceListDto>> GetPriceListsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetPriceListsAsync(includeInactive, cancellationToken);

        public Task<IReadOnlyList<LookupItemDto>> GetPriceListLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetPriceListLookupsAsync(includeInactive, cancellationToken);

        public Task<ProductPriceListDto?> GetPriceListByIdAsync(Guid priceListId, CancellationToken cancellationToken = default)
            => _repository.GetPriceListByIdAsync(priceListId, cancellationToken);

        public Task<ProductPriceListDto> CreatePriceListAsync(CreateProductPriceListRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreatePriceListAsync(request, cancellationToken));

        public Task<bool> UpdatePriceListAsync(Guid priceListId, UpdateProductPriceListRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdatePriceListAsync(priceListId, request, cancellationToken));

        public Task<bool> DeletePriceListAsync(Guid priceListId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeletePriceListAsync(priceListId, cancellationToken));

        public Task<IReadOnlyList<ProductPriceListItemDto>> GetPriceListItemsAsync(Guid priceListId, CancellationToken cancellationToken = default)
            => _repository.GetPriceListItemsAsync(priceListId, cancellationToken);

        public Task<ProductPriceListItemDto?> GetPriceListItemByIdAsync(Guid priceListItemId, CancellationToken cancellationToken = default)
            => _repository.GetPriceListItemByIdAsync(priceListItemId, cancellationToken);

        public Task<ProductPriceListItemDto> CreatePriceListItemAsync(CreateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreatePriceListItemAsync(request, cancellationToken));

        public Task<bool> UpdatePriceListItemAsync(Guid priceListItemId, UpdateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdatePriceListItemAsync(priceListItemId, request, cancellationToken));

        public Task<bool> DeletePriceListItemAsync(Guid priceListItemId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeletePriceListItemAsync(priceListItemId, cancellationToken));

        public Task<IReadOnlyList<UnitDefinitionDto>> GetUnitDefinitionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetUnitDefinitionsAsync(includeInactive, cancellationToken);

        public Task<IReadOnlyList<LookupItemDto>> GetUnitDefinitionLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
            => _repository.GetUnitDefinitionLookupsAsync(includeInactive, cancellationToken);

        public Task<UnitDefinitionDto?> GetUnitDefinitionByIdAsync(Guid unitDefinitionId, CancellationToken cancellationToken = default)
            => _repository.GetUnitDefinitionByIdAsync(unitDefinitionId, cancellationToken);

        public Task<UnitDefinitionDto> CreateUnitDefinitionAsync(CreateUnitDefinitionRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.CreateUnitDefinitionAsync(request, cancellationToken));

        public Task<bool> UpdateUnitDefinitionAsync(Guid unitDefinitionId, UpdateUnitDefinitionRequestDto request, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.UpdateUnitDefinitionAsync(unitDefinitionId, request, cancellationToken));

        public Task<bool> DeleteUnitDefinitionAsync(Guid unitDefinitionId, CancellationToken cancellationToken = default)
            => ExecuteWithSqlMapping(() => _repository.DeleteUnitDefinitionAsync(unitDefinitionId, cancellationToken));

        private static async Task<T> ExecuteWithSqlMapping<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (SqlException ex)
            {
                throw MapSqlException(ex);
            }
        }

        private static BaseException MapSqlException(SqlException ex)
        {
            if (ex.Number is 2601 or 2627)
            {
                var message = ex.Message;
                if (message.Contains("IX_Products_ProductCode"))
                    return new ConflictException("Bu koda sahip ürün zaten mevcut.");
                if (message.Contains("IX_Products_Barcode") || message.Contains("Barcode"))
                    return new ConflictException("Bu barkoda sahip ürün zaten mevcut.");
                if (message.Contains("IX_ProductSuppliers_SupplierCode") || message.Contains("SupplierCode"))
                    return new ConflictException("Bu tedarikçi koduna sahip kayıt zaten mevcut.");
                if (message.Contains("IX_Warehouses_Code") || message.Contains("IX_Product_Warehouses"))
                    return new ConflictException("Bu koda sahip depo zaten mevcut.");
                if (message.Contains("IX_UnitDefinitions_Code"))
                    return new ConflictException("Bu koda sahip birim tanımı zaten mevcut.");
                if (message.Contains("IX_ProductUnits_ProductId_Code"))
                    return new ConflictException("Bu üründe aynı koda sahip birim zaten mevcut.");
                if (message.Contains("IX_ProductPriceLists_Code") || message.Contains("PriceList"))
                    return new ConflictException("Bu koda sahip fiyat listesi zaten mevcut.");
                if (message.Contains("IX_ProductModules_ProductId_ModuleCode"))
                    return new ConflictException("Bu ürüne ait aynı kodda modül zaten mevcut.");
                if (message.Contains("IX_ProductLicenseOfferings_ProductId_Name"))
                    return new ConflictException("Bu ürüne ait aynı isimde lisans teklifi zaten mevcut.");

                return new ConflictException("Bu kayıt zaten mevcut.");
            }

            return ex.Number switch
            {
                547 => new ValidationException("request", "İlişkili veri kuralı ihlal edildi."),
                515 => MapNullColumnException(ex.Message),
                8115 => new ValidationException("request", "Sayısal alan değeri izin verilen aralığın dışında."),
                245 => new ValidationException("request", "Alan tipi geçersiz."),
                _ => new BaseException("Veritabanı işlemi sırasında hata oluştu.", 500, ex)
            };
        }

        private static ValidationException MapNullColumnException(string message)
        {
            // SQL 515 mesaj formatı: Cannot insert the value NULL into column 'ColumnName', table '...'; column does not allow nulls.
            var match = System.Text.RegularExpressions.Regex.Match(message, @"column '([^']+)'");
            var field = match.Success ? match.Groups[1].Value : "unknown";
            return new ValidationException(field, $"'{field}' alanı boş bırakılamaz.");
        }
    }
}
