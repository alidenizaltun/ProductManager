using ProductManager.API.Infrastructures.Middlewares;
using ProductManager.Shared.Infrastructure.Exceptions;
using ProductManager.API.Models;
using Microsoft.AspNetCore.Diagnostics;
using Hangfire;
using System.Text.Json;

namespace ProductManager.API.Infrastructures.Extensions
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

                        var errorModel = new SystemModel.ErrorDetails
                        {
                            StatusCode = statusCode,
                            ErrorCode = errorCode,
                        };

                        errorModel.Message = GetErrorMessage(contextFeature.Error);
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
                DashboardTitle = "ProductManager API - Recurring Jobs"
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
