using ProductManagement.API.Infrastructures.Middlewares;
using ProductManagement.Shared.Infrastructure.Exceptions;
using ProductManagement.API.Models;
using Microsoft.AspNetCore.Diagnostics;
using Hangfire;
using System.Text.Json;

namespace ProductManagement.API.Infrastructures.Extensions
{
    public static class ApplicationExtension
    {
        public static void GlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    context.AddHeaderTool();

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        int statusCode = GetStatusCode(contextFeature.Error);

                        string errorCode = string.Concat("GEM_", Guid.NewGuid().ToString("N").AsSpan(0, 8));

                        // Öncesinde bu handler hatayı hiç loglamıyordu: 500'ler istemciye rastgele
                        // bir kodla dönüp sunucu tarafında hiçbir iz bırakmadan kayboluyordu.
                        // ErrorCode ile korelasyon kurulabilsin diye logluyoruz.
                        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("GlobalExceptionHandler");
                        logger.LogError(contextFeature.Error,
                            "İşlenmeyen hata [{ErrorCode}] {Method} {Path} -> {StatusCode}",
                            errorCode, context.Request.Method, context.Request.Path, statusCode);

                        // Konsol logu Plesk/IIS tarafında yakalanmıyor olabilir (stdout log
                        // ayarına bağlı); dosyaya da yazarak garanti altına alıyoruz - Program.cs'deki
                        // en dış catch bloğunun kullandığı aynı "Logs/" kalıbı.
                        try
                        {
                            var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
                            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

                            var logFile = Path.Combine(logDir, $"errors_{DateTime.UtcNow:yyyy-MM-dd}.log");
                            var logLine = $"{DateTimeOffset.UtcNow:O} [{errorCode}] {context.Request.Method} {context.Request.Path} -> {statusCode}{Environment.NewLine}{contextFeature.Error}{Environment.NewLine}{new string('-', 80)}{Environment.NewLine}";
                            await File.AppendAllTextAsync(logFile, logLine);
                        }
                        catch
                        {
                            // Dosyaya yazamıyorsak (izin/disk sorunu) sessizce geç - yanıtı engellememeli.
                        }

                        var errorModel = new SystemModel.ErrorDetails
                        {
                            StatusCode = statusCode,
                            ErrorCode = errorCode,
                        };

                        // Geçici tanı kapısı: dosya/loga erişim olmadan canlıda gerçek hatayı görmek
                        // için. DIAGNOSTICS_DEBUG_KEY ortam değişkeni tanımlıysa ve istek aynı değeri
                        // X-Debug-Key header'ında taşıyorsa tam exception döner. Ortam değişkeni
                        // tanımlı değilse bu yol tamamen kapalıdır - kaynak koduna yazılmıyor,
                        // appsettings'e commit'lenmiyor. Sorun teşhis edildikten sonra Plesk'ten
                        // ortam değişkeni silinmeli.
                        var debugKey = Environment.GetEnvironmentVariable("DIAGNOSTICS_DEBUG_KEY");
                        bool isDebugRequest = !string.IsNullOrEmpty(debugKey) &&
                            context.Request.Headers["X-Debug-Key"] == debugKey;

                        errorModel.Message = isDebugRequest
                            ? contextFeature.Error.ToString()
                            : GetErrorMessage(contextFeature.Error);
                        errorModel.AdditionalData = GetAdditionalData(contextFeature.Error);

                        context.Response.StatusCode = statusCode;
                        await context.Response.WriteAsync(errorModel.ToString());
                    }
                });
            });
        }

        public static void AppMiddleare(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.AddHeaderTool();
                await next();
            });
        }

        private static void AddHeaderTool(this HttpContext context)
        {
            var header = context.Response.Headers;

            header.TryAdd("X-Content-Type-Options", "nosniff");
            header.TryAdd("X-Robots-Tag", "noindex, nofollow");
            header.TryAdd("Referrer-Policy", "no-referrer");

            header.TryAdd(
                "Permissions-Policy",
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()"
            );

            header.TryAdd("X-Frame-Options", "SAMEORIGIN");
            header.TryAdd("Cross-Origin-Opener-Policy", "same-origin");
            header.TryAdd("Cross-Origin-Resource-Policy", "same-site");
            header.Remove("X-Powered-By");
            header.Remove("X-AspNet-Version");
            header.Remove("X-AspNetMvc-Version");
            header.Remove("Server");
        }

        public static void ConfigureSignalRHubs(this IApplicationBuilder app) { }

        public static void ConfigureCustomMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<JwtTokenMiddleware>();
        }

        public static void ConfigureLocalization(this IApplicationBuilder app)
        {
            app.UseRequestLocalization();
        }

        public static void ConfigureHangfire(this IApplicationBuilder app)
        {
            app.UseHangfireDashboard(pathMatch: "/deva/recurring-jobs", options: new DashboardOptions
            {
                DashboardTitle = "ProductManagement API - Recurring Jobs"
            });
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                BaseException b => b.StatusCode,
                JsonException => StatusCodes.Status400BadRequest,
                BadHttpRequestException => StatusCodes.Status400BadRequest,
                InvalidDataException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                BaseException b => b.Message,
                JsonException => "Gönderilen JSON formatı geçersiz. Lütfen alan tiplerini ve sayı formatlarını kontrol edin.",
                BadHttpRequestException => "İstek gövdesi okunamadı. Lütfen gönderilen verinin formatını kontrol edin.",
                InvalidDataException => "Gönderilen veri geçerli değil. Lütfen formatı kontrol edin.",
                _ =>
#if DEBUG
                    exception.Message
#else
                    "Beklenmedik bir sistem hatası oluştu."
#endif
            };
        }

        private static object? GetAdditionalData(Exception exception)
        {
            return exception switch
            {
                BaseException b => b.AdditionalData,
                JsonException jsonException => new Dictionary<string, string[]>
                {
                    [NormalizeJsonPath(jsonException.Path)] =
                    [
                        "Bu alanın değeri beklenen veri tipine uygun değil. Sayısal alanlarda nokta kullanın; örn: 18.5."
                    ]
                },
                _ => null
            };
        }

        private static string NormalizeJsonPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "$")
            {
                return "request";
            }

            return path.StartsWith("$.", StringComparison.Ordinal)
                ? path[2..]
                : path.TrimStart('$', '.');
        }
    }
}
