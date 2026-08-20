using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductManagement.Domain.Entities;
using ProductManagement.Repository.Shared.Abstract;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Infrastructure.Security;
using System.Security.Claims;
using System.Text.Json;

namespace ProductManagement.Service.Concrete
{
    /// <summary>
    /// Uygulama başlangıcında bir kez çalışan idempotent seed işlemleri:
    /// Admin rolüne tüm izin claim'lerini verir ve appsettings.json'daki
    /// Mailjet e-posta ayarını (varsa) Entegrasyonlar tablosuna görüntüleme
    /// amaçlı bir kayıt olarak taşır.
    /// </summary>
    public sealed class StartupSeedService : IStartupSeedService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ISystemManagementRepository _repository;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StartupSeedService> _logger;

        public StartupSeedService(
            RoleManager<ApplicationRole> roleManager,
            ISystemManagementRepository repository,
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration configuration,
            ILogger<StartupSeedService> logger)
        {
            _roleManager = roleManager;
            _repository = repository;
            _dataProtectionProvider = dataProtectionProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            foreach (var roleName in Permissions.BypassRoles)
            {
                await SeedBypassRolePermissionsAsync(roleName);
            }

            await SeedMailjetIntegrationAsync(cancellationToken);
        }

        private async Task SeedBypassRolePermissionsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole(roleName)
                {
                    Description = "Sistem yöneticisi",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return;
            }

            var existingPermissions = (await _roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var permission in Permissions.AllKeys)
            {
                if (!existingPermissions.Contains(permission))
                {
                    await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                }
            }
        }

        private async Task SeedMailjetIntegrationAsync(CancellationToken cancellationToken)
        {
            if (await _repository.IntegrationExistsAsync(cancellationToken))
            {
                return;
            }

            var mailjetAdapter = _configuration.GetSection("DevaGateway:Adapters")
                .GetChildren()
                .FirstOrDefault(a => a["Adapter"] == "Mailjet");

            if (mailjetAdapter == null)
            {
                return;
            }

            var credentials = new Dictionary<string, string>();
            var publicKey = mailjetAdapter["PublicKey"];
            var secretKey = mailjetAdapter["SecretKey"];

            if (!string.IsNullOrEmpty(publicKey))
            {
                credentials["PublicKey"] = publicKey;
            }

            if (!string.IsNullOrEmpty(secretKey))
            {
                credentials["SecretKey"] = secretKey;
            }

            if (credentials.Count == 0)
            {
                return;
            }

            var protector = _dataProtectionProvider.CreateProtector(DataProtectionPurposes.IntegrationsCredentials);
            var credentialsProtected = protector.Protect(JsonSerializer.Serialize(credentials));

            await _repository.CreateIntegrationAsync(
                name: "Mailjet (E-posta)",
                type: "Email",
                providerKey: "Mailjet",
                isEnabled: true,
                configJson: null,
                credentialsProtected: credentialsProtected,
                isSystemManaged: true,
                description: "Sistem e-posta gönderimi için kullanılan Mailjet entegrasyonu. Canlı yapılandırması appsettings.json üzerinden yönetilir; buradaki kayıt görüntüleme amaçlıdır.",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Mailjet entegrasyon kaydı seed edildi.");
        }
    }
}
