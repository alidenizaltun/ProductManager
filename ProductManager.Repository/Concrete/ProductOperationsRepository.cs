using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProductManager.Repository.Shared.Abstract;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Extensions;
using System.Data;

namespace ProductManager.Repository.Concrete
{
    public sealed partial class ProductOperationsRepository : IProductOperationsRepository
    {
        private const int DefaultTake = 100;
        private const int MaxTake = 500;
        private readonly string _connectionString;

        public ProductOperationsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetActiveConnectionString();
        }

        public async Task<IReadOnlyList<LookupItemDto>> GetProductLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    CONCAT(ProductCode, ' - ', Name) AS Name
FROM [Product].[Products]
WHERE IsDeleted = 0
  AND (@IncludeInactive = 1 OR IsActive = 1)
ORDER BY Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<LookupItemDto>(
                new CommandDefinition(sql, new { IncludeInactive = includeInactive }, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<IReadOnlyList<LookupItemDto>> GetCategoryLookupsAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Name
FROM [Product].[ProductCategories]
WHERE IsDeleted = 0
ORDER BY Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<LookupItemDto>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<IReadOnlyList<ProductSupplierDto>> GetSuppliersAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    SupplierCode,
    Name,
    TaxNumber,
    Email,
    Phone,
    Address,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductSuppliers]
WHERE IsDeleted = 0
  AND (@IncludeInactive = 1 OR IsActive = 1)
ORDER BY Name;";

            using var connection = CreateConnection();
            var suppliers = await connection.QueryAsync<ProductSupplierDto>(
                new CommandDefinition(sql, new { IncludeInactive = includeInactive }, cancellationToken: cancellationToken));

            return suppliers.AsList();
        }

