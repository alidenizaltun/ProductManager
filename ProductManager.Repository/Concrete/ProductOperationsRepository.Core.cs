using Dapper;
using ProductManager.Shared.Dtos.ProductOperations;
using System.Data;
using System.Text;

namespace ProductManager.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository
    {
        public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
        {
            var take = NormalizeTake(filter.Take);
            var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
            var includeLargeFields = filter.IncludeLargeFields;

            var sqlBuilder = new StringBuilder(@"
SELECT TOP (@Take)
    Id,
    ProductCode,
    Name,
    ShortDescription,
    Kind,
    Status,
    Brand,
    Manufacturer,
    Barcode,
    IsActive,
    IsSellable,
    IsPurchasable,
    TrackInventory,
    DefaultCurrencyCode,
    UnitOfMeasure,
    TaxRate,
    TaxCode,
    Tags,
    CreatedAt,
    UpdatedAt");

            if (includeLargeFields)
            {
                sqlBuilder.Append(",\n    Description,\n    MetadataJson");
            }

            sqlBuilder.Append(@"
FROM [Product].[Products]
WHERE IsDeleted = 0");

            var parameters = new DynamicParameters();
            parameters.Add("Take", take);

            if (search is not null)
            {
                sqlBuilder.Append("\n  AND (ProductCode LIKE @SearchPattern OR Name LIKE @SearchPattern)");
                parameters.Add("SearchPattern", $"%{search}%");
            }

            if (filter.Kind.HasValue)
            {
                sqlBuilder.Append("\n  AND Kind = @Kind");
                parameters.Add("Kind", filter.Kind);
            }

            if (filter.Status.HasValue)
            {
                sqlBuilder.Append("\n  AND Status = @Status");
                parameters.Add("Status", filter.Status);
            }

            if (filter.IsActive.HasValue)
            {
                sqlBuilder.Append("\n  AND IsActive = @IsActive");
                parameters.Add("IsActive", filter.IsActive);
            }

            sqlBuilder.Append("\nORDER BY CreatedAt DESC;");

            using var connection = CreateConnection();
            var products = await connection.QueryAsync<ProductDto>(
                new CommandDefinition(
                    sqlBuilder.ToString(),
                    parameters,
                    cancellationToken: cancellationToken));

            return products.AsList();
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductCode,
    Name,
    ShortDescription,
    Description,
    Kind,
    Status,
    Brand,
    Manufacturer,
    Barcode,
    IsActive,
    IsSellable,
    IsPurchasable,
    TrackInventory,
    DefaultCurrencyCode,
    UnitOfMeasure,
    TaxRate,
    TaxCode,
    Tags,
    MetadataJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[Products]
WHERE Id = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductDetailDto?> GetProductDetailByIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
-- 1: Product
SELECT
    Id, ProductCode, Name, ShortDescription, Description, Kind, Status,
    Brand, Manufacturer, Barcode, IsActive, IsSellable, IsPurchasable,
    TrackInventory, DefaultCurrencyCode, UnitOfMeasure, TaxRate, TaxCode,
    Tags, MetadataJson, CreatedAt, UpdatedAt
FROM [Product].[Products]
WHERE Id = @ProductId AND IsDeleted = 0;

-- 2: AttributeValues
SELECT
    av.Id, av.ProductId, av.AttributeDefinitionId,
    av.ValueText, av.ValueNumber, av.ValueBool, av.ValueDate, av.ValueJson,
    av.CreatedAt, av.UpdatedAt
FROM [Product].[ProductAttributeValues] av
WHERE av.ProductId = @ProductId AND av.IsDeleted = 0;

-- 3: Variants
SELECT Id, ProductId, Sku, Barcode, Name, OptionValuesJson,
       AdditionalPrice, AdditionalCost, IsActive, CreatedAt, UpdatedAt
FROM [Product].[ProductVariants]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 4: Prices
SELECT Id, ProductId, ProductVariantId, PriceType, Amount, CompareAtAmount,
       CurrencyCode, MinQuantity, MaxQuantity, ValidFrom, ValidTo,
       SalesChannel, CustomerGroupCode, CreatedAt, UpdatedAt
FROM [Product].[ProductPrices]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 5: Inventories
SELECT Id, ProductId, ProductVariantId, WarehouseId, WarehouseCode,
       QuantityOnHand, QuantityReserved,
       QuantityOnHand - QuantityReserved AS QuantityAvailable,
       ReorderPoint, ReorderQuantity, InventoryPolicy, CreatedAt, UpdatedAt
FROM [Product].[ProductInventories]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 6: MediaItems
SELECT Id, ProductId, MediaType, Url, ThumbnailUrl, MimeType, AltText,
       IsPrimary, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductMediaItems]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY SortOrder;

-- 7: CategoryMaps
SELECT Id, ProductId, ProductCategoryId, IsPrimary, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductCategoryMaps]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 8: BundleItems
SELECT Id, BundleProductId, ChildProductId, ChildVariantId, Quantity,
       IsOptional, RuleJson, CreatedAt, UpdatedAt
FROM [Product].[ProductBundleItems]
WHERE BundleProductId = @ProductId AND IsDeleted = 0;

-- 9: SupplierMaps
SELECT Id, ProductId, ProductSupplierId, SupplierProductCode, SupplierCost,
       LeadTimeInDays, MinOrderQuantity, IsPreferred, CreatedAt, UpdatedAt
FROM [Product].[ProductSupplierMaps]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 10: PhysicalProfile
SELECT Id, ProductId, Weight, Width, Height, Length,
       RequiresShipping, IsFragile, IsHazardous, RequiresSerialNumber,
       WarrantyInMonths, CreatedAt, UpdatedAt
FROM [Product].[ProductPhysicalProfiles]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 11: SoftwareProfile
SELECT Id, ProductId, Version, LicenseModel, SeatCount, DownloadUrl,
       SupportedPlatformsJson, SystemRequirementsJson, ReleaseNotes, CreatedAt, UpdatedAt
FROM [Product].[ProductSoftwareProfiles]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 12: ServiceProfile
SELECT Id, ProductId, DeliveryMode, DurationInMinutes, MaxConcurrentBooking,
       ServiceAreaJson, ServiceLevelAgreementJson, CapacityRuleJson, CreatedAt, UpdatedAt
FROM [Product].[ProductServiceProfiles]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 13: SubscriptionProfile
SELECT Id, ProductId, BillingPeriodUnit, BillingPeriodValue, TrialDays,
 AutoRenew, GracePeriodDays, CancellationPolicy, SubscriptionRulesJson, CreatedAt, UpdatedAt
FROM [Product].[ProductSubscriptionProfiles]
WHERE ProductId = @ProductId AND IsDeleted = 0;

-- 14: Modules
SELECT Id, ProductId, ModuleCode, Name, Description, AdditionalPrice,
 CurrencyCode, IsOptional, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductModules]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY SortOrder, Name;

-- 15: SoftwarePricingTiers
SELECT Id, ProductId, LicenseModel, Unit, MinUnits, MaxUnits,
 PricePerUnit, FlatFee, CurrencyCode, IsActive, CreatedAt, UpdatedAt
FROM [Product].[SoftwarePricingTiers]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY LicenseModel, MinUnits;

-- 16: LicenseOfferings
SELECT Id, ProductId, LicenseModel, Name, Description, BasePrice, CurrencyCode,
 BillingPeriodUnit, BillingPeriodValue, AutoRenew, GracePeriodDays,
 TrialDays, ConvertToOfferingId, MaxSeats, ValidFrom, ValidTo,
 IsActive, SortOrder, CreatedAt, UpdatedAt
FROM [Product].[ProductLicenseOfferings]
WHERE ProductId = @ProductId AND IsDeleted = 0
ORDER BY SortOrder, LicenseModel;
";

            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));

            var product = await multi.ReadSingleOrDefaultAsync<ProductDto>();
            if (product is null)
            {
                return null;
            }

            var attributeValues = (await multi.ReadAsync<ProductAttributeValueDto>()).AsList();
            var variants = (await multi.ReadAsync<ProductVariantDto>()).AsList();
            var prices = (await multi.ReadAsync<ProductPriceDto>()).AsList();
            var inventories = (await multi.ReadAsync<ProductInventoryDto>()).AsList();
            var mediaItems = (await multi.ReadAsync<ProductMediaDto>()).AsList();
            var categoryMaps = (await multi.ReadAsync<ProductCategoryMapDto>()).AsList();
            var bundleItems = (await multi.ReadAsync<ProductBundleItemDto>()).AsList();
            var supplierMaps = (await multi.ReadAsync<ProductSupplierMapDto>()).AsList();
            var physicalProfile = await multi.ReadSingleOrDefaultAsync<ProductPhysicalProfileDto>();
            var softwareProfile = await multi.ReadSingleOrDefaultAsync<ProductSoftwareProfileDto>();
            var serviceProfile = await multi.ReadSingleOrDefaultAsync<ProductServiceProfileDto>();
            var subscriptionProfile = await multi.ReadSingleOrDefaultAsync<ProductSubscriptionProfileDto>();
            var modules = (await multi.ReadAsync<ProductModuleDto>()).AsList();
            var pricingTiers = (await multi.ReadAsync<SoftwarePricingTierDto>()).AsList();
            var licenseOfferings = (await multi.ReadAsync<ProductLicenseOfferingDto>()).AsList();

            return new ProductDetailDto
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                Name = product.Name,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                Kind = product.Kind,
                Status = product.Status,
                Brand = product.Brand,
                Manufacturer = product.Manufacturer,
                Barcode = product.Barcode,
                IsActive = product.IsActive,
                IsSellable = product.IsSellable,
                IsPurchasable = product.IsPurchasable,
                TrackInventory = product.TrackInventory,
                DefaultCurrencyCode = product.DefaultCurrencyCode,
                UnitOfMeasure = product.UnitOfMeasure,
                TaxRate = product.TaxRate,
                TaxCode = product.TaxCode,
                Tags = product.Tags,
                MetadataJson = product.MetadataJson,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                AttributeValues = attributeValues,
                Variants = variants,
                Prices = prices,
                Inventories = inventories,
                MediaItems = mediaItems,
                CategoryMaps = categoryMaps,
                BundleItems = bundleItems,
                SupplierMaps = supplierMaps,
                PhysicalProfile = physicalProfile,
                SoftwareProfile = softwareProfile,
                ServiceProfile = serviceProfile,
                SubscriptionProfile = subscriptionProfile,
                Modules = modules,
                SoftwarePricingTiers = pricingTiers,
                LicenseOfferings = licenseOfferings,
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[Products]
(
    Id,
    ProductCode,
    Name,
    ShortDescription,
    Description,
    Kind,
    Status,
    Brand,
    Manufacturer,
    Barcode,
    IsActive,
    IsSellable,
    IsPurchasable,
    TrackInventory,
    DefaultCurrencyCode,
    UnitOfMeasure,
    TaxRate,
    TaxCode,
    Tags,
    MetadataJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductCode,
    @Name,
    @ShortDescription,
    @Description,
    @Kind,
    @Status,
    @Brand,
    @Manufacturer,
    @Barcode,
    @IsActive,
    @IsSellable,
    @IsPurchasable,
    @TrackInventory,
    @DefaultCurrencyCode,
    @UnitOfMeasure,
    @TaxRate,
    @TaxCode,
    @Tags,
    @MetadataJson,
    @Now,
    0
);";

            var productId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = productId,
                        request.ProductCode,
                        request.Name,
                        request.ShortDescription,
                        request.Description,
                        request.Kind,
                        request.Status,
                        request.Brand,
                        request.Manufacturer,
                        request.Barcode,
                        request.IsActive,
                        request.IsSellable,
                        request.IsPurchasable,
                        request.TrackInventory,
                        request.DefaultCurrencyCode,
                        request.UnitOfMeasure,
                        request.TaxRate,
                        request.TaxCode,
                        request.Tags,
                        request.MetadataJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetProductByIdAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Product could not be loaded after insert.");
        }

        public async Task<ProductDto> CreateProductFullAsync(CreateProductFullRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.Product is null)
            {
                throw new InvalidOperationException("Product payload is required.");
            }

            var productId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string productSql = @"
