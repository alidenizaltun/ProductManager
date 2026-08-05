namespace ProductManager.Shared.Infrastructure.Security
{
    public static class Permissions
    {
        public const string ClaimType = "permission";

        /// <summary>
        /// Bu rollerdeki kullanıcılar tüm izin kontrollerini otomatik geçer.
        /// "SuperAdmin", ProductManager.WebUI'nin kök hesabı (pm@gmail.com) için
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
        };

        public static IReadOnlyList<string> AllKeys { get; } = All.Select(p => p.Key).ToList();
    }
}
