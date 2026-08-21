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
        };

        public static IReadOnlyList<string> AllKeys { get; } = All.Select(p => p.Key).ToList();
    }
}
