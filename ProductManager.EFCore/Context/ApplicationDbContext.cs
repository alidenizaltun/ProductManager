using ProductManager.Domain.Entities;
using ProductManager.Domain.Entities.Product;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.EfCore.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
        IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();
        public DbSet<ProductInventory> ProductInventories => Set<ProductInventory>();
        public DbSet<ProductAttributeDefinition> ProductAttributeDefinitions => Set<ProductAttributeDefinition>();
        public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<ProductCategoryMap> ProductCategoryMaps => Set<ProductCategoryMap>();
        public DbSet<ProductMedia> ProductMediaItems => Set<ProductMedia>();
        public DbSet<ProductBundleItem> ProductBundleItems => Set<ProductBundleItem>();
        public DbSet<ProductPhysicalProfile> ProductPhysicalProfiles => Set<ProductPhysicalProfile>();
        public DbSet<ProductSoftwareProfile> ProductSoftwareProfiles => Set<ProductSoftwareProfile>();
        public DbSet<ProductServiceProfile> ProductServiceProfiles => Set<ProductServiceProfile>();
        public DbSet<ProductSubscriptionProfile> ProductSubscriptionProfiles => Set<ProductSubscriptionProfile>();
        public DbSet<ProductSupplier> ProductSuppliers => Set<ProductSupplier>();
        public DbSet<ProductSupplierMap> ProductSupplierMaps => Set<ProductSupplierMap>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
 public DbSet<ProductPriceList> ProductPriceLists => Set<ProductPriceList>();
 public DbSet<ProductPriceListItem> ProductPriceListItems => Set<ProductPriceListItem>();
 public DbSet<ProductModule> ProductModules => Set<ProductModule>();
 public DbSet<SoftwarePricingTier> SoftwarePricingTiers => Set<SoftwarePricingTier>();
 public DbSet<ProductLicenseOffering> ProductLicenseOfferings => Set<ProductLicenseOffering>();
 public DbSet<UnitDefinition> UnitDefinitions => Set<UnitDefinition>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureIdentityTables();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