INSERT INTO [Product].[Products]
(
    Id,
    ProductCode,
    Name,
    ShortDescription,
    Description,
    Kind,
    Status,
    Brand,
    Manufacturer,
    Barcode,
    IsActive,
    IsSellable,
    IsPurchasable,
    TrackInventory,
    DefaultCurrencyCode,
    UnitOfMeasure,
    TaxRate,
    TaxCode,
    Tags,
    MetadataJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductCode,
    @Name,
    @ShortDescription,
    @Description,
    @Kind,
    @Status,
    @Brand,
    @Manufacturer,
    @Barcode,
    @IsActive,
    @IsSellable,
    @IsPurchasable,
    @TrackInventory,
    @DefaultCurrencyCode,
    @UnitOfMeasure,
    @TaxRate,
    @TaxCode,
    @Tags,
    @MetadataJson,
    @Now,
    0
);";

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        productSql,
                        new
                        {
                            Id = productId,
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
                            request.Product.UnitOfMeasure,
                            request.Product.TaxRate,
                            request.Product.TaxCode,
                            request.Product.Tags,
                            request.Product.MetadataJson,
                            Now = now
                        },
                        transaction,
                        cancellationToken: cancellationToken));

                await InsertAttributeValuesAsync(connection, transaction, productId, now, request.AttributeValues, cancellationToken);
                await InsertVariantsAsync(connection, transaction, productId, now, request.Variants, cancellationToken);
                await InsertPricesAsync(connection, transaction, productId, now, request.Prices, cancellationToken);
                await InsertInventoriesAsync(connection, transaction, productId, now, request.Inventories, cancellationToken);
                await InsertMediaAsync(connection, transaction, productId, now, request.MediaItems, cancellationToken);
                await InsertCategoryMapsAsync(connection, transaction, productId, now, request.CategoryMaps, cancellationToken);
                await InsertBundleItemsAsync(connection, transaction, productId, now, request.BundleItems, cancellationToken);
                await InsertSupplierMapsAsync(connection, transaction, productId, now, request.SupplierMaps, cancellationToken);
                await InsertInventoryTransactionsAsync(connection, transaction, productId, now, request.InventoryTransactions, cancellationToken);
                await InsertInventoryReservationsAsync(connection, transaction, productId, now, request.InventoryReservations, cancellationToken);
                await InsertPriceListItemsAsync(connection, transaction, productId, now, request.PriceListItems, cancellationToken);
                await InsertModulesAsync(connection, transaction, productId, now, request.Modules, cancellationToken);
                await InsertSoftwarePricingTiersAsync(connection, transaction, productId, now, request.SoftwarePricingTiers, cancellationToken);
                await InsertLicenseOfferingsAsync(connection, transaction, productId, now, request.LicenseOfferings, cancellationToken);
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

            return await GetProductByIdAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Product could not be loaded after insert.");
        }

        private static async Task InsertAttributeValuesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductAttributeValueRequestDto>? values,
            CancellationToken cancellationToken)
        {
            if (values is null || values.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductAttributeValues]
(
    Id,
    ProductId,
    AttributeDefinitionId,
    ValueText,
    ValueNumber,
    ValueBool,
    ValueDate,
    ValueJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @AttributeDefinitionId,
    @ValueText,
    @ValueNumber,
    @ValueBool,
    @ValueDate,
    @ValueJson,
    @Now,
    0
);";

            var parameters = values.Select(value => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                value.AttributeDefinitionId,
                value.ValueText,
                value.ValueNumber,
                value.ValueBool,
                value.ValueDate,
                value.ValueJson,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertVariantsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductVariantRequestDto>? variants,
            CancellationToken cancellationToken)
        {
            if (variants is null || variants.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductVariants]
(
    Id,
    ProductId,
    Sku,
    Barcode,
    Name,
    OptionValuesJson,
    AdditionalPrice,
    AdditionalCost,
    IsActive,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @Sku,
    @Barcode,
    @Name,
    @OptionValuesJson,
    @AdditionalPrice,
    @AdditionalCost,
    @IsActive,
    @Now,
    0
);";

            var parameters = variants.Select(variant => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                variant.Sku,
                variant.Barcode,
                variant.Name,
                variant.OptionValuesJson,
                variant.AdditionalPrice,
                variant.AdditionalCost,
                variant.IsActive,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertPricesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductPriceRequestDto>? prices,
            CancellationToken cancellationToken)
        {
            if (prices is null || prices.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductPrices]
(
    Id,
    ProductId,
    ProductVariantId,
    PriceType,
    Amount,
    CompareAtAmount,
    CurrencyCode,
    MinQuantity,
    MaxQuantity,
    ValidFrom,
    ValidTo,
    SalesChannel,
    CustomerGroupCode,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductVariantId,
    @PriceType,
    @Amount,
    @CompareAtAmount,
    @CurrencyCode,
    @MinQuantity,
    @MaxQuantity,
    @ValidFrom,
    @ValidTo,
    @SalesChannel,
    @CustomerGroupCode,
    @Now,
    0
);";

            var parameters = prices.Select(price => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                price.ProductVariantId,
                price.PriceType,
                price.Amount,
                price.CompareAtAmount,
                price.CurrencyCode,
                price.MinQuantity,
                price.MaxQuantity,
                price.ValidFrom,
                price.ValidTo,
                price.SalesChannel,
                price.CustomerGroupCode,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertInventoriesAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductInventoryRequestDto>? inventories,
            CancellationToken cancellationToken)
        {
            if (inventories is null || inventories.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductInventories]
(
    Id,
    ProductId,
    ProductVariantId,
    WarehouseId,
    WarehouseCode,
    QuantityOnHand,
    QuantityReserved,
    ReorderPoint,
    ReorderQuantity,
    InventoryPolicy,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductVariantId,
    @WarehouseId,
    @WarehouseCode,
    @QuantityOnHand,
    @QuantityReserved,
    @ReorderPoint,
    @ReorderQuantity,
    @InventoryPolicy,
    @Now,
    0
);";

            var parameters = inventories.Select(inventory => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                inventory.ProductVariantId,
                inventory.WarehouseId,
                inventory.WarehouseCode,
                inventory.QuantityOnHand,
                inventory.QuantityReserved,
                inventory.ReorderPoint,
                inventory.ReorderQuantity,
                inventory.InventoryPolicy,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertMediaAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductMediaRequestDto>? mediaItems,
            CancellationToken cancellationToken)
        {
            if (mediaItems is null || mediaItems.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductMediaItems]
(
    Id,
    ProductId,
    MediaType,
    Url,
    ThumbnailUrl,
    MimeType,
    AltText,
    IsPrimary,
    SortOrder,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @MediaType,
    @Url,
    @ThumbnailUrl,
    @MimeType,
    @AltText,
    @IsPrimary,
    @SortOrder,
    @Now,
    0
);";

            var parameters = mediaItems.Select(media => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                media.MediaType,
                media.Url,
                media.ThumbnailUrl,
                media.MimeType,
                media.AltText,
                media.IsPrimary,
                media.SortOrder,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertCategoryMapsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductCategoryMapRequestDto>? categoryMaps,
            CancellationToken cancellationToken)
        {
            if (categoryMaps is null || categoryMaps.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductCategoryMaps]
(
    Id,
    ProductId,
    ProductCategoryId,
    IsPrimary,
    SortOrder,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductCategoryId,
    @IsPrimary,
    @SortOrder,
    @Now,
    0
);";

            var parameters = categoryMaps.Select(map => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                map.ProductCategoryId,
                map.IsPrimary,
                map.SortOrder,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertBundleItemsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductBundleItemRequestDto>? bundleItems,
            CancellationToken cancellationToken)
        {
            if (bundleItems is null || bundleItems.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductBundleItems]
(
    Id,
    BundleProductId,
    ChildProductId,
    ChildVariantId,
    Quantity,
    IsOptional,
    RuleJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @BundleProductId,
    @ChildProductId,
    @ChildVariantId,
    @Quantity,
    @IsOptional,
    @RuleJson,
    @Now,
    0
);";

            var parameters = bundleItems
            .Where(item => item.ChildProductId != Guid.Empty)
            .Select(item => new
            {
                Id = Guid.NewGuid(),
                BundleProductId = item.BundleProductId == Guid.Empty ? productId : item.BundleProductId,
                item.ChildProductId,
                item.ChildVariantId,
                item.Quantity,
                item.IsOptional,
                item.RuleJson,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertSupplierMapsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductSupplierMapRequestDto>? supplierMaps,
            CancellationToken cancellationToken)
        {
            if (supplierMaps is null || supplierMaps.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductSupplierMaps]
(
    Id,
    ProductId,
    ProductSupplierId,
    SupplierProductCode,
    SupplierCost,
    LeadTimeInDays,
    MinOrderQuantity,
    IsPreferred,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductSupplierId,
    @SupplierProductCode,
    @SupplierCost,
    @LeadTimeInDays,
    @MinOrderQuantity,
    @IsPreferred,
    @Now,
    0
);";

            var parameters = supplierMaps.Select(map => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                map.ProductSupplierId,
                map.SupplierProductCode,
                map.SupplierCost,
                map.LeadTimeInDays,
                map.MinOrderQuantity,
                map.IsPreferred,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertInventoryTransactionsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateInventoryTransactionRequestDto>? transactions,
            CancellationToken cancellationToken)
        {
            if (transactions is null || transactions.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[InventoryTransactions]
(
    Id,
    ProductId,
    ProductVariantId,
    WarehouseId,
    TransactionType,
    Quantity,
    UnitCost,
    ReferenceType,
    ReferenceNumber,
    Note,
    OccurredAt,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductVariantId,
    @WarehouseId,
    @TransactionType,
    @Quantity,
    @UnitCost,
    @ReferenceType,
    @ReferenceNumber,
    @Note,
    @OccurredAt,
    @Now,
    0
);";

            var parameters = transactions.Select(transactionItem => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                transactionItem.ProductVariantId,
                transactionItem.WarehouseId,
                transactionItem.TransactionType,
                transactionItem.Quantity,
                transactionItem.UnitCost,
                transactionItem.ReferenceType,
                transactionItem.ReferenceNumber,
                transactionItem.Note,
                OccurredAt = transactionItem.OccurredAt ?? now,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertInventoryReservationsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateInventoryReservationRequestDto>? reservations,
            CancellationToken cancellationToken)
        {
            if (reservations is null || reservations.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[InventoryReservations]
(
    Id,
    ProductId,
    ProductVariantId,
    WarehouseId,
    Quantity,
    ReservationCode,
    ReservedUntil,
    Status,
    SourceType,
    SourceId,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductVariantId,
    @WarehouseId,
    @Quantity,
    @ReservationCode,
    @ReservedUntil,
    @Status,
    @SourceType,
    @SourceId,
    @Now,
    0
);";

            var parameters = reservations.Select(reservation => new
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                reservation.ProductVariantId,
                reservation.WarehouseId,
                reservation.Quantity,
                reservation.ReservationCode,
                reservation.ReservedUntil,
                reservation.Status,
                reservation.SourceType,
                reservation.SourceId,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task InsertPriceListItemsAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            IReadOnlyList<CreateProductPriceListItemRequestDto>? items,
            CancellationToken cancellationToken)
        {
            if (items is null || items.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO [Product].[ProductPriceListItems]
(
    Id,
    ProductPriceListId,
    ProductId,
    ProductVariantId,
    Amount,
    CompareAtAmount,
    MinQuantity,
    MaxQuantity,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductPriceListId,
    @ProductId,
    @ProductVariantId,
    @Amount,
    @CompareAtAmount,
    @MinQuantity,
    @MaxQuantity,
    @Now,
    0
);";

            var parameters = items.Select(item => new
            {
                Id = Guid.NewGuid(),
                item.ProductPriceListId,
                ProductId = productId,
                item.ProductVariantId,
                item.Amount,
                item.CompareAtAmount,
                item.MinQuantity,
                item.MaxQuantity,
                Now = now
            });

            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: cancellationToken));
        }

        private static async Task UpsertPhysicalProfileAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            UpsertProductPhysicalProfileRequestDto? profile,
            CancellationToken cancellationToken)
        {
            if (profile is null)
            {
                return;
            }

            const string updateSql = @"
UPDATE [Product].[ProductPhysicalProfiles]
SET
    Weight = @Weight,
    Width = @Width,
    Height = @Height,
    Length = @Length,
    RequiresShipping = @RequiresShipping,
    IsFragile = @IsFragile,
    IsHazardous = @IsHazardous,
    RequiresSerialNumber = @RequiresSerialNumber,
    WarrantyInMonths = @WarrantyInMonths,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        profile.Weight,
                        profile.Width,
                        profile.Height,
                        profile.Length,
                        profile.RequiresShipping,
                        profile.IsFragile,
                        profile.IsHazardous,
                        profile.RequiresSerialNumber,
                        profile.WarrantyInMonths,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            if (affectedRows > 0)
            {
                return;
            }

            const string insertSql = @"
INSERT INTO [Product].[ProductPhysicalProfiles]
(
    Id,
    ProductId,
    Weight,
    Width,
    Height,
    Length,
    RequiresShipping,
    IsFragile,
    IsHazardous,
    RequiresSerialNumber,
    WarrantyInMonths,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @Weight,
    @Width,
    @Height,
    @Length,
    @RequiresShipping,
    @IsFragile,
    @IsHazardous,
    @RequiresSerialNumber,
    @WarrantyInMonths,
    @Now,
    0
);";

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertSql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        profile.Weight,
                        profile.Width,
                        profile.Height,
                        profile.Length,
                        profile.RequiresShipping,
                        profile.IsFragile,
                        profile.IsHazardous,
                        profile.RequiresSerialNumber,
                        profile.WarrantyInMonths,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));
        }

        private static async Task UpsertSoftwareProfileAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            UpsertProductSoftwareProfileRequestDto? profile,
            CancellationToken cancellationToken)
        {
            if (profile is null)
            {
                return;
            }

            const string updateSql = @"
UPDATE [Product].[ProductSoftwareProfiles]
SET
    Version = @Version,
    LicenseModel = @LicenseModel,
    SeatCount = @SeatCount,
    DownloadUrl = @DownloadUrl,
    SupportedPlatformsJson = @SupportedPlatformsJson,
    SystemRequirementsJson = @SystemRequirementsJson,
    ReleaseNotes = @ReleaseNotes,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        profile.Version,
                        profile.LicenseModel,
                        profile.SeatCount,
                        profile.DownloadUrl,
                        profile.SupportedPlatformsJson,
                        profile.SystemRequirementsJson,
                        profile.ReleaseNotes,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            if (affectedRows > 0)
            {
                return;
            }

            const string insertSql = @"
INSERT INTO [Product].[ProductSoftwareProfiles]
(
    Id,
    ProductId,
    Version,
    LicenseModel,
    SeatCount,
    DownloadUrl,
    SupportedPlatformsJson,
    SystemRequirementsJson,
    ReleaseNotes,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @Version,
    @LicenseModel,
    @SeatCount,
    @DownloadUrl,
    @SupportedPlatformsJson,
    @SystemRequirementsJson,
    @ReleaseNotes,
    @Now,
    0
);";

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertSql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        profile.Version,
                        profile.LicenseModel,
                        profile.SeatCount,
                        profile.DownloadUrl,
                        profile.SupportedPlatformsJson,
                        profile.SystemRequirementsJson,
                        profile.ReleaseNotes,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));
        }

        private static async Task UpsertServiceProfileAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            UpsertProductServiceProfileRequestDto? profile,
            CancellationToken cancellationToken)
        {
            if (profile is null)
            {
                return;
            }

            const string updateSql = @"
UPDATE [Product].[ProductServiceProfiles]
SET
    DeliveryMode = @DeliveryMode,
    DurationInMinutes = @DurationInMinutes,
    MaxConcurrentBooking = @MaxConcurrentBooking,
    ServiceAreaJson = @ServiceAreaJson,
    ServiceLevelAgreementJson = @ServiceLevelAgreementJson,
    CapacityRuleJson = @CapacityRuleJson,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        profile.DeliveryMode,
                        profile.DurationInMinutes,
                        profile.MaxConcurrentBooking,
                        profile.ServiceAreaJson,
                        profile.ServiceLevelAgreementJson,
                        profile.CapacityRuleJson,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            if (affectedRows > 0)
            {
                return;
            }

            const string insertSql = @"
INSERT INTO [Product].[ProductServiceProfiles]
(
    Id,
    ProductId,
    DeliveryMode,
    DurationInMinutes,
    MaxConcurrentBooking,
    ServiceAreaJson,
    ServiceLevelAgreementJson,
    CapacityRuleJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @DeliveryMode,
    @DurationInMinutes,
    @MaxConcurrentBooking,
    @ServiceAreaJson,
    @ServiceLevelAgreementJson,
    @CapacityRuleJson,
    @Now,
    0
);";

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertSql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        profile.DeliveryMode,
                        profile.DurationInMinutes,
                        profile.MaxConcurrentBooking,
                        profile.ServiceAreaJson,
                        profile.ServiceLevelAgreementJson,
                        profile.CapacityRuleJson,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));
        }

        private static async Task UpsertSubscriptionProfileAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            Guid productId,
            DateTime now,
            UpsertProductSubscriptionProfileRequestDto? profile,
            CancellationToken cancellationToken)
        {
            if (profile is null)
            {
                return;
            }

            const string updateSql = @"
UPDATE [Product].[ProductSubscriptionProfiles]
SET
    BillingPeriodUnit = @BillingPeriodUnit,
    BillingPeriodValue = @BillingPeriodValue,
    TrialDays = @TrialDays,
    AutoRenew = @AutoRenew,
    GracePeriodDays = @GracePeriodDays,
    CancellationPolicy = @CancellationPolicy,
    SubscriptionRulesJson = @SubscriptionRulesJson,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        profile.BillingPeriodUnit,
                        profile.BillingPeriodValue,
                        profile.TrialDays,
                        profile.AutoRenew,
                        profile.GracePeriodDays,
                        profile.CancellationPolicy,
                        profile.SubscriptionRulesJson,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            if (affectedRows > 0)
            {
                return;
            }

            const string insertSql = @"
INSERT INTO [Product].[ProductSubscriptionProfiles]
(
    Id,
    ProductId,
    BillingPeriodUnit,
    BillingPeriodValue,
    TrialDays,
    AutoRenew,
    GracePeriodDays,
    CancellationPolicy,
    SubscriptionRulesJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @BillingPeriodUnit,
    @BillingPeriodValue,
    @TrialDays,
    @AutoRenew,
    @GracePeriodDays,
    @CancellationPolicy,
    @SubscriptionRulesJson,
    @Now,
    0
);";

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertSql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        profile.BillingPeriodUnit,
                        profile.BillingPeriodValue,
                        profile.TrialDays,
                        profile.AutoRenew,
                        profile.GracePeriodDays,
                        profile.CancellationPolicy,
                        profile.SubscriptionRulesJson,
                        Now = now
                    },
                    transaction,
                    cancellationToken: cancellationToken));
        }

        public async Task<bool> UpdateProductAsync(Guid productId, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
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
    UnitOfMeasure = @UnitOfMeasure,
    TaxRate = @TaxRate,
    TaxCode = @TaxCode,
    Tags = @Tags,
    MetadataJson = @MetadataJson,
    UpdatedAt = @Now
WHERE Id = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        ProductId = productId,
                        request.ProductCode,
                        request.Name,
                        request.ShortDescription,
                        request.Description,
                        request.Kind,
                        request.Status,
                        request.Brand,
                        request.Manufacturer,
                        request.Barcode,
                        request.IsActive,
                        request.IsSellable,
                        request.IsPurchasable,
                        request.TrackInventory,
                        request.DefaultCurrencyCode,
                        request.UnitOfMeasure,
                        request.TaxRate,
                        request.TaxCode,
                        request.Tags,
                        request.MetadataJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string deleteProductSql = @"
UPDATE [Product].[Products]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE Id = @ProductId AND IsDeleted = 0;";

            const string cascadeSql = @"
UPDATE [Product].[ProductAttributeValues]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductVariants]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductPrices]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductInventories]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductMediaItems]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductCategoryMaps]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductBundleItems]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE BundleProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductSupplierMaps]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductPhysicalProfiles]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductSoftwareProfiles]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductServiceProfiles]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;

