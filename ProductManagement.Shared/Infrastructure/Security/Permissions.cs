namespace ProductManagement.Shared.Infrastructure.Security
{
    public static class Permissions
    {
        public const string ClaimType = "permission";

        /// <summary>
        /// Bu rollerdeki kullanıcılar tüm izin kontrollerini otomatik geçer.
        /// "SuperAdmin", ProductManagement.WebUI'nin kök hesabı (pm@gmail.com) için
        /// kullanılan, "Admin"den ayrı önceden var olan bir roldür.
        /// </summary>
        public static readonly string[] BypassRoles = ["Admin", "SuperAdmin"];

        public static class Users
        {
            public const string View = "Users.View";
            public const string Manage = "Users.Manage";
        }

        public static class Roles
        {
            public const string View = "Roles.View";
            public const string Manage = "Roles.Manage";
        }

        public static class Settings
        {
            public const string View = "Settings.View";
            public const string Manage = "Settings.Manage";
        }

        public static class Integrations
        {
            public const string View = "Integrations.View";
            public const string Manage = "Integrations.Manage";
        }

        public static class PricingTemplates
        {
            public const string View = "Pricing.Templates.View";
            public const string Manage = "Pricing.Templates.Manage";
        }

        public static class PriceRevisions
        {
            public const string View = "Pricing.Revisions.View";

            /// <summary>Revizyon oluşturma, kapsam düzenleme, önizleme ve onaya gönderme.</summary>
            public const string Manage = "Pricing.Revisions.Manage";

            /// <summary>Onaylama/reddetme. Hazırlayan ile onaylayan çoğu kurumda aynı kişi değildir.</summary>
            public const string Approve = "Pricing.Revisions.Approve";

            /// <summary>Onaylı revizyonu uygulama ve geri alma.</summary>
            public const string Apply = "Pricing.Revisions.Apply";
        }

        /// <summary>
        /// Ürün kartı ve ona bağlı yapılar: varyant, birim, profil, medya, ilişkiler,
        /// lisans teklifleri ve modüller.
        /// </summary>
        public static class Products
        {
            public const string View = "Products.View";
            public const string Manage = "Products.Manage";
        }

        /// <summary>Ürün fiyatları ve fiyatlandırma kuralları.</summary>
        public static class Prices
        {
            public const string View = "Pricing.Prices.View";
            public const string Manage = "Pricing.Prices.Manage";
        }

        /// <summary>Fiyat listeleri ve kalemleri.</summary>
        public static class PriceLists
        {
            public const string View = "Pricing.PriceLists.View";
            public const string Manage = "Pricing.PriceLists.Manage";
        }

        /// <summary>Kategori, tedarikçi, depo, öznitelik, birim tanımı ve bölge.</summary>
        public static class Catalog
        {
            public const string View = "Catalog.View";
            public const string Manage = "Catalog.Manage";
        }

        /// <summary>Stok kayıtları, rezervasyonlar ve hareketler.</summary>
        public static class Inventory
        {
            public const string View = "Inventory.View";
            public const string Manage = "Inventory.Manage";
        }

        public static IReadOnlyList<PermissionDefinition> All { get; } = new List<PermissionDefinition>
        {
            new(Users.View, "Kullanıcıları Görüntüle", "Kullanıcılar"),
            new(Users.Manage, "Kullanıcıları Yönet", "Kullanıcılar"),
            new(Roles.View, "Rolleri Görüntüle", "Roller ve Yetkiler"),
            new(Roles.Manage, "Rolleri Yönet", "Roller ve Yetkiler"),
            new(Settings.View, "Sistem Ayarlarını Görüntüle", "Sistem Ayarları"),
            new(Settings.Manage, "Sistem Ayarlarını Yönet", "Sistem Ayarları"),
            new(Integrations.View, "Entegrasyonları Görüntüle", "Entegrasyonlar"),
            new(Integrations.Manage, "Entegrasyonları Yönet", "Entegrasyonlar"),
            new(PricingTemplates.View, "Fiyat Şablonlarını Görüntüle", "Fiyatlandırma"),
            new(PricingTemplates.Manage, "Fiyat Şablonlarını Yönet", "Fiyatlandırma"),
            new(PriceRevisions.View, "Zam Revizyonlarını Görüntüle", "Fiyatlandırma"),
            new(PriceRevisions.Manage, "Zam Revizyonu Hazırla", "Fiyatlandırma"),
            new(PriceRevisions.Approve, "Zam Revizyonunu Onayla", "Fiyatlandırma"),
            new(PriceRevisions.Apply, "Zam Revizyonunu Uygula", "Fiyatlandırma"),
            new(Products.View, "Ürünleri Görüntüle", "Ürünler"),
            new(Products.Manage, "Ürünleri Yönet", "Ürünler"),
            new(Prices.View, "Ürün Fiyatlarını Görüntüle", "Fiyatlandırma"),
            new(Prices.Manage, "Ürün Fiyatlarını Yönet", "Fiyatlandırma"),
            new(PriceLists.View, "Fiyat Listelerini Görüntüle", "Fiyatlandırma"),
            new(PriceLists.Manage, "Fiyat Listelerini Yönet", "Fiyatlandırma"),
            new(Catalog.View, "Katalog Tanımlarını Görüntüle", "Katalog"),
            new(Catalog.Manage, "Katalog Tanımlarını Yönet", "Katalog"),
            new(Inventory.View, "Stok Bilgilerini Görüntüle", "Stok"),
            new(Inventory.Manage, "Stok Hareketlerini Yönet", "Stok"),
        };

        public static IReadOnlyList<string> AllKeys { get; } = All.Select(p => p.Key).ToList();
    }
}