        public async Task<IReadOnlyList<LookupItemDto>> GetSupplierLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Name
FROM [Product].[ProductSuppliers]
WHERE IsDeleted = 0
  AND (@IncludeInactive = 1 OR IsActive = 1)
ORDER BY Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<LookupItemDto>(
                new CommandDefinition(sql, new { IncludeInactive = includeInactive }, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<ProductSupplierDto?> GetSupplierByIdAsync(Guid supplierId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    SupplierCode,
    Name,
    TaxNumber,
    Email,
    Phone,
    Address,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductSuppliers]
WHERE Id = @SupplierId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductSupplierDto>(
                new CommandDefinition(sql, new { SupplierId = supplierId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductSupplierDto> CreateSupplierAsync(CreateProductSupplierRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductSuppliers]
(
    Id,
    SupplierCode,
    Name,
    TaxNumber,
    Email,
    Phone,
    Address,
    IsActive,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @SupplierCode,
    @Name,
    @TaxNumber,
    @Email,
    @Phone,
    @Address,
    @IsActive,
    @Now,
    0
);";

            var now = DateTime.UtcNow;

            var supplierId = await InsertWithGeneratedCodeAsync(
                request.SupplierCode,
                SupplierCodeSource,
                async (connection, transaction, code, ct) =>
                {
                    var id = Guid.NewGuid();
                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            sql,
                            new
                            {
                                Id = id,
                                SupplierCode = code,
                                request.Name,
                                request.TaxNumber,
                                request.Email,
                                request.Phone,
                                request.Address,
                                request.IsActive,
                                Now = now
                            },
                            transaction,
                            cancellationToken: ct));
                    return id;
                },
                cancellationToken);

            return await GetSupplierByIdAsync(supplierId, cancellationToken)
                ?? throw new InvalidOperationException("Supplier could not be loaded after insert.");
        }

        public async Task<bool> UpdateSupplierAsync(Guid supplierId, UpdateProductSupplierRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductSuppliers]
SET
    SupplierCode = @SupplierCode,
    Name = @Name,
    TaxNumber = @TaxNumber,
    Email = @Email,
    Phone = @Phone,
    Address = @Address,
    IsActive = @IsActive,
    UpdatedAt = @Now
WHERE Id = @SupplierId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        SupplierId = supplierId,
                        request.SupplierCode,
                        request.Name,
                        request.TaxNumber,
                        request.Email,
                        request.Phone,
                        request.Address,
                        request.IsActive,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteSupplierAsync(Guid supplierId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductSuppliers]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @SupplierId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { SupplierId = supplierId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductSupplierMapDto>> GetProductSupplierMapsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    m.Id,
    m.ProductId,
    m.ProductSupplierId,
    s.SupplierCode,
    s.Name AS SupplierName,
    m.SupplierProductCode,
    m.SupplierCost,
    m.LeadTimeInDays,
    m.MinOrderQuantity,
    m.IsPreferred,
    m.CreatedAt,
    m.UpdatedAt
FROM [Product].[ProductSupplierMaps] m
LEFT JOIN [Product].[ProductSuppliers] s ON s.Id = m.ProductSupplierId AND s.IsDeleted = 0
WHERE m.ProductId = @ProductId
  AND m.IsDeleted = 0
ORDER BY m.IsPreferred DESC, m.CreatedAt DESC;";

            using var connection = CreateConnection();
            var maps = await connection.QueryAsync<ProductSupplierMapDto>(
                new CommandDefinition(sql, new { ProductId = productId }, cancellationToken: cancellationToken));

            return maps.AsList();
        }

        public async Task<ProductSupplierMapDto?> GetSupplierMapByIdAsync(Guid supplierMapId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    m.Id,
    m.ProductId,
    m.ProductSupplierId,
    s.SupplierCode,
    s.Name AS SupplierName,
    m.SupplierProductCode,
    m.SupplierCost,
    m.LeadTimeInDays,
    m.MinOrderQuantity,
    m.IsPreferred,
    m.CreatedAt,
    m.UpdatedAt
FROM [Product].[ProductSupplierMaps] m
LEFT JOIN [Product].[ProductSuppliers] s ON s.Id = m.ProductSupplierId AND s.IsDeleted = 0
WHERE m.Id = @SupplierMapId
  AND m.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductSupplierMapDto>(
                new CommandDefinition(sql, new { SupplierMapId = supplierMapId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductSupplierMapDto> CreateSupplierMapAsync(CreateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default)
        {
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

            var mapId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = mapId,
                        request.ProductId,
                        request.ProductSupplierId,
                        request.SupplierProductCode,
                        request.SupplierCost,
                        request.LeadTimeInDays,
                        request.MinOrderQuantity,
                        request.IsPreferred,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetSupplierMapByIdAsync(mapId, cancellationToken)
                ?? throw new InvalidOperationException("Supplier map could not be loaded after insert.");
        }

        public async Task<bool> UpdateSupplierMapAsync(Guid supplierMapId, UpdateProductSupplierMapRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductSupplierMaps]
SET
    SupplierProductCode = @SupplierProductCode,
    SupplierCost = @SupplierCost,
    LeadTimeInDays = @LeadTimeInDays,
    MinOrderQuantity = @MinOrderQuantity,
    IsPreferred = @IsPreferred,
    UpdatedAt = @Now
WHERE Id = @SupplierMapId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        SupplierMapId = supplierMapId,
                        request.SupplierProductCode,
                        request.SupplierCost,
                        request.LeadTimeInDays,
                        request.MinOrderQuantity,
                        request.IsPreferred,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteSupplierMapAsync(Guid supplierMapId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductSupplierMaps]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @SupplierMapId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { SupplierMapId = supplierMapId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<WarehouseDto>> GetWarehousesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Code,
    Name,
    Description,
    Address,
    City,
    Country,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM [Product].[Warehouses]
WHERE IsDeleted = 0
  AND (@IncludeInactive = 1 OR IsActive = 1)
ORDER BY Name;";

            using var connection = CreateConnection();
            var warehouses = await connection.QueryAsync<WarehouseDto>(
                new CommandDefinition(sql, new { IncludeInactive = includeInactive }, cancellationToken: cancellationToken));

            return warehouses.AsList();
        }

        public async Task<IReadOnlyList<LookupItemDto>> GetWarehouseLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    CONCAT(Code, ' - ', Name) AS Name
FROM [Product].[Warehouses]
WHERE IsDeleted = 0
  AND (@IncludeInactive = 1 OR IsActive = 1)
ORDER BY Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<LookupItemDto>(
                new CommandDefinition(sql, new { IncludeInactive = includeInactive }, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<WarehouseDto?> GetWarehouseByIdAsync(Guid warehouseId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Code,
    Name,
    Description,
    Address,
    City,
    Country,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM [Product].[Warehouses]
WHERE Id = @WarehouseId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<WarehouseDto>(
                new CommandDefinition(sql, new { WarehouseId = warehouseId }, cancellationToken: cancellationToken));
        }

        public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[Warehouses]
(
    Id,
    Code,
    Name,
    Description,
    Address,
    City,
    Country,
    IsActive,
    CreatedAt,
    IsDeleted
)
VALUES
(
    @Id,
    @Code,
    @Name,
    @Description,
    @Address,
    @City,
    @Country,
    @IsActive,
    @Now,
    0
);";

            var warehouseId = await InsertWithGeneratedCodeAsync(
                request.Code,
                WarehouseCodeSource,
                async (connection, transaction, code, ct) =>
                {
                    var id = Guid.NewGuid();
                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            sql,
                            new
                            {
                                Id = id,
                                Code = code,
                                request.Name,
                                request.Description,
                                request.Address,
                                request.City,
                                request.Country,
                                request.IsActive,
                                Now = DateTime.UtcNow
                            },
                            transaction,
                            cancellationToken: ct));
                    return id;
                },
                cancellationToken);

            return await GetWarehouseByIdAsync(warehouseId, cancellationToken)
                ?? throw new InvalidOperationException("Warehouse could not be loaded after insert.");
        }

        public async Task<bool> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[Warehouses]
SET
    Code = @Code,
    Name = @Name,
    Description = @Description,
    Address = @Address,
    City = @City,
    Country = @Country,
    IsActive = @IsActive,
    UpdatedAt = @Now
WHERE Id = @WarehouseId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        WarehouseId = warehouseId,
                        request.Code,
                        request.Name,
                        request.Description,
                        request.Address,
                        request.City,
                        request.Country,
                        request.IsActive,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteWarehouseAsync(Guid warehouseId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[Warehouses]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @WarehouseId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { WarehouseId = warehouseId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<InventoryTransactionDto>> GetInventoryTransactionsAsync(InventoryTransactionFilterDto filter, CancellationToken cancellationToken = default)
        {
            var take = NormalizeTake(filter.Take);

            const string sql = @"
SELECT TOP (@Take)
    t.Id,
    t.ProductId,
    t.ProductVariantId,
    v.Sku AS VariantSku,
    v.Name AS VariantName,
    t.WarehouseId,
    w.Name AS WarehouseName,
    t.TransactionType,
    t.Quantity,
    t.UnitCost,
    t.ReferenceType,
    t.ReferenceNumber,
    t.Note,
    t.OccurredAt,
    t.CreatedAt
FROM [Product].[InventoryTransactions] t
LEFT JOIN [Product].[ProductVariants] v ON v.Id = t.ProductVariantId AND v.IsDeleted = 0
LEFT JOIN [Product].[Warehouses] w ON w.Id = t.WarehouseId AND w.IsDeleted = 0
WHERE t.IsDeleted = 0
  AND (@ProductId IS NULL OR t.ProductId = @ProductId)
  AND (@ProductVariantId IS NULL OR t.ProductVariantId = @ProductVariantId)
  AND (@WarehouseId IS NULL OR t.WarehouseId = @WarehouseId)
  AND (@TransactionType IS NULL OR t.TransactionType = @TransactionType)
  AND (@DateFrom IS NULL OR t.OccurredAt >= @DateFrom)
  AND (@DateTo IS NULL OR t.OccurredAt <= @DateTo)
ORDER BY t.OccurredAt DESC, t.CreatedAt DESC;";

            using var connection = CreateConnection();
            var transactions = await connection.QueryAsync<InventoryTransactionDto>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Take = take,
                        filter.ProductId,
                        filter.ProductVariantId,
                        filter.WarehouseId,
                        filter.TransactionType,
                        filter.DateFrom,
                        filter.DateTo
                    },
                    cancellationToken: cancellationToken));

            return transactions.AsList();
        }

        public async Task<InventoryTransactionDto?> GetInventoryTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    t.Id,
    t.ProductId,
    t.ProductVariantId,
    v.Sku AS VariantSku,
    v.Name AS VariantName,
    t.WarehouseId,
    w.Name AS WarehouseName,
    t.TransactionType,
    t.Quantity,
    t.UnitCost,
    t.ReferenceType,
    t.ReferenceNumber,
    t.Note,
    t.OccurredAt,
    t.CreatedAt
FROM [Product].[InventoryTransactions] t
LEFT JOIN [Product].[ProductVariants] v ON v.Id = t.ProductVariantId AND v.IsDeleted = 0
LEFT JOIN [Product].[Warehouses] w ON w.Id = t.WarehouseId AND w.IsDeleted = 0
WHERE t.Id = @TransactionId
  AND t.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<InventoryTransactionDto>(
                new CommandDefinition(sql, new { TransactionId = transactionId }, cancellationToken: cancellationToken));
        }

        public async Task<InventoryTransactionDto> CreateInventoryTransactionAsync(CreateInventoryTransactionRequestDto request, CancellationToken cancellationToken = default)
        {
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

            var transactionId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var occurredAt = request.OccurredAt ?? now;

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = transactionId,
                        request.ProductId,
                        request.ProductVariantId,
                        request.WarehouseId,
                        request.TransactionType,
                        request.Quantity,
                        request.UnitCost,
                        request.ReferenceType,
                        request.ReferenceNumber,
                        request.Note,
                        OccurredAt = occurredAt,
                        Now = now
                    },
                    cancellationToken: cancellationToken));

            return await GetInventoryTransactionByIdAsync(transactionId, cancellationToken)
                ?? throw new InvalidOperationException("Inventory transaction could not be loaded after insert.");
        }