UPDATE [Product].[ProductSubscriptionProfiles]
SET IsDeleted = 1, DeletedAt = @Now, UpdatedAt = @Now
WHERE ProductId = @ProductId AND IsDeleted = 0;
";

            var now = DateTime.UtcNow;
            var parameters = new { ProductId = productId, Now = now };

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var affectedRows = await connection.ExecuteAsync(
                    new CommandDefinition(deleteProductSql, parameters, transaction, cancellationToken: cancellationToken));

                if (affectedRows == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                await connection.ExecuteAsync(
                    new CommandDefinition(cascadeSql, parameters, transaction, cancellationToken: cancellationToken));

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IReadOnlyList<ProductAttributeDefinitionDto>> GetAttributeDefinitionsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    [Key],
    DisplayName,
    DataType,
    IsRequired,
    IsFilterable,
    IsVariantAxis,
    AllowedValuesJson,
    ValidationRuleJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductAttributeDefinitions]
WHERE IsDeleted = 0
ORDER BY DisplayName;";

            using var connection = CreateConnection();
            var definitions = await connection.QueryAsync<ProductAttributeDefinitionDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return definitions.AsList();
        }

        public async Task<ProductAttributeDefinitionDto?> GetAttributeDefinitionByIdAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    [Key],
    DisplayName,
    DataType,
    IsRequired,
    IsFilterable,
    IsVariantAxis,
    AllowedValuesJson,
    ValidationRuleJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductAttributeDefinitions]
