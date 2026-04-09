using ProductManager.API.Infrastructures.Middlewares;
using ProductManager.Shared.Infrastructure.Exceptions;
using ProductManager.API.Models;
using Microsoft.AspNetCore.Diagnostics;
using Hangfire;

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
                        int statusCode = contextFeature.Error switch
                        {
                            BaseException b => b.StatusCode,
                            _ => 500
                        };

                        string errorCode = string.Concat("GEM_", Guid.NewGuid().ToString("N").AsSpan(0, 8));

                        var errorModel = new SystemModel.ErrorDetails
                        {
                            StatusCode = statusCode,
                            ErrorCode = errorCode,
                        };

                        errorModel.Message = contextFeature.Error switch
                        {
                            BaseException b => b.Message,
                            _ =>
                            #if DEBUG
                                contextFeature.Error.Message
                            #else
                                "Beklenmedik bir sistem hatası oluştu."
                            #endif
                        };

                        errorModel.AdditionalData = contextFeature.Error switch
                        {
                            BaseException b => b.AdditionalData,
                            _ => null
                        };

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
    }
}