        public async Task<IReadOnlyList<InventoryReservationDto>> GetInventoryReservationsAsync(InventoryReservationFilterDto filter, CancellationToken cancellationToken = default)
        {
            var take = NormalizeTake(filter.Take);

            const string sql = @"
SELECT TOP (@Take)
    r.Id,
    r.ProductId,
    r.ProductVariantId,
    v.Sku AS VariantSku,
    v.Name AS VariantName,
    r.WarehouseId,
    w.Name AS WarehouseName,
    r.Quantity,
    r.ReservationCode,
    r.ReservedUntil,
    r.Status,
    r.SourceType,
    r.SourceId,
    r.CreatedAt,
    r.UpdatedAt
FROM [Product].[InventoryReservations] r
LEFT JOIN [Product].[ProductVariants] v ON v.Id = r.ProductVariantId AND v.IsDeleted = 0
LEFT JOIN [Product].[Warehouses] w ON w.Id = r.WarehouseId AND w.IsDeleted = 0
WHERE r.IsDeleted = 0
  AND (@ProductId IS NULL OR r.ProductId = @ProductId)
  AND (@ProductVariantId IS NULL OR r.ProductVariantId = @ProductVariantId)
  AND (@WarehouseId IS NULL OR r.WarehouseId = @WarehouseId)
  AND (@Status IS NULL OR r.Status = @Status)
  AND (@ReservedUntilMin IS NULL OR r.ReservedUntil >= @ReservedUntilMin)
  AND (@ReservedUntilMax IS NULL OR r.ReservedUntil <= @ReservedUntilMax)
ORDER BY r.CreatedAt DESC;";

            using var connection = CreateConnection();
            var reservations = await connection.QueryAsync<InventoryReservationDto>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Take = take,
                        filter.ProductId,
                        filter.ProductVariantId,
                        filter.WarehouseId,
                        filter.Status,
                        filter.ReservedUntilMin,
                        filter.ReservedUntilMax
                    },
                    cancellationToken: cancellationToken));

            return reservations.AsList();
        }

        public async Task<InventoryReservationDto?> GetInventoryReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    r.Id,
    r.ProductId,
    r.ProductVariantId,
    v.Sku AS VariantSku,
    v.Name AS VariantName,
    r.WarehouseId,
    w.Name AS WarehouseName,
    r.Quantity,
    r.ReservationCode,
    r.ReservedUntil,
    r.Status,
    r.SourceType,
    r.SourceId,
    r.CreatedAt,
    r.UpdatedAt