WHERE Id = @AttributeDefinitionId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductAttributeDefinitionDto>(
                new CommandDefinition(sql, new { AttributeDefinitionId = attributeDefinitionId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductAttributeDefinitionDto> CreateAttributeDefinitionAsync(CreateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductAttributeDefinitions]
(
    Id,
    [Key],
    DisplayName,
    DataType,
    IsRequired,
    IsFilterable,
    IsVariantAxis,
    AllowedValuesJson,
    ValidationRuleJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @Key,
    @DisplayName,
    @DataType,
    @IsRequired,
    @IsFilterable,
    @IsVariantAxis,
    @AllowedValuesJson,
    @ValidationRuleJson,
    @Now,
    0
);";

            var definitionId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = definitionId,
                        request.Key,
                        request.DisplayName,
                        request.DataType,
                        request.IsRequired,
                        request.IsFilterable,
                        request.IsVariantAxis,
                        request.AllowedValuesJson,
                        request.ValidationRuleJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetAttributeDefinitionByIdAsync(definitionId, cancellationToken)
                ?? throw new InvalidOperationException("Attribute definition could not be loaded after insert.");
        }

        public async Task<bool> UpdateAttributeDefinitionAsync(Guid attributeDefinitionId, UpdateProductAttributeDefinitionRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductAttributeDefinitions]
SET
    [Key] = @Key,
    DisplayName = @DisplayName,
    DataType = @DataType,
    IsRequired = @IsRequired,
    IsFilterable = @IsFilterable,
    IsVariantAxis = @IsVariantAxis,
    AllowedValuesJson = @AllowedValuesJson,
    ValidationRuleJson = @ValidationRuleJson,
    UpdatedAt = @Now
WHERE Id = @AttributeDefinitionId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        AttributeDefinitionId = attributeDefinitionId,
                        request.Key,
                        request.DisplayName,
                        request.DataType,
                        request.IsRequired,
                        request.IsFilterable,
                        request.IsVariantAxis,
                        request.AllowedValuesJson,
                        request.ValidationRuleJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteAttributeDefinitionAsync(Guid attributeDefinitionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductAttributeDefinitions]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @AttributeDefinitionId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { AttributeDefinitionId = attributeDefinitionId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductAttributeValueDto>> GetProductAttributeValuesAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    AttributeDefinitionId,
    ValueText,
    ValueNumber,
    ValueBool,
    ValueDate,
    ValueJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductAttributeValues]
WHERE ProductId = @ProductId
  AND IsDeleted = 0
ORDER BY CreatedAt DESC;";

            using var connection = CreateConnection();
            var values = await connection.QueryAsync<ProductAttributeValueDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));

            return values.AsList();
        }

        public async Task<ProductAttributeValueDto?> GetAttributeValueByIdAsync(Guid attributeValueId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    AttributeDefinitionId,
    ValueText,
    ValueNumber,
    ValueBool,
    ValueDate,
    ValueJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductAttributeValues]
