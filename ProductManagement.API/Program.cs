using ProductManagement.API.Infrastructures.Extensions;
using Deva.Extensions.G2way.Infrastructures.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using DateTimeConverter = ProductManagement.Shared.Infrastructures.Converters.DateTimeConverter;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.UseKestrel(c =>
    {
        c.AddServerHeader = false;
    });

    builder.Services.ConfigureHangfire(builder);

    builder.Services.AddControllers(o =>
    {
        o.ConfigureCacheProfiles(builder);
        o.Filters.Add(new ResponseCacheAttribute { CacheProfileName = "NoCaches" });
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddApplicationPart(typeof(ProductManagement.Presentation.AssemblyReference).Assembly);
    builder.Services.ConfigureUserFriendlyValidationErrors();

    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(1);
    });

    builder.Services.AddHttpLogging(logging =>
    {
        logging.LoggingFields = HttpLoggingFields.All;
        logging.RequestBodyLogLimit = Int32.MaxValue;
        logging.ResponseBodyLogLimit = Int32.MaxValue;

        logging.RequestHeaders.Add("Authorization");
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddOpenApi();
    builder.Services.ConfigureDbContext(builder);
    builder.Services.ConfigureIdentity();
    builder.Services.ConfigureAuthentication(builder);
    builder.Services.ConfigureSwagger(builder);
    builder.Services.AddAuthenticationServices();
    builder.Services.AddMapster();

    TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

    builder.Services.AddFluentValidationAutoValidation()
        .AddValidatorsFromAssembly(typeof(Program).Assembly)
        .AddValidatorsFromAssembly(typeof(ProductManagement.Presentation.AssemblyReference).Assembly);
    builder.Services.AddHelperIOC();
    builder.Services.AddLocalizationIOC(builder.Configuration);
    builder.Services.AddAllManagerIOC();
    builder.Services.ConfigureRateLimit(builder);
    builder.Services.ConfigureCors(builder);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();

    // builder.Services.ConfigureFileSerilog(builder);
    builder.Services.AddDevaGateway(builder.Configuration);

    var app = builder.Build();

    // app.ApplyMigrations();
    // app.ConfigreFirebase();

    // Migration'lar otomatik uygulanmadığı için (bkz. app.ApplyMigrations() yorum satırı),
    // yeni tablolar henüz DB'de olmayabilir; seed hatası tüm uygulamanın ayağa kalkmasını
    // engellememeli, sadece loglanmalı.
    using (var seedScope = app.Services.CreateScope())
    {
        try
        {
            var seedService = seedScope.ServiceProvider.GetRequiredService<ProductManagement.Service.Shared.Abstract.IStartupSeedService>();
            await seedService.SeedAsync();
        }
        catch (Exception seedEx)
        {
            var seedLogger = seedScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            seedLogger.LogWarning(seedEx, "Başlangıç seed işlemi atlandı (muhtemelen migration henüz uygulanmadı).");
        }
    }

    app.ConfigureHangfire();
    app.ConfigureLocalization();
    app.UseHttpLogging();
    app.UseRouting();
    app.UseCors();
    app.UseResponseCaching();
    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseHsts();
    app.AppMiddleare();
    app.GlobalExceptionHandler();
    app.ConfigureCustomMiddlewares();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapOpenApi();

    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/openapi/v1.json");
        options.WithTitle("Product Management API Documentation");
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("ERROR WEB API!! -> " + ex);

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => "Guess what happened? Yes, you know :) ");

    app.Run();

    var directory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

    var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"error_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.log");
    using FileStream fs = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
    using StreamWriter sw = new StreamWriter(fs);
    sw.WriteLine($"Hata: " + ex);
}