FROM [Product].[InventoryReservations] r
LEFT JOIN [Product].[ProductVariants] v ON v.Id = r.ProductVariantId AND v.IsDeleted = 0
LEFT JOIN [Product].[Warehouses] w ON w.Id = r.WarehouseId AND w.IsDeleted = 0
WHERE r.Id = @ReservationId
  AND r.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<InventoryReservationDto>(
                new CommandDefinition(sql, new { ReservationId = reservationId }, cancellationToken: cancellationToken));
        }

        public async Task<InventoryReservationDto> CreateInventoryReservationAsync(CreateInventoryReservationRequestDto request, CancellationToken cancellationToken = default)
        {
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

            var reservationId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = reservationId,
                        request.ProductId,
                        request.ProductVariantId,
                        request.WarehouseId,
                        request.Quantity,
                        request.ReservationCode,
                        request.ReservedUntil,
                        request.Status,
                        request.SourceType,
                        request.SourceId,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetInventoryReservationByIdAsync(reservationId, cancellationToken)
                ?? throw new InvalidOperationException("Inventory reservation could not be loaded after insert.");
        }

        public async Task<bool> UpdateInventoryReservationStatusAsync(Guid reservationId, UpdateInventoryReservationStatusRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[InventoryReservations]
SET
    Status = @Status,
    ReservedUntil = @ReservedUntil,
    UpdatedAt = @Now
WHERE Id = @ReservationId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        ReservationId = reservationId,
                        request.Status,
                        request.ReservedUntil,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeleteInventoryReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[InventoryReservations]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @ReservationId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { ReservationId = reservationId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductPriceListDto>> GetPriceListsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Code,
    Name,
    Description,
    CurrencyCode,
    IsActive,
    ValidFrom,
    ValidTo,
    SalesChannel,
    CustomerGroupCode,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductPriceLists]
WHERE IsDeleted = 0
  AND (@IncludeInactive = 1 OR IsActive = 1)
ORDER BY Name;";

            using var connection = CreateConnection();
            var priceLists = await connection.QueryAsync<ProductPriceListDto>(
                new CommandDefinition(sql, new { IncludeInactive = includeInactive }, cancellationToken: cancellationToken));

            return priceLists.AsList();
        }

        public async Task<IReadOnlyList<LookupItemDto>> GetPriceListLookupsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Name
