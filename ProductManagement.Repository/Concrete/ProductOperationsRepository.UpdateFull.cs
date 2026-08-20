using Dapper;
using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository
    {
        public async Task<bool> UpdateProductFullAsync(Guid productId, UpdateProductFullRequestDto request, CancellationToken cancellationToken = default)
        {
            // Product null kontrolü
            if (request.Product is null)
            {
                throw new InvalidOperationException("Product payload is required.");
            }

            // Önce ürünün var olup olmadığını kontrol et
            var existing = await GetProductByIdAsync(productId, cancellationToken);
            if (existing is null)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Temel ürün bilgilerini güncelle
                const string updateProductSql = @"
UPDATE [Product].[Products]
SET
 ProductCode = @ProductCode,
 Name = @Name,
 ShortDescription = @ShortDescription,
 Description = @Description,
 Kind = @Kind,
 Status = @Status,
 Brand = @Brand,
 Manufacturer = @Manufacturer,
 Barcode = @Barcode,
 IsActive = @IsActive,
 IsSellable = @IsSellable,
 IsPurchasable = @IsPurchasable,
 TrackInventory = @TrackInventory,
 DefaultCurrencyCode = @DefaultCurrencyCode,
 Tags = @Tags,
 MetadataJson = @MetadataJson,
 UpdatedAt = @Now
WHERE Id = @ProductId
 AND IsDeleted = 0;";

                await connection.ExecuteAsync(
                new CommandDefinition(
                updateProductSql,
 new
 {
     ProductId = productId,
     request.Product.ProductCode,
     request.Product.Name,
     request.Product.ShortDescription,
     request.Product.Description,
     request.Product.Kind,
     request.Product.Status,
     request.Product.Brand,
     request.Product.Manufacturer,
     request.Product.Barcode,
     request.Product.IsActive,
     request.Product.IsSellable,
     request.Product.IsPurchasable,
     request.Product.TrackInventory,
     request.Product.DefaultCurrencyCode,
     request.Product.Tags,
     request.Product.MetadataJson,
     Now = now
 },
                transaction,
                cancellationToken: cancellationToken));

                // 2. İlişkili koleksiyonlar: mevcut kayıtları soft-delete yap, yenilerini ekle
                if (request.AttributeValues is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductAttributeValues]", productId, cancellationToken);
                    await InsertAttributeValuesAsync(connection, transaction, productId, now, request.AttributeValues, cancellationToken);
                }

                if (request.Variants is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductVariants]", productId, cancellationToken);
                    await InsertVariantsAsync(connection, transaction, productId, now, request.Variants, cancellationToken);
                }

                if (request.Prices is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductPrices]", productId, cancellationToken);
                    await InsertPricesAsync(connection, transaction, productId, now, request.Prices, cancellationToken);
                }

                if (request.Inventories is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductInventories]", productId, cancellationToken);
                    await InsertInventoriesAsync(connection, transaction, productId, now, request.Inventories, cancellationToken);
                }

                if (request.MediaItems is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductMediaItems]", productId, cancellationToken);
                    await InsertMediaAsync(connection, transaction, productId, now, request.MediaItems, cancellationToken);
                }

                if (request.CategoryMaps is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductCategoryMaps]", productId, cancellationToken);
                    await InsertCategoryMapsAsync(connection, transaction, productId, now, request.CategoryMaps, cancellationToken);
                }

                if (request.BundleItems is not null)
                {
                    // ChildProductId boş olan bundle item'ları geçerli değil, filtrele
                    var validBundleItems = request.BundleItems
                    .Where(b => b.ChildProductId != Guid.Empty)
                    .ToList();

                    await HardDeleteByBundleProductIdAsync(connection, transaction, productId, cancellationToken);
                    await InsertBundleItemsAsync(connection, transaction, productId, now, validBundleItems, cancellationToken);
                }

                if (request.SupplierMaps is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductSupplierMaps]", productId, cancellationToken);
                    await InsertSupplierMapsAsync(connection, transaction, productId, now, request.SupplierMaps, cancellationToken);
                }

                IReadOnlyDictionary<string, Guid>? productUnitTempIdMap = null;
                if (request.ProductUnits is not null)
                {
                    if (request.LicenseOfferings is not null)
                    {
                        await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductPricingRules]", productId, cancellationToken);
                        await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductLicenseOfferings]", productId, cancellationToken);
                    }
                    else if (request.PricingRules is not null)
                    {
                        await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductPricingRules]", productId, cancellationToken);
                    }

                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductUnits]", productId, cancellationToken);
                    productUnitTempIdMap = await InsertProductUnitsAsync(connection, transaction, productId, now, request.ProductUnits, cancellationToken);
                }

                if (request.PricingRules is not null && request.LicenseOfferings is null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductPricingRules]", productId, cancellationToken);
                    await InsertPricingRulesAsync(connection, transaction, productId, now, request.PricingRules, null, productUnitTempIdMap, cancellationToken);
                }

                IReadOnlyDictionary<string, Guid>? moduleCodeMap = null;
                if (request.Modules is not null)
                {
                    await HardDeleteModuleOfferingPricesByProductIdAsync(connection, transaction, productId, cancellationToken);
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductModules]", productId, cancellationToken);
                    moduleCodeMap = await InsertModulesAsync(connection, transaction, productId, now, request.Modules, cancellationToken);
                }

                IReadOnlyDictionary<string, Guid>? licenseOfferingTempIdMap = null;
                if (request.LicenseOfferings is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductPricingRules]", productId, cancellationToken);
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductLicenseOfferings]", productId, cancellationToken);
                    licenseOfferingTempIdMap = await InsertLicenseOfferingsAsync(connection, transaction, productId, now, request.LicenseOfferings, productUnitTempIdMap, cancellationToken);
                }

                if (request.PricingRules is not null && request.LicenseOfferings is not null)
                {
                    await InsertPricingRulesAsync(connection, transaction, productId, now, request.PricingRules, licenseOfferingTempIdMap, productUnitTempIdMap, cancellationToken);
                }

                if (moduleCodeMap is not null && licenseOfferingTempIdMap is not null)
                {
                    await InsertModuleOfferingPricesAsync(connection, transaction, now, request.Modules, moduleCodeMap, licenseOfferingTempIdMap, cancellationToken);
                }

                // Yeni işlemler/rezervasyonlar sadece eklenir, silinmez (audit trail korunur)
                if (request.InventoryTransactions is not null && request.InventoryTransactions.Count > 0)
                {
                    await InsertInventoryTransactionsAsync(connection, transaction, productId, now, request.InventoryTransactions, cancellationToken);
                }

                if (request.InventoryReservations is not null && request.InventoryReservations.Count > 0)
                {
                    await InsertInventoryReservationsAsync(connection, transaction, productId, now, request.InventoryReservations, cancellationToken);
                }

                if (request.PriceListItems is not null)
                {
                    await HardDeleteByProductIdAsync(connection, transaction, "[Product].[ProductPriceListItems]", productId, cancellationToken);
                    await InsertPriceListItemsAsync(connection, transaction, productId, now, request.PriceListItems, cancellationToken);
                }

                // Profiller: upsert (varsa güncelle, yoksa ekle)
                await UpsertPhysicalProfileAsync(connection, transaction, productId, now, request.PhysicalProfile, cancellationToken);
                await UpsertSoftwareProfileAsync(connection, transaction, productId, now, request.SoftwareProfile, cancellationToken);
                await UpsertServiceProfileAsync(connection, transaction, productId, now, request.ServiceProfile, cancellationToken);
                await UpsertSubscriptionProfileAsync(connection, transaction, productId, now, request.SubscriptionProfile, cancellationToken);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return true;
        }

        // Unique index'e takılmamak için fiziksel silme kullanıyoruz
        private static async Task HardDeleteByProductIdAsync(
        System.Data.IDbConnection connection,
        System.Data.IDbTransaction transaction,
        string tableName,
        Guid productId,
        CancellationToken cancellationToken)
        {
            var sql = $"DELETE FROM {tableName} WHERE ProductId = @ProductId;";
            await connection.ExecuteAsync(
            new CommandDefinition(sql, new { ProductId = productId }, transaction, cancellationToken: cancellationToken));
        }

        private static async Task HardDeleteByBundleProductIdAsync(
        System.Data.IDbConnection connection,
        System.Data.IDbTransaction transaction,
        Guid productId,
        CancellationToken cancellationToken)
        {
            const string sql = "DELETE FROM [Product].[ProductBundleItems] WHERE BundleProductId = @ProductId;";
            await connection.ExecuteAsync(
            new CommandDefinition(sql, new { ProductId = productId }, transaction, cancellationToken: cancellationToken));
        }

        private static async Task HardDeleteModuleOfferingPricesByProductIdAsync(
        System.Data.IDbConnection connection,
        System.Data.IDbTransaction transaction,
        Guid productId,
        CancellationToken cancellationToken)
        {
            const string sql = @"
DELETE p FROM [Product].[ProductModuleOfferingPrices] p
JOIN [Product].[ProductModules] m ON m.Id = p.ProductModuleId
WHERE m.ProductId = @ProductId;";
            await connection.ExecuteAsync(
            new CommandDefinition(sql, new { ProductId = productId }, transaction, cancellationToken: cancellationToken));
        }
    }
}