WHERE Id = @AttributeValueId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductAttributeValueDto>(
                new CommandDefinition(sql, new { AttributeValueId = attributeValueId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductAttributeValueDto> CreateAttributeValueAsync(CreateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductAttributeValues]
(
    Id,
    ProductId,
    AttributeDefinitionId,
    ValueText,
    ValueNumber,
    ValueBool,
    ValueDate,
    ValueJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @AttributeDefinitionId,
    @ValueText,
    @ValueNumber,
    @ValueBool,
    @ValueDate,
    @ValueJson,
    @Now,
    0
);";

            var attributeValueId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = attributeValueId,
                        request.ProductId,
                        request.AttributeDefinitionId,
                        request.ValueText,
                        request.ValueNumber,
                        request.ValueBool,
                        request.ValueDate,
                        request.ValueJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetAttributeValueByIdAsync(attributeValueId, cancellationToken)
                ?? throw new InvalidOperationException("Attribute value could not be loaded after insert.");
        }

        public async Task<bool> UpdateAttributeValueAsync(Guid attributeValueId, UpdateProductAttributeValueRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductAttributeValues]
SET
    ValueText = @ValueText,
    ValueNumber = @ValueNumber,
    ValueBool = @ValueBool,
    ValueDate = @ValueDate,
    ValueJson = @ValueJson,
    UpdatedAt = @Now
WHERE Id = @AttributeValueId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        AttributeValueId = attributeValueId,
                        request.ValueText,
                        request.ValueNumber,
                        request.ValueBool,
                        request.ValueDate,
                        request.ValueJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteAttributeValueAsync(Guid attributeValueId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductAttributeValues]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @AttributeValueId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { AttributeValueId = attributeValueId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Code,
    Name,
    Description,
    ParentCategoryId,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductCategories]
WHERE IsDeleted = 0
ORDER BY Name;";

            using var connection = CreateConnection();
            var categories = await connection.QueryAsync<ProductCategoryDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return categories.AsList();
        }

        public async Task<ProductCategoryDto?> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Code,
    Name,
    Description,
    ParentCategoryId,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductCategories]
WHERE Id = @CategoryId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductCategoryDto>(
                new CommandDefinition(sql, new { CategoryId = categoryId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductCategories]
(
    Id,
    Code,
    Name,
    Description,
    ParentCategoryId,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @Code,
    @Name,
    @Description,
    @ParentCategoryId,
    @Now,
    0
);";

            var categoryId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = categoryId,
                        request.Code,
                        request.Name,
                        request.Description,
                        request.ParentCategoryId,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetCategoryByIdAsync(categoryId, cancellationToken)
                ?? throw new InvalidOperationException("Category could not be loaded after insert.");
        }

        public async Task<bool> UpdateCategoryAsync(Guid categoryId, UpdateProductCategoryRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductCategories]
SET
    Code = @Code,
    Name = @Name,
    Description = @Description,
    ParentCategoryId = @ParentCategoryId,
    UpdatedAt = @Now
WHERE Id = @CategoryId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        CategoryId = categoryId,
                        request.Code,
                        request.Name,
                        request.Description,
                        request.ParentCategoryId,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductCategories]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @CategoryId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { CategoryId = categoryId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductCategoryMapDto>> GetProductCategoryMapsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    ProductCategoryId,
    IsPrimary,
    SortOrder,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductCategoryMaps]
WHERE ProductId = @ProductId
  AND IsDeleted = 0
ORDER BY IsPrimary DESC, SortOrder, CreatedAt DESC;";

            using var connection = CreateConnection();
            var maps = await connection.QueryAsync<ProductCategoryMapDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));

            return maps.AsList();
        }

        public async Task<ProductCategoryMapDto?> GetCategoryMapByIdAsync(Guid categoryMapId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    ProductCategoryId,
    IsPrimary,
    SortOrder,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductCategoryMaps]