FROM [Product].[ProductPriceLists]
WHERE IsDeleted = 0
  AND (@IncludeInactive = 1 OR IsActive = 1)
ORDER BY Name;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<LookupItemDto>(
                new CommandDefinition(sql, new { IncludeInactive = includeInactive }, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<ProductPriceListDto?> GetPriceListByIdAsync(Guid priceListId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    Id,
    Code,
    Name,
    Description,
    CurrencyCode,
    IsActive,
    ValidFrom,
    ValidTo,
    SalesChannel,
    CustomerGroupCode,
    CreatedAt,
    UpdatedAt
FROM [Product].[ProductPriceLists]
WHERE Id = @PriceListId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductPriceListDto>(
                new CommandDefinition(sql, new { PriceListId = priceListId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductPriceListDto> CreatePriceListAsync(CreateProductPriceListRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO [Product].[ProductPriceLists]
(
    Id,
    Code,
    Name,
    Description,
    CurrencyCode,
    IsActive,
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
    @Code,
    @Name,
    @Description,
    @CurrencyCode,
    @IsActive,
    @ValidFrom,
    @ValidTo,
    @SalesChannel,
    @CustomerGroupCode,
    @Now,
    0
);";

            var priceListId = await InsertWithGeneratedCodeAsync(
                request.Code,
                PriceListCodeSource,
                async (connection, transaction, code, ct) =>
                {
                    var id = Guid.NewGuid();
                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            sql,
                            new
                            {
                                Id = id,
                                Code = code,
                                request.Name,
                                request.Description,
                                request.CurrencyCode,
                                request.IsActive,
                                request.ValidFrom,
                                request.ValidTo,
                                request.SalesChannel,
                                request.CustomerGroupCode,
                                Now = DateTime.UtcNow
                            },
                            transaction,
                            cancellationToken: ct));
                    return id;
                },
                cancellationToken);

            return await GetPriceListByIdAsync(priceListId, cancellationToken)
                ?? throw new InvalidOperationException("Price list could not be loaded after insert.");
        }

        public async Task<bool> UpdatePriceListAsync(Guid priceListId, UpdateProductPriceListRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPriceLists]
SET
    Code = @Code,
    Name = @Name,
    Description = @Description,
    CurrencyCode = @CurrencyCode,
    IsActive = @IsActive,
    ValidFrom = @ValidFrom,
    ValidTo = @ValidTo,
    SalesChannel = @SalesChannel,
    CustomerGroupCode = @CustomerGroupCode,
    UpdatedAt = @Now
WHERE Id = @PriceListId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        PriceListId = priceListId,
                        request.Code,
                        request.Name,
                        request.Description,
                        request.CurrencyCode,
                        request.IsActive,
                        request.ValidFrom,
                        request.ValidTo,
                        request.SalesChannel,
                        request.CustomerGroupCode,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeletePriceListAsync(Guid priceListId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPriceLists]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @PriceListId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { PriceListId = priceListId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<IReadOnlyList<ProductPriceListItemDto>> GetPriceListItemsAsync(Guid priceListId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    pli.Id,
    pli.ProductPriceListId,
    pl.Code AS PriceListCode,
    pl.Name AS PriceListName,
    pli.ProductId,
    pli.ProductVariantId,
    v.Sku AS VariantSku,
    v.Name AS VariantName,
    pli.Amount,
    pli.CompareAtAmount,
    pli.MinQuantity,
    pli.MaxQuantity,
    pli.CreatedAt,
    pli.UpdatedAt
FROM [Product].[ProductPriceListItems] pli
LEFT JOIN [Product].[ProductPriceLists] pl ON pl.Id = pli.ProductPriceListId AND pl.IsDeleted = 0
LEFT JOIN [Product].[ProductVariants] v ON v.Id = pli.ProductVariantId AND v.IsDeleted = 0
WHERE pli.ProductPriceListId = @PriceListId
  AND pli.IsDeleted = 0
ORDER BY pli.MinQuantity, pli.MaxQuantity, pli.CreatedAt DESC;";

            using var connection = CreateConnection();
            var items = await connection.QueryAsync<ProductPriceListItemDto>(
                new CommandDefinition(sql, new { PriceListId = priceListId }, cancellationToken: cancellationToken));

            return items.AsList();
        }

        public async Task<ProductPriceListItemDto?> GetPriceListItemByIdAsync(Guid priceListItemId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT
    pli.Id,
    pli.ProductPriceListId,
    pl.Code AS PriceListCode,
    pl.Name AS PriceListName,
    pli.ProductId,
    pli.ProductVariantId,
    v.Sku AS VariantSku,
    v.Name AS VariantName,
    pli.Amount,
    pli.CompareAtAmount,
    pli.MinQuantity,
    pli.MaxQuantity,
    pli.CreatedAt,
    pli.UpdatedAt
FROM [Product].[ProductPriceListItems] pli
LEFT JOIN [Product].[ProductPriceLists] pl ON pl.Id = pli.ProductPriceListId AND pl.IsDeleted = 0
LEFT JOIN [Product].[ProductVariants] v ON v.Id = pli.ProductVariantId AND v.IsDeleted = 0
WHERE pli.Id = @PriceListItemId
  AND pli.IsDeleted = 0;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<ProductPriceListItemDto>(
                new CommandDefinition(sql, new { PriceListItemId = priceListItemId }, cancellationToken: cancellationToken));
        }

        public async Task<ProductPriceListItemDto> CreatePriceListItemAsync(CreateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default)
        {
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

            var itemId = Guid.NewGuid();

            using var connection = CreateConnection();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = itemId,
                        request.ProductPriceListId,
                        request.ProductId,
                        request.ProductVariantId,
                        request.Amount,
                        request.CompareAtAmount,
                        request.MinQuantity,
                        request.MaxQuantity,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return await GetPriceListItemByIdAsync(itemId, cancellationToken)
                ?? throw new InvalidOperationException("Price list item could not be loaded after insert.");
        }

        public async Task<bool> UpdatePriceListItemAsync(Guid priceListItemId, UpdateProductPriceListItemRequestDto request, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPriceListItems]
SET
    Amount = @Amount,
    CompareAtAmount = @CompareAtAmount,
    MinQuantity = @MinQuantity,
    MaxQuantity = @MaxQuantity,
    UpdatedAt = @Now
WHERE Id = @PriceListItemId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        PriceListItemId = priceListItemId,
                        request.Amount,
                        request.CompareAtAmount,
                        request.MinQuantity,
                        request.MaxQuantity,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        public async Task<bool> DeletePriceListItemAsync(Guid priceListItemId, CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE [Product].[ProductPriceListItems]
SET
    IsDeleted = 1,
    DeletedAt = @Now,
    UpdatedAt = @Now
WHERE Id = @PriceListItemId
  AND IsDeleted = 0;";

            using var connection = CreateConnection();
            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(sql, new { PriceListItemId = priceListItemId, Now = DateTime.UtcNow }, cancellationToken: cancellationToken));

            return affectedRows > 0;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private static int NormalizeTake(int take)
        {
            if (take <= 0)
            {
                return DefaultTake;
            }

            return Math.Min(take, MaxTake);
        }

    }
}
