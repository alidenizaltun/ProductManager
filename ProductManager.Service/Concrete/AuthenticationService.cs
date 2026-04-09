using ProductManager.Domain.Entities;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Service.Shared.Configuration;
using ProductManager.Shared.Dtos.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProductManager.Service.Concrete
{
    public sealed class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Giriş denemesi başarısız: Kullanıcı bulunamadı. Email: {Email}", request.Email);
                return AuthResponseDto.Failure("Geçersiz e-posta veya şifre.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Giriş denemesi başarısız: Hesap pasif. UserId: {UserId}", user.Id);
                return AuthResponseDto.Failure("Hesabınız pasif durumda. Lütfen yönetici ile iletişime geçin.");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                _logger.LogWarning("Giriş denemesi başarısız: Hesap kilitli. UserId: {UserId}, LockoutEnd: {LockoutEnd}", user.Id, lockoutEnd);
                return AuthResponseDto.Failure($"Hesabınız geçici olarak kilitlendi. Lütfen {lockoutEnd?.LocalDateTime:HH:mm} tarihinden sonra tekrar deneyin.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Hesap kilitlendi. UserId: {UserId}", user.Id);
                    return AuthResponseDto.Failure("Çok fazla başarısız giriş denemesi. Hesabınız geçici olarak kilitlendi.");
                }

                if (result.IsNotAllowed)
                {
                    _logger.LogWarning("Giriş izni yok. UserId: {UserId}", user.Id);
                    return AuthResponseDto.Failure("Giriş yapmanıza izin verilmiyor. Lütfen e-posta adresinizi onaylayın.");
                }

                _logger.LogWarning("Giriş denemesi başarısız: Yanlış şifre. Email: {Email}", request.Email);
                return AuthResponseDto.Failure("Geçersiz e-posta veya şifre.");
            }

            // Başarılı giriş - token oluştur
            var roles = await _userManager.GetRolesAsync(user);
            var additionalClaims = new Dictionary<string, string>();

            var tokenResponse = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email!,
                roles,
                additionalClaims,
                request.RememberMe);

            // Refresh token'ı kaydet
            var refreshTokenExpiryDays = request.RememberMe
                ? _jwtSettings.RememberMeRefreshTokenExpirationDays
                : _jwtSettings.RefreshTokenExpirationDays;

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenExpiryDays);

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Kullanıcı giriş yaptı. UserId: {UserId}", user.Id);

            var userDto = MapToUserDto(user, roles);

            return AuthResponseDto.Success(userDto, tokenResponse);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
        {
            // E-posta kontrolü
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return AuthResponseDto.Failure("Bu e-posta adresi zaten kullanılıyor.");
            }

            // Şifre eşleşme kontrolü
            if (request.Password != request.ConfirmPassword)
            {
                return AuthResponseDto.Failure("Şifreler eşleşmiyor.");
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

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Kullanıcı kaydı başarısız. Email: {Email}, Hatalar: {Errors}", request.Email, string.Join(", ", errors));
                return AuthResponseDto.Failure(errors);
            }

            // Varsayılan rol ata
            await EnsureRoleExistsAsync("User");
            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("Yeni kullanıcı kaydedildi. UserId: {UserId}, Email: {Email}", user.Id, user.Email);

            // Otomatik giriş yap ve token oluştur
            var roles = await _userManager.GetRolesAsync(user);
            var additionalClaims = new Dictionary<string, string>();

            var tokenResponse = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email!,
                roles,
                additionalClaims);

            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            await _userManager.UpdateAsync(user);

            var userDto = MapToUserDto(user, roles);

            return AuthResponseDto.Success(userDto, tokenResponse);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            if (principal == null)
            {
                return AuthResponseDto.Failure("Geçersiz access token.");
            }

            var userId = _tokenService.GetUserIdFromToken(request.AccessToken);

            if (!userId.HasValue)
            {
                return AuthResponseDto.Failure("Token'dan kullanıcı bilgisi alınamadı.");
            }

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());

            if (user == null)
            {
                return AuthResponseDto.Failure("Kullanıcı bulunamadı.");
            }

            if (!user.IsActive)
            {
                return AuthResponseDto.Failure("Hesap pasif durumda.");
            }

            if (user.RefreshToken != request.RefreshToken)
            {
                _logger.LogWarning("Refresh token eşleşmiyor. UserId: {UserId}", user.Id);
                return AuthResponseDto.Failure("Geçersiz refresh token.");
            }

            if (user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                _logger.LogWarning("Refresh token süresi dolmuş. UserId: {UserId}", user.Id);
                return AuthResponseDto.Failure("Refresh token süresi dolmuş. Lütfen tekrar giriş yapın.");
            }

            // Yeni token oluştur
            var roles = await _userManager.GetRolesAsync(user);
            var additionalClaims = new Dictionary<string, string>();

            var tokenResponse = _tokenService.GenerateAccessToken(
                user.Id,
                user.Email!,
                roles,
                additionalClaims);

            // Refresh token'ı güncelle
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token yenilendi. UserId: {UserId}", user.Id);

            var userDto = MapToUserDto(user, roles);

            return AuthResponseDto.Success(userDto, tokenResponse);
        }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Kullanıcı çıkış yaptı. UserId: {UserId}", userId);
            }
        }

        public async Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                // Security stamp'ı güncelle - tüm token'ları geçersiz kılar
                await _userManager.UpdateSecurityStampAsync(user);

                _logger.LogInformation("Kullanıcının tüm oturumları sonlandırıldı. UserId: {UserId}", userId);
            }
        }

        public async Task<AuthResponseDto> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return AuthResponseDto.Failure("Yeni şifreler eşleşmiyor.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return AuthResponseDto.Failure("Kullanıcı bulunamadı.");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return AuthResponseDto.Failure(errors);
            }

            // Güvenlik için tüm refresh token'ları geçersiz kıl
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Kullanıcı şifresi değiştirildi. UserId: {UserId}", userId);

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = MapToUserDto(user, roles);

            return new AuthResponseDto
            {
                Succeeded = true,
                User = userDto
            };
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
            {
                // Güvenlik için her zaman true dön
                return true;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"]
                ?? _configuration.GetSection("Cors:Client").Get<string[]>()?.FirstOrDefault(x => x.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                ?? "https://bayiportal.godeva.com.tr";

            var encodedEmail = Uri.EscapeDataString(user.Email!);
            var encodedToken = Uri.EscapeDataString(token);
            var resetPasswordUrl = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?email={encodedEmail}&token={encodedToken}";

            var subject = "ProductManager Portal - Sifre Sifirlama Talebi";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Sifre Sifirlama Talebi</h2>
                    <p>Merhaba {user.FirstName} {user.LastName},</p>
                    <p>Hesabiniz icin bir sifre sifirlama talebi aldik.</p>

                    <p>Sifrenizi sifirlamak icin asagidaki butona tiklayin:</p>
                    <p>
                        <a href='{System.Net.WebUtility.HtmlEncode(resetPasswordUrl)}' style='display: inline-block; padding: 10px 18px; background-color: #007bff; color: #ffffff; text-decoration: none; border-radius: 5px;'>
                            Sifreyi Sifirla
                        </a>
                    </p>
                    <p style='word-break: break-all;'>
                        Buton calismazsa bu linki tarayiciniza yapistirin:<br />
                        <a href='{System.Net.WebUtility.HtmlEncode(resetPasswordUrl)}'>{System.Net.WebUtility.HtmlEncode(resetPasswordUrl)}</a>
                    </p>
                    <p>Eger bu islemi siz yapmadiysaniz bu e-postayi dikkate almayin.</p>

                    <hr style='margin: 24px 0;' />
                    <p style='color: #666; font-size: 12px;'>Bu e-posta otomatik olarak gonderilmistir. Lutfen yanitlamayin.</p>
                </body>
                </html>
            ";

            await _emailService.SendEmailAsync(user.Email!, subject, body);

            _logger.LogInformation("Şifre sıfırlama token'ı oluşturuldu. UserId: {UserId}", user.Id);

            return true;
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return AuthResponseDto.Failure("Şifreler eşleşmiyor.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return AuthResponseDto.Failure("Geçersiz istek.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return AuthResponseDto.Failure(errors);
            }

            // Tüm refresh token'ları geçersiz kıl
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Kullanıcı şifresi sıfırlandı. UserId: {UserId}", user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = MapToUserDto(user, roles);

            return new AuthResponseDto
            {
                Succeeded = true,
                User = userDto
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return MapToUserDto(user, roles);
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                _logger.LogInformation("E-posta onaylandı. UserId: {UserId}", userId);
            }

            return result.Succeeded;
        }

        #region Private Methods

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole(roleName)
                {
                    Description = $"{roleName} rolü",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }
        }

        private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                Roles = roles,
                CreatedAt = user.CreatedAt
            };
        }

        #endregion
    }
}