WHERE Id = @CategoryMapId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductCategoryMapDto>(
                new CommandDefinition(sql, new { CategoryMapId = categoryMapId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductCategoryMapDto> CreateCategoryMapAsync(CreateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductCategoryMaps]
(
    Id,
    ProductId,
    ProductCategoryId,
    IsPrimary,
    SortOrder,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductCategoryId,
    @IsPrimary,
    @SortOrder,
    @Now,
    0
);";

            var mapId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = mapId,
                        request.ProductId,
                        request.ProductCategoryId,
                        request.IsPrimary,
                        request.SortOrder,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetCategoryMapByIdAsync(mapId, cancellationToken)
                ?? throw new InvalidOperationException("Category map could not be loaded after insert.");
        }

        public async Task<bool> UpdateCategoryMapAsync(Guid categoryMapId, UpdateProductCategoryMapRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductCategoryMaps]
SET
    IsPrimary = @IsPrimary,
    SortOrder = @SortOrder,
    UpdatedAt = @Now
WHERE Id = @CategoryMapId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        CategoryMapId = categoryMapId,
                        request.IsPrimary,
                        request.SortOrder,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteCategoryMapAsync(Guid categoryMapId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductCategoryMaps]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @CategoryMapId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { CategoryMapId = categoryMapId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductMediaDto>> GetProductMediaAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    MediaType,
    Url,
    ThumbnailUrl,
    MimeType,
    AltText,
    IsPrimary,
    SortOrder,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductMediaItems]
WHERE ProductId = @ProductId
  AND IsDeleted = 0
ORDER BY IsPrimary DESC, SortOrder, CreatedAt DESC;";

            using var connection = CreateConnection();
            var mediaItems = await connection.QueryAsync<ProductMediaDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));

            return mediaItems.AsList();
        }

        public async Task<ProductMediaDto?> GetMediaByIdAsync(Guid mediaId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    MediaType,
    Url,
    ThumbnailUrl,
    MimeType,
    AltText,
    IsPrimary,
    SortOrder,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductMediaItems]
WHERE Id = @MediaId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductMediaDto>(
                new CommandDefinition(sql, new { MediaId = mediaId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductMediaDto> CreateMediaAsync(CreateProductMediaRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductMediaItems]
(
    Id,
    ProductId,
    MediaType,
    Url,
    ThumbnailUrl,
    MimeType,
    AltText,
    IsPrimary,
    SortOrder,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @MediaType,
    @Url,
    @ThumbnailUrl,
    @MimeType,
    @AltText,
    @IsPrimary,
    @SortOrder,
    @Now,
    0
);";

            var mediaId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = mediaId,
                        request.ProductId,
                        request.MediaType,
                        request.Url,
                        request.ThumbnailUrl,
                        request.MimeType,
                        request.AltText,
                        request.IsPrimary,
                        request.SortOrder,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetMediaByIdAsync(mediaId, cancellationToken)
                ?? throw new InvalidOperationException("Media item could not be loaded after insert.");
        }

        public async Task<bool> UpdateMediaAsync(Guid mediaId, UpdateProductMediaRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductMediaItems]
SET
    MediaType = @MediaType,
    Url = @Url,
    ThumbnailUrl = @ThumbnailUrl,
    MimeType = @MimeType,
    AltText = @AltText,
    IsPrimary = @IsPrimary,
    SortOrder = @SortOrder,
    UpdatedAt = @Now
WHERE Id = @MediaId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        MediaId = mediaId,
                        request.MediaType,
                        request.Url,
                        request.ThumbnailUrl,
                        request.MimeType,
                        request.AltText,
                        request.IsPrimary,
                        request.SortOrder,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductMediaItems]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @MediaId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { MediaId = mediaId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductBundleItemDto>> GetBundleItemsAsync(Guid bundleProductId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    BundleProductId,
    ChildProductId,
    ChildVariantId,
    Quantity,
    IsOptional,
    RuleJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductBundleItems]
WHERE BundleProductId = @BundleProductId
  AND IsDeleted = 0
ORDER BY CreatedAt DESC;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductBundleItemDto>(
                new CommandDefinition(sql, new { BundleProductId = bundleProductId }, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<ProductBundleItemDto?> GetBundleItemByIdAsync(Guid bundleItemId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    BundleProductId,
    ChildProductId,
    ChildVariantId,
    Quantity,
    IsOptional,
    RuleJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductBundleItems]
WHERE Id = @BundleItemId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductBundleItemDto>(
                new CommandDefinition(sql, new { BundleItemId = bundleItemId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductBundleItemDto> CreateBundleItemAsync(CreateProductBundleItemRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductBundleItems]
(
    Id,
    BundleProductId,
    ChildProductId,
    ChildVariantId,
    Quantity,
    IsOptional,
    RuleJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @BundleProductId,
    @ChildProductId,
    @ChildVariantId,
    @Quantity,
    @IsOptional,
    @RuleJson,
    @Now,
    0
);";

            var bundleItemId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = bundleItemId,
                        request.BundleProductId,
                        request.ChildProductId,
                        request.ChildVariantId,
                        request.Quantity,
                        request.IsOptional,
                        request.RuleJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetBundleItemByIdAsync(bundleItemId, cancellationToken)
                ?? throw new InvalidOperationException("Bundle item could not be loaded after insert.");
        }

        public async Task<bool> UpdateBundleItemAsync(Guid bundleItemId, UpdateProductBundleItemRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductBundleItems]
SET
    ChildProductId = @ChildProductId,
    ChildVariantId = @ChildVariantId,
    Quantity = @Quantity,
    IsOptional = @IsOptional,
    RuleJson = @RuleJson,
    UpdatedAt = @Now
WHERE Id = @BundleItemId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        BundleItemId = bundleItemId,
                        request.ChildProductId,
                        request.ChildVariantId,
                        request.Quantity,
                        request.IsOptional,
                        request.RuleJson,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteBundleItemAsync(Guid bundleItemId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductBundleItems]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @BundleItemId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { BundleItemId = bundleItemId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductVariantDto>> GetProductVariantsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    Sku,
    Barcode,
    Name,
    OptionValuesJson,
    AdditionalPrice,
    AdditionalCost,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductVariants]
WHERE ProductId = @ProductId
  AND IsDeleted = 0
ORDER BY CreatedAt DESC;";

            using var connection = CreateConnection();
            var variants = await connection.QueryAsync<ProductVariantDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));

            return variants.AsList();
        }

        public async Task<ProductVariantDto?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    Sku,
    Barcode,
    Name,
    OptionValuesJson,
    AdditionalPrice,
    AdditionalCost,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductVariants]
WHERE Id = @VariantId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductVariantDto>(
                new CommandDefinition(sql, new { VariantId = variantId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductVariantDto> CreateVariantAsync(CreateProductVariantRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductVariants]
(
    Id,
    ProductId,
    Sku,
    Barcode,
    Name,
    OptionValuesJson,
    AdditionalPrice,
    AdditionalCost,
    IsActive,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @Sku,
    @Barcode,
    @Name,
    @OptionValuesJson,
    @AdditionalPrice,
    @AdditionalCost,
    @IsActive,
    @Now,
    0
);";

            var variantId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = variantId,
                        request.ProductId,
                        request.Sku,
                        request.Barcode,
                        request.Name,
                        request.OptionValuesJson,
                        request.AdditionalPrice,
                        request.AdditionalCost,
                        request.IsActive,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetVariantByIdAsync(variantId, cancellationToken)
                ?? throw new InvalidOperationException("Product variant could not be loaded after insert.");
        }

        public async Task<bool> UpdateVariantAsync(Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductVariants]
SET
    Sku = @Sku,
    Barcode = @Barcode,
    Name = @Name,
    OptionValuesJson = @OptionValuesJson,
    AdditionalPrice = @AdditionalPrice,
    AdditionalCost = @AdditionalCost,
    IsActive = @IsActive,
    UpdatedAt = @Now
WHERE Id = @VariantId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        VariantId = variantId,
                        request.Sku,
                        request.Barcode,
                        request.Name,
                        request.OptionValuesJson,
                        request.AdditionalPrice,
                        request.AdditionalCost,
                        request.IsActive,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductVariants]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @VariantId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { VariantId = variantId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductPriceDto>> GetProductPricesAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    ProductVariantId,
    PriceType,
    Amount,
    CompareAtAmount,
    CurrencyCode,
    MinQuantity,
    MaxQuantity,
    ValidFrom,
    ValidTo,
    SalesChannel,
    CustomerGroupCode,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductPrices]
WHERE ProductId = @ProductId
  AND IsDeleted = 0
ORDER BY CreatedAt DESC;";

            using var connection = CreateConnection();
            var prices = await connection.QueryAsync<ProductPriceDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));

            return prices.AsList();
        }

        public async Task<ProductPriceDto?> GetPriceByIdAsync(Guid priceId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    ProductVariantId,
    PriceType,
    Amount,
    CompareAtAmount,
    CurrencyCode,
    MinQuantity,
    MaxQuantity,
    ValidFrom,
    ValidTo,
    SalesChannel,
    CustomerGroupCode,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductPrices]
