using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProductManagement.Domain.Entities;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.Identity;
using ProductManagement.Shared.Infrastructure.Exceptions;
using System.Security.Cryptography;

namespace ProductManagement.Service.Concrete
{
    public sealed class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync(string? search, bool includeInactive, CancellationToken cancellationToken = default)
        {
            var query = _userManager.Users.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    u.Email!.Contains(term) ||
                    (u.FirstName != null && u.FirstName.Contains(term)) ||
                    (u.LastName != null && u.LastName.Contains(term)));
            }

            var users = await query.OrderBy(u => u.Email).ToListAsync(cancellationToken);

            // Kullanıcı başına GetRolesAsync çağırmak yerine (N+1), rol sayısı kullanıcı
            // sayısından çok daha küçük olduğundan rolleri gezip ters bir harita kurulur.
            var userRoles = new Dictionary<Guid, List<string>>();
            var roleNames = await _roleManager.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);
            foreach (var roleName in roleNames)
            {
                foreach (var user in await _userManager.GetUsersInRoleAsync(roleName))
                {
                    if (!userRoles.TryGetValue(user.Id, out var list))
                        userRoles[user.Id] = list = [];

                    list.Add(roleName);
                }
            }

            return users
                .Select(user => MapToDto(user, userRoles.TryGetValue(user.Id, out var roles) ? roles : []))
                .ToList();
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        public async Task<AdminUserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
            {
                throw new ConflictException("Bu e-posta adresi zaten kullanılıyor.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.Now,
                EmailConfirmed = false
            };

            var temporaryPassword = GenerateTemporaryPassword();
            var createResult = await _userManager.CreateAsync(user, temporaryPassword);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description);
                throw new BadRequestException("Kullanıcı oluşturulamadı.", new Dictionary<string, string[]> { ["general"] = errors.ToArray() });
            }

            var rolesToAssign = await ResolveValidRolesAsync(request.Roles);
            if (rolesToAssign.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, rolesToAssign);
            }

            await SendInvitationEmailAsync(user);

            _logger.LogInformation("Admin tarafından yeni kullanıcı oluşturuldu. UserId: {UserId}", user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.Now;

            if (!request.IsActive)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                throw new BadRequestException("Kullanıcı güncellenemedi.", new Dictionary<string, string[]> { ["general"] = errors.ToArray() });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var targetRoles = await ResolveValidRolesAsync(request.Roles);

            var rolesToRemove = currentRoles.Except(targetRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var rolesToAdd = targetRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (rolesToRemove.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            _logger.LogInformation("Kullanıcı güncellendi. UserId: {UserId}", user.Id);

            return true;
        }

        public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.Now;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Kullanıcı pasifleştirildi. UserId: {UserId}", user.Id);

            return true;
        }

        public async Task<bool> ResendInvitationAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return false;
            }

            await SendInvitationEmailAsync(user);

            return true;
        }

        #region Private Methods

        private async Task<List<string>> ResolveValidRolesAsync(IEnumerable<string> requestedRoles)
        {
            var result = new List<string>();
            foreach (var roleName in requestedRoles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    result.Add(roleName);
                }
            }

            return result;
        }

        private async Task SendInvitationEmailAsync(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"]
                ?? _configuration.GetSection("Cors:Client").Get<string[]>()?.FirstOrDefault(x => x.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                ?? "https://bayiportal.godeva.com.tr";

            var encodedEmail = Uri.EscapeDataString(user.Email!);
            var encodedToken = Uri.EscapeDataString(token);
            var setPasswordUrl = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?email={encodedEmail}&token={encodedToken}";

            var subject = "ProductManagement Portal - Hesabınız Oluşturuldu";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Hoş Geldiniz!</h2>
                    <p>Merhaba {user.FirstName} {user.LastName},</p>
                    <p>ProductManagement Portal hesabınız oluşturuldu. Şifrenizi belirlemek için aşağıdaki butona tıklayın:</p>
                    <p>
                        <a href='{System.Net.WebUtility.HtmlEncode(setPasswordUrl)}' style='display: inline-block; padding: 10px 18px; background-color: #007bff; color: #ffffff; text-decoration: none; border-radius: 5px;'>
                            Şifremi Belirle
                        </a>
                    </p>
                    <p style='word-break: break-all;'>
                        Buton çalışmazsa bu linki tarayıcınıza yapıştırın:<br />
                        <a href='{System.Net.WebUtility.HtmlEncode(setPasswordUrl)}'>{System.Net.WebUtility.HtmlEncode(setPasswordUrl)}</a>
                    </p>
                    <hr style='margin: 24px 0;' />
                    <p style='color: #666; font-size: 12px;'>Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayın.</p>
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(16);
            var password = new char[16];
            for (var i = 0; i < password.Length; i++)
            {
                password[i] = chars[bytes[i] % chars.Length];
            }

            return new string(password) + "!1";
        }

        private static AdminUserDto MapToDto(ApplicationUser user, IList<string> roles)
        {
            return new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                Roles = roles,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        #endregion
    }
}
