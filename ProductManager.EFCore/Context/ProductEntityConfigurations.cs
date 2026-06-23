using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManager.Domain.Entities.Product;

namespace ProductManager.EfCore.Context
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasIndex(p => p.ProductCode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_Products_ProductCode");

            builder.Property(p => p.ProductCode)
                .HasMaxLength(64);

            builder.Property(p => p.Name)
                .HasMaxLength(250);

            builder.Property(p => p.DefaultCurrencyCode)
                .HasMaxLength(3);

            builder.HasMany(p => p.BundleItems)
                .WithOne(b => b.BundleProduct)
                .HasForeignKey(b => b.BundleProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PhysicalProfile)
                .WithOne(pp => pp.Product)
                .HasForeignKey<ProductPhysicalProfile>(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.SoftwareProfile)
                .WithOne(sp => sp.Product)
                .HasForeignKey<ProductSoftwareProfile>(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.ServiceProfile)
                .WithOne(sp => sp.Product)
                .HasForeignKey<ProductServiceProfile>(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.SubscriptionProfile)
                .WithOne(sp => sp.Product)
                .HasForeignKey<ProductSubscriptionProfile>(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.UnitDefinition)
                .WithMany()
                .HasForeignKey(p => p.UnitDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ProductBundleItemConfiguration : IEntityTypeConfiguration<ProductBundleItem>
    {
        public void Configure(EntityTypeBuilder<ProductBundleItem> builder)
        {
            builder.HasOne(b => b.ChildProduct)
                .WithMany()
                .HasForeignKey(b => b.ChildProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ChildVariant)
                .WithMany()
                .HasForeignKey(b => b.ChildVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ProductSupplierConfiguration : IEntityTypeConfiguration<ProductSupplier>
    {
        public void Configure(EntityTypeBuilder<ProductSupplier> builder)
        {
            builder.HasIndex(s => s.SupplierCode)
                .IsUnique();

            builder.Property(s => s.SupplierCode)
                .HasMaxLength(64);

            builder.Property(s => s.Name)
                .HasMaxLength(250);
        }
    }

    public class ProductSupplierMapConfiguration : IEntityTypeConfiguration<ProductSupplierMap>
    {
        public void Configure(EntityTypeBuilder<ProductSupplierMap> builder)
        {
            builder.HasOne(m => m.Product)
                .WithMany(p => p.SupplierMaps)
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.ProductSupplier)
                .WithMany(s => s.ProductMaps)
                .HasForeignKey(m => m.ProductSupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => new { m.ProductId, m.ProductSupplierId })
                .IsUnique();
        }
    }

    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.HasIndex(w => w.Code)
                .IsUnique();

            builder.Property(w => w.Code)
                .HasMaxLength(32);

            builder.Property(w => w.Name)
                .HasMaxLength(150);
        }
    }

    public class ProductInventoryConfiguration : IEntityTypeConfiguration<ProductInventory>
    {
        public void Configure(EntityTypeBuilder<ProductInventory> builder)
        {
            builder.HasOne(i => i.Warehouse)
                .WithMany(w => w.Inventories)
                .HasForeignKey(i => i.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ProductPricingRuleConfiguration : IEntityTypeConfiguration<ProductPricingRule>
    {
        public void Configure(EntityTypeBuilder<ProductPricingRule> builder)
        {
            builder.HasOne(r => r.Product)
            .WithMany(p => p.PricingRules)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.ProductVariant)
            .WithMany()
            .HasForeignKey(r => r.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ProductLicenseOffering)
            .WithMany()
            .HasForeignKey(r => r.ProductLicenseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.ProductId, r.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_ProductPricingRules_ProductId_Code");

            builder.HasIndex(r => new { r.ProductId, r.IsActive, r.Priority })
            .HasDatabaseName("IX_ProductPricingRules_Product_Active_Priority");

            builder.Property(r => r.Code)
            .HasMaxLength(64);

            builder.Property(r => r.Name)
            .HasMaxLength(200);

            builder.Property(r => r.SalesChannel)
            .HasMaxLength(64);

            builder.Property(r => r.CustomerGroupCode)
            .HasMaxLength(64);
        }
    }

    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.HasOne(t => t.Product)
                .WithMany(p => p.InventoryTransactions)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.ProductVariant)
                .WithMany(v => v.InventoryTransactions)
                .HasForeignKey(t => t.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Warehouse)
                .WithMany(w => w.InventoryTransactions)
                .HasForeignKey(t => t.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
    {
        public void Configure(EntityTypeBuilder<InventoryReservation> builder)
        {
            builder.HasOne(r => r.Product)
                .WithMany(p => p.InventoryReservations)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.ProductVariant)
                .WithMany(v => v.InventoryReservations)
                .HasForeignKey(r => r.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Warehouse)
                .WithMany(w => w.InventoryReservations)
                .HasForeignKey(r => r.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ProductPriceListConfiguration : IEntityTypeConfiguration<ProductPriceList>
    {
        public void Configure(EntityTypeBuilder<ProductPriceList> builder)
        {
            builder.HasIndex(p => p.Code)
                .IsUnique();

            builder.Property(p => p.Code)
                .HasMaxLength(64);

            builder.Property(p => p.Name)
                .HasMaxLength(200);

            builder.Property(p => p.CurrencyCode)
                .HasMaxLength(3);
        }
    }

    public class ProductPriceListItemConfiguration : IEntityTypeConfiguration<ProductPriceListItem>
    {
        public void Configure(EntityTypeBuilder<ProductPriceListItem> builder)
        {
            builder.HasOne(i => i.ProductPriceList)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.ProductPriceListId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Product)
            .WithMany(p => p.PriceListItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.ProductVariant)
            .WithMany(v => v.PriceListItems)
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => new { i.ProductPriceListId, i.ProductId, i.ProductVariantId, i.MinQuantity, i.MaxQuantity })
            .HasDatabaseName("IX_ProductPriceListItems_UniqueRange");
        }
    }

    public class ProductModuleConfiguration : IEntityTypeConfiguration<ProductModule>
    {
        public void Configure(EntityTypeBuilder<ProductModule> builder)
        {
            builder.HasOne(m => m.Product)
            .WithMany(p => p.Modules)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => new { m.ProductId, m.ModuleCode })
            .IsUnique()
            .HasDatabaseName("IX_ProductModules_ProductId_ModuleCode");

            builder.Property(m => m.ModuleCode)
            .HasMaxLength(64);

            builder.Property(m => m.Name)
            .HasMaxLength(200);
        }
    }

    public class ProductModuleOfferingPriceConfiguration : IEntityTypeConfiguration<ProductModuleOfferingPrice>
    {
        public void Configure(EntityTypeBuilder<ProductModuleOfferingPrice> builder)
        {
            builder.HasOne(p => p.ProductModule)
            .WithMany(m => m.OfferingPrices)
            .HasForeignKey(p => p.ProductModuleId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.ProductLicenseOffering)
            .WithMany()
            .HasForeignKey(p => p.ProductLicenseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => new { p.ProductModuleId, p.ProductLicenseOfferingId })
            .IsUnique()
            .HasDatabaseName("IX_ProductModuleOfferingPrices_Module_Offering");

            builder.Property(p => p.Price)
            .HasPrecision(18, 4);

            builder.Property(p => p.CurrencyCode)
            .HasMaxLength(3);
        }
    }

    public class SoftwarePricingTierConfiguration : IEntityTypeConfiguration<SoftwarePricingTier>
    {
        public void Configure(EntityTypeBuilder<SoftwarePricingTier> builder)
        {
            builder.HasOne(t => t.Product)
            .WithMany(p => p.SoftwarePricingTiers)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.ProductLicenseOffering)
            .WithMany(o => o.PricingTiers)
            .HasForeignKey(t => t.ProductLicenseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.UnitDefinition)
            .WithMany()
            .HasForeignKey(t => t.UnitDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => new { t.ProductId, t.ProductLicenseOfferingId, t.UnitDefinitionId, t.MinUnits })
            .HasDatabaseName("IX_SoftwarePricingTiers_Product_Offering_Unit_Min");

            builder.Property(t => t.PricePerUnit)
            .HasPrecision(18, 4);

            builder.Property(t => t.FlatFee)
            .HasPrecision(18, 4);

            builder.Property(t => t.CurrencyCode)
            .HasMaxLength(3);
        }
    }

    public class UnitDefinitionConfiguration : IEntityTypeConfiguration<UnitDefinition>
    {
        public void Configure(EntityTypeBuilder<UnitDefinition> builder)
        {
            builder.HasIndex(u => u.Code)
            .IsUnique()
            .HasDatabaseName("IX_UnitDefinitions_Code");

            builder.Property(u => u.Code)
            .HasMaxLength(32);

            builder.Property(u => u.Name)
            .HasMaxLength(100);
        }
    }

    public class ProductUnitConversionConfiguration : IEntityTypeConfiguration<ProductUnitConversion>
    {
        public void Configure(EntityTypeBuilder<ProductUnitConversion> builder)
        {
            builder.HasOne(c => c.Product)
            .WithMany(p => p.UnitConversions)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.FromUnitDefinition)
            .WithMany()
            .HasForeignKey(c => c.FromUnitDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.ToUnitDefinition)
            .WithMany()
            .HasForeignKey(c => c.ToUnitDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

            // Aynı ürün için aynı From→To birim çifti tekrar edemez
            builder.HasIndex(c => new { c.ProductId, c.FromUnitDefinitionId, c.ToUnitDefinitionId })
            .IsUnique()
            .HasDatabaseName("IX_ProductUnitConversions_Product_From_To");

            builder.Property(c => c.ConversionFactor)
            .HasPrecision(18, 6);
        }
    }

    public class ProductLicenseOfferingConfiguration : IEntityTypeConfiguration<ProductLicenseOffering>
    {
        public void Configure(EntityTypeBuilder<ProductLicenseOffering> builder)
        {
            builder.HasOne(o => o.Product)
            .WithMany(p => p.LicenseOfferings)
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            // Trial'dan dönüşüm hedefini self-referencing olarak tanımla
            builder.HasOne(o => o.ConvertToOffering)
            .WithMany()
            .HasForeignKey(o => o.ConvertToOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.Name)
            .HasMaxLength(200);

            builder.Property(o => o.BasePrice)
            .HasPrecision(18, 4);

            builder.Property(o => o.CurrencyCode)
            .HasMaxLength(3);

            // Bir ürünün aynı LicenseModel tipinde birden fazla offering'i olabilir (farklı süreler).
            // Benzersizliği Name üzerinden sağlıyoruz.
            builder.HasIndex(o => new { o.ProductId, o.Name })
            .IsUnique()
            .HasDatabaseName("IX_ProductLicenseOfferings_ProductId_Name");
        }
    }
}