WHERE Id = @PriceId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductPriceDto>(
                new CommandDefinition(sql, new { PriceId = priceId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductPriceDto> CreatePriceAsync(CreateProductPriceRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductPrices]
(
    Id,
    ProductId,
    ProductVariantId,
    PriceType,
    Amount,
    CompareAtAmount,
    CurrencyCode,
    MinQuantity,
    MaxQuantity,
    ValidFrom,
    ValidTo,
    SalesChannel,
    CustomerGroupCode,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductVariantId,
    @PriceType,
    @Amount,
    @CompareAtAmount,
    @CurrencyCode,
    @MinQuantity,
    @MaxQuantity,
    @ValidFrom,
    @ValidTo,
    @SalesChannel,
    @CustomerGroupCode,
    @Now,
    0
);";

            var priceId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = priceId,
                        request.ProductId,
                        request.ProductVariantId,
                        request.PriceType,
                        request.Amount,
                        request.CompareAtAmount,
                        request.CurrencyCode,
                        request.MinQuantity,
                        request.MaxQuantity,
                        request.ValidFrom,
                        request.ValidTo,
                        request.SalesChannel,
                        request.CustomerGroupCode,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetPriceByIdAsync(priceId, cancellationToken)
                ?? throw new InvalidOperationException("Product price could not be loaded after insert.");
        }

        public async Task<bool> UpdatePriceAsync(Guid priceId, UpdateProductPriceRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPrices]
SET
    ProductVariantId = @ProductVariantId,
    PriceType = @PriceType,
    Amount = @Amount,
    CompareAtAmount = @CompareAtAmount,
    CurrencyCode = @CurrencyCode,
    MinQuantity = @MinQuantity,
    MaxQuantity = @MaxQuantity,
    ValidFrom = @ValidFrom,
    ValidTo = @ValidTo,
    SalesChannel = @SalesChannel,
    CustomerGroupCode = @CustomerGroupCode,
    UpdatedAt = @Now
WHERE Id = @PriceId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        PriceId = priceId,
                        request.ProductVariantId,
                        request.PriceType,
                        request.Amount,
                        request.CompareAtAmount,
                        request.CurrencyCode,
                        request.MinQuantity,
                        request.MaxQuantity,
                        request.ValidFrom,
                        request.ValidTo,
                        request.SalesChannel,
                        request.CustomerGroupCode,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeletePriceAsync(Guid priceId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPrices]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @PriceId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { PriceId = priceId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductInventoryDto>> GetProductInventoriesAsync(ProductInventoryFilterDto filter, CancellationToken cancellationToken = default)
        {
            var take = NormalizeTake(filter.Take);

            const string sql = @"
SELECT TOP (@Take)
    Id,
    ProductId,
    ProductVariantId,
    WarehouseId,
    WarehouseCode,
    QuantityOnHand,
    QuantityReserved,
    QuantityOnHand - QuantityReserved AS QuantityAvailable,
    ReorderPoint,
    ReorderQuantity,
    InventoryPolicy,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductInventories]
WHERE IsDeleted = 0
  AND (@ProductId IS NULL OR ProductId = @ProductId)
  AND (@ProductVariantId IS NULL OR ProductVariantId = @ProductVariantId)
  AND (@WarehouseId IS NULL OR WarehouseId = @WarehouseId)
ORDER BY UpdatedAt DESC, CreatedAt DESC;";

            using var connection = CreateConnection();
            var inventories = await connection.QueryAsync<ProductInventoryDto>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Take = take,
                        filter.ProductId,
                        filter.ProductVariantId,
                        filter.WarehouseId
                    },
                    cancellationToken: cancellationToken));

            return inventories.AsList();
        }

        public async Task<ProductInventoryDto?> GetInventoryByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    ProductVariantId,
    WarehouseId,
    WarehouseCode,
    QuantityOnHand,
    QuantityReserved,
    QuantityOnHand - QuantityReserved AS QuantityAvailable,
    ReorderPoint,
    ReorderQuantity,
    InventoryPolicy,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductInventories]
