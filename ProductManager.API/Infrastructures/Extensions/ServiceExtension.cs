using AspNetCoreRateLimit;
using ProductManager.API.Infrastructures.Helpers;
using ProductManager.API.Infrastructures.Localize;
using ProductManager.Domain.Abstract;
using ProductManager.Domain.Entities;
using ProductManager.EfCore.Context;
using ProductManager.Repository.Concrete;
using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Abstract;
using ProductManager.Service.Concrete;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Service.Shared.Configuration;
using ProductManager.Shared.Abstract;
using ProductManager.Shared.Infrastructure.Extensions;
using ProductManager.Shared.Infrastructure.Helpers;
using Dapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RepoDb;
using Scrutor;
using Swashbuckle.AspNetCore.Filters;
using System.Data;
using System.Globalization;
using System.Text;

namespace ProductManager.API.Infrastructures.Extensions
{
    public static class ServiceExtension
    {
        public static void AddHelperIOC(this IServiceCollection service) { }

        public static void AddLocalizationIOC(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddTransient<ILocalize, LocalizeHelper>();
            service.AddTransient<ILocalization, LocalizationHelper>();

            service.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            var supportedCultures = configuration
                .GetSection("Localization:SupportedCultures")
                .Get<string[]>()
                ?.Select(c => new CultureInfo(c))
                .ToArray() ?? Array.Empty<CultureInfo>();

            service.Configure<RequestLocalizationOptions>(options =>
            {
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(context =>
                {
                    var languages = context.Request.Headers["Accept-Language"].ToString();
                    var currentLanguage = languages.Split(',').FirstOrDefault();
                    var defaultLanguage = string.IsNullOrEmpty(currentLanguage?.Trim()) ? "tr" : currentLanguage;

                    if (!supportedCultures.Any(c => c.Name.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase)))
                    {
                        defaultLanguage = "tr";
                    }

                    return Task.FromResult(new ProviderCultureResult(defaultLanguage, defaultLanguage))!;
                }));
            });
        }

        public static void ConfigureRateLimit(this IServiceCollection service, IHostApplicationBuilder builder)
        {
            service.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));
            builder.Services.AddMemoryCache();
            service.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            service.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            service.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            service.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        }

        public static void ConfigureCors(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddCors(options =>
            {
                string[]? whiteListForClient = builder.Configuration.GetSection("Cors:Client").Get<string[]>();

                if (whiteListForClient?.Any() == true)
                {
                    options.AddDefaultPolicy(
                        policy =>
                        {
                            policy.WithOrigins(whiteListForClient)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                        }
                    );
                }
                else
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    });
                }
            });
        }

        public static void ConfigureSignalR(this IServiceCollection service) { }

        public static void ConfigureAuthentication(this IServiceCollection service, IHostApplicationBuilder builder)
        {
            // JWT ayarlarını yapılandır
            var jwtSettingsSection = builder.Configuration.GetSection(JwtSettings.SectionName);
            service.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>()!;

            service.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),
                    RequireExpirationTime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });

            // Authorization politikaları
            service.AddAuthorizationBuilder()
                .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
                .AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Admin"));

            // Claim tabanlı granüler izin politikaları (Permission:X) dinamik olarak üretilir
            service.AddSingleton<IAuthorizationHandler, ProductManager.Shared.Infrastructure.Security.PermissionAuthorizationHandler>();
            service.AddSingleton<IAuthorizationPolicyProvider, ProductManager.Shared.Infrastructure.Security.PermissionPolicyProvider>();
        }

        public static void AddAuthenticationServices(this IServiceCollection service)
        {
            service.AddScoped<ITokenService, TokenService>();
            service.AddScoped<IAuthenticationService, AuthenticationService>();
            service.AddScoped<ICurrentUserService, CurrentUserService>();
        }

        public static void ConfigureSwagger(this IServiceCollection service, IHostApplicationBuilder builder)
        {
            service.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Standart Auhorization header using the bearer scheme (\"token\")",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer"),
                        new List<string>()
                    }
                });

                #region Swagger Doc

                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "ProductManager Web Api Service - v1",
                        Version = "v1",
                        Description = $"ProductManager uygulamasının arayüz api hizmetidir.",
                        TermsOfService = new Uri("https://devayazilim.com.tr/"),
                        Contact = new OpenApiContact
                        {
                            Name = "Mevlut Can Turaci",
                            Email = "mevlut@sistemiq.com"
                        },
                        License = new OpenApiLicense
                        {
                            Name = $"Deva Software © {DateTime.Now.Year}",
                            Url = new Uri("https://devayazilim.com.tr/")
                        }
                    }
                );

                options.SwaggerDoc("v2",
                    new OpenApiInfo
                    {
                        Title = "ProductManager Web Api Service - v2",
                        Version = "v2",
                        Description = $"ProductManager uygulamasının arayüz api hizmetidir.",
                        TermsOfService = new Uri("https://devayazilim.com.tr/"),
                        Contact = new OpenApiContact
                        {
                            Name = "Ali Deniz Altun",
                            Email = "alidenizaltun66@gmail.com"
                        },
                        License = new OpenApiLicense
                        {
                            Name = $"Deva Software © {DateTime.Now.Year}",
                            Url = new Uri("https://devayazilim.com.tr/")
                        }
                    }
                );

                #endregion

                options.OperationFilter<SecurityRequirementsOperationFilter>();

                // Swagger'a ait attributeleri kullanabilmek için ekledik.
                options.EnableAnnotations();
            });


            // Register in Swagger configuration
            service.AddSwaggerGen(c => { });

        }

        public static void ConfigureDbContext(this IServiceCollection service, IHostApplicationBuilder builder)
        {
            RepoDb.GlobalConfiguration.Setup().UseSqlServer();

            service.AddDbContext<ApplicationDbContext>(op =>
            {
                op.UseSqlServer(builder.Configuration.GetActiveConnectionString());
            }, contextLifetime: ServiceLifetime.Scoped);
        }

        public static void ConfigureIdentity(this IServiceCollection service)
        {
            service.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password ayarları
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequiredLength = 6;

                // Lockout ayarları
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User ayarları
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign-in ayarları
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        }

        public static void AddAllManagerIOC(this IServiceCollection service)
        {
            service.AddScoped(typeof(Lazy<>), typeof(Lazier<>));

            service.AddScoped<IRepositoryManager, RepositoryManager>();
            service.AddScoped<IProductOperationsRepository, ProductOperationsRepository>();
            service.AddScoped<ISystemManagementRepository, SystemManagementRepository>();
            service.AddScoped<IServiceManager, ServiceManager>();

            service.Scan(selector => selector
                .FromAssemblies(
                    typeof(ProductManager.Service.AssemblyRefrence).Assembly
                )
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsMatchingInterface()
                .WithScopedLifetime()
            );
        }

        public static void ConfigureHangfire(this IServiceCollection service, IHostApplicationBuilder builder)
        {
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetActiveConnectionString())
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
            );

            builder.Services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "email", "default", "sms" };

                options.WorkerCount = Environment.ProcessorCount * 2;
            }); ;

            builder.Services.AddScoped<IBackgroundJobClient>(provider => new BackgroundJobClient());
        }
    }
}