WHERE Id = @InventoryId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductInventoryDto>(
                new CommandDefinition(sql, new { InventoryId = inventoryId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductInventoryDto> CreateInventoryAsync(CreateProductInventoryRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductInventories]
(
    Id,
    ProductId,
    ProductVariantId,
    WarehouseId,
    WarehouseCode,
    QuantityOnHand,
    QuantityReserved,
    ReorderPoint,
    ReorderQuantity,
    InventoryPolicy,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @ProductVariantId,
    @WarehouseId,
    @WarehouseCode,
    @QuantityOnHand,
    @QuantityReserved,
    @ReorderPoint,
    @ReorderQuantity,
    @InventoryPolicy,
    @Now,
    0
);";

            var inventoryId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = inventoryId,
                        request.ProductId,
                        request.ProductVariantId,
                        request.WarehouseId,
                        request.WarehouseCode,
                        request.QuantityOnHand,
                        request.QuantityReserved,
                        request.ReorderPoint,
                        request.ReorderQuantity,
                        request.InventoryPolicy,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetInventoryByIdAsync(inventoryId, cancellationToken)
                ?? throw new InvalidOperationException("Product inventory could not be loaded after insert.");
        }

        public async Task<bool> UpdateInventoryAsync(Guid inventoryId, UpdateProductInventoryRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductInventories]
SET
    ProductVariantId = @ProductVariantId,
    WarehouseId = @WarehouseId,
    WarehouseCode = @WarehouseCode,
    QuantityOnHand = @QuantityOnHand,
    QuantityReserved = @QuantityReserved,
    ReorderPoint = @ReorderPoint,
    ReorderQuantity = @ReorderQuantity,
    InventoryPolicy = @InventoryPolicy,
    UpdatedAt = @Now
WHERE Id = @InventoryId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        InventoryId = inventoryId,
                        request.ProductVariantId,
                        request.WarehouseId,
                        request.WarehouseCode,
                        request.QuantityOnHand,
                        request.QuantityReserved,
                        request.ReorderPoint,
                        request.ReorderQuantity,
                        request.InventoryPolicy,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductInventories]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @InventoryId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { InventoryId = inventoryId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<ProductPhysicalProfileDto?> GetPhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    Weight,
    Width,
    Height,
    Length,
    RequiresShipping,
    IsFragile,
    IsHazardous,
    RequiresSerialNumber,
    WarrantyInMonths,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductPhysicalProfiles]
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductPhysicalProfileDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductPhysicalProfileDto> UpsertPhysicalProfileAsync(Guid productId, UpsertProductPhysicalProfileRequestDto request, CancellationToken cancellationToken = default)
        {
            const string updateSql = @"
UPDATE [Product].[ProductPhysicalProfiles]
SET
    Weight = @Weight,
    Width = @Width,
    Height = @Height,
    Length = @Length,
    RequiresShipping = @RequiresShipping,
    IsFragile = @IsFragile,
    IsHazardous = @IsHazardous,
    RequiresSerialNumber = @RequiresSerialNumber,
    WarrantyInMonths = @WarrantyInMonths,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            using var connection = CreateConnection();
            var now = DateTime.UtcNow;

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        request.Weight,
                        request.Width,
                        request.Height,
                        request.Length,
                        request.RequiresShipping,
                        request.IsFragile,
                        request.IsHazardous,
                        request.RequiresSerialNumber,
                        request.WarrantyInMonths,
                        Now = now
                    },
                    cancellationToken: cancellationToken));

            if (affectedRows == 0)
            {
                const string insertSql = @"
INSERT INTO [Product].[ProductPhysicalProfiles]
(
    Id,
    ProductId,
    Weight,
    Width,
    Height,
    Length,
    RequiresShipping,
    IsFragile,
    IsHazardous,
    RequiresSerialNumber,
    WarrantyInMonths,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @Weight,
    @Width,
    @Height,
    @Length,
    @RequiresShipping,
    @IsFragile,
    @IsHazardous,
    @RequiresSerialNumber,
    @WarrantyInMonths,
    @Now,
    0
);";

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        insertSql,
                        new
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            request.Weight,
                            request.Width,
                            request.Height,
                            request.Length,
                            request.RequiresShipping,
                            request.IsFragile,
                            request.IsHazardous,
                            request.RequiresSerialNumber,
                            request.WarrantyInMonths,
                            Now = now
                        },
                        cancellationToken: cancellationToken));
            }

            return await GetPhysicalProfileAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Physical profile could not be loaded after upsert.");
        }

        public async Task<bool> DeletePhysicalProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPhysicalProfiles]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { ProductId = productId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<ProductSoftwareProfileDto?> GetSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    Version,
    LicenseModel,
    SeatCount,
    DownloadUrl,
    SupportedPlatformsJson,
    SystemRequirementsJson,
    ReleaseNotes,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductSoftwareProfiles]
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductSoftwareProfileDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductSoftwareProfileDto> UpsertSoftwareProfileAsync(Guid productId, UpsertProductSoftwareProfileRequestDto request, CancellationToken cancellationToken = default)
        {
            const string updateSql = @"
UPDATE [Product].[ProductSoftwareProfiles]
SET
    Version = @Version,
    LicenseModel = @LicenseModel,
    SeatCount = @SeatCount,
    DownloadUrl = @DownloadUrl,
    SupportedPlatformsJson = @SupportedPlatformsJson,
    SystemRequirementsJson = @SystemRequirementsJson,
    ReleaseNotes = @ReleaseNotes,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            using var connection = CreateConnection();
            var now = DateTime.UtcNow;

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        request.Version,
                        request.LicenseModel,
                        request.SeatCount,
                        request.DownloadUrl,
                        request.SupportedPlatformsJson,
                        request.SystemRequirementsJson,
                        request.ReleaseNotes,
                        Now = now
                    },
                    cancellationToken: cancellationToken));

            if (affectedRows == 0)
            {
                const string insertSql = @"
INSERT INTO [Product].[ProductSoftwareProfiles]
(
    Id,
    ProductId,
    Version,
    LicenseModel,
    SeatCount,
    DownloadUrl,
    SupportedPlatformsJson,
    SystemRequirementsJson,
    ReleaseNotes,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @Version,
    @LicenseModel,
    @SeatCount,
    @DownloadUrl,
    @SupportedPlatformsJson,
    @SystemRequirementsJson,
    @ReleaseNotes,
    @Now,
    0
);";

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        insertSql,
                        new
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            request.Version,
                            request.LicenseModel,
                            request.SeatCount,
                            request.DownloadUrl,
                            request.SupportedPlatformsJson,
                            request.SystemRequirementsJson,
                            request.ReleaseNotes,
                            Now = now
                        },
                        cancellationToken: cancellationToken));
            }

            return await GetSoftwareProfileAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Software profile could not be loaded after upsert.");
        }

        public async Task<bool> DeleteSoftwareProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductSoftwareProfiles]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { ProductId = productId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<ProductServiceProfileDto?> GetServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    DeliveryMode,
    DurationInMinutes,
    MaxConcurrentBooking,
    ServiceAreaJson,
    ServiceLevelAgreementJson,
    CapacityRuleJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductServiceProfiles]
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductServiceProfileDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductServiceProfileDto> UpsertServiceProfileAsync(Guid productId, UpsertProductServiceProfileRequestDto request, CancellationToken cancellationToken = default)
        {
            const string updateSql = @"
UPDATE [Product].[ProductServiceProfiles]
SET
    DeliveryMode = @DeliveryMode,
    DurationInMinutes = @DurationInMinutes,
    MaxConcurrentBooking = @MaxConcurrentBooking,
    ServiceAreaJson = @ServiceAreaJson,
    ServiceLevelAgreementJson = @ServiceLevelAgreementJson,
    CapacityRuleJson = @CapacityRuleJson,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            using var connection = CreateConnection();
            var now = DateTime.UtcNow;

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        request.DeliveryMode,
                        request.DurationInMinutes,
                        request.MaxConcurrentBooking,
                        request.ServiceAreaJson,
                        request.ServiceLevelAgreementJson,
                        request.CapacityRuleJson,
                        Now = now
                    },
                    cancellationToken: cancellationToken));

            if (affectedRows == 0)
            {
                const string insertSql = @"
INSERT INTO [Product].[ProductServiceProfiles]
(
    Id,
    ProductId,
    DeliveryMode,
    DurationInMinutes,
    MaxConcurrentBooking,
    ServiceAreaJson,
    ServiceLevelAgreementJson,
    CapacityRuleJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @DeliveryMode,
    @DurationInMinutes,
    @MaxConcurrentBooking,
    @ServiceAreaJson,
    @ServiceLevelAgreementJson,
    @CapacityRuleJson,
    @Now,
    0
);";

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        insertSql,
                        new
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            request.DeliveryMode,
                            request.DurationInMinutes,
                            request.MaxConcurrentBooking,
                            request.ServiceAreaJson,
                            request.ServiceLevelAgreementJson,
                            request.CapacityRuleJson,
                            Now = now
                        },
                        cancellationToken: cancellationToken));
            }

            return await GetServiceProfileAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Service profile could not be loaded after upsert.");
        }

        public async Task<bool> DeleteServiceProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductServiceProfiles]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { ProductId = productId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<ProductSubscriptionProfileDto?> GetSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    ProductId,
    BillingPeriodUnit,
    BillingPeriodValue,
    TrialDays,
    AutoRenew,
    GracePeriodDays,
    CancellationPolicy,
    SubscriptionRulesJson,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductSubscriptionProfiles]
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductSubscriptionProfileDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductSubscriptionProfileDto> UpsertSubscriptionProfileAsync(Guid productId, UpsertProductSubscriptionProfileRequestDto request, CancellationToken cancellationToken = default)
        {
            const string updateSql = @"
UPDATE [Product].[ProductSubscriptionProfiles]
SET
    BillingPeriodUnit = @BillingPeriodUnit,
    BillingPeriodValue = @BillingPeriodValue,
    TrialDays = @TrialDays,
    AutoRenew = @AutoRenew,
    GracePeriodDays = @GracePeriodDays,
    CancellationPolicy = @CancellationPolicy,
    SubscriptionRulesJson = @SubscriptionRulesJson,
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = @Now
WHERE ProductId = @ProductId;";

            using var connection = CreateConnection();
            var now = DateTime.UtcNow;

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    updateSql,
                    new
                    {
                        ProductId = productId,
                        request.BillingPeriodUnit,
                        request.BillingPeriodValue,
                        request.TrialDays,
                        request.AutoRenew,
                        request.GracePeriodDays,
                        request.CancellationPolicy,
                        request.SubscriptionRulesJson,
                        Now = now
                    },
                    cancellationToken: cancellationToken));

            if (affectedRows == 0)
            {
                const string insertSql = @"
INSERT INTO [Product].[ProductSubscriptionProfiles]
(
    Id,
    ProductId,
    BillingPeriodUnit,
    BillingPeriodValue,
    TrialDays,
    AutoRenew,
    GracePeriodDays,
    CancellationPolicy,
    SubscriptionRulesJson,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @ProductId,
    @BillingPeriodUnit,
    @BillingPeriodValue,
    @TrialDays,
    @AutoRenew,
    @GracePeriodDays,
    @CancellationPolicy,
    @SubscriptionRulesJson,
    @Now,
    0
);";

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        insertSql,
                        new
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            request.BillingPeriodUnit,
                            request.BillingPeriodValue,
                            request.TrialDays,
                            request.AutoRenew,
                            request.GracePeriodDays,
                            request.CancellationPolicy,
                            request.SubscriptionRulesJson,
                            Now = now
                        },
                        cancellationToken: cancellationToken));
            }

            return await GetSubscriptionProfileAsync(productId, cancellationToken)
                ?? throw new InvalidOperationException("Subscription profile could not be loaded after upsert.");
        }

        public async Task<bool> DeleteSubscriptionProfileAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductSubscriptionProfiles]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE ProductId = @ProductId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { ProductId = productId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }
    }
}
