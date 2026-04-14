using Microsoft.AspNetCore.ResponseCompression;
using ProductManager.Repository.Concrete;
using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Concrete;
using ProductManager.Service.Shared.Abstract;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

var cacheableAssetExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".css",
    ".js",
    ".png",
    ".jpg",
    ".jpeg",
    ".gif",
    ".svg",
    ".webp",
    ".ico",
    ".woff",
    ".woff2",
    ".ttf",
    ".eot",
    ".map"
};

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.AddScoped<IProductOperationsRepository, ProductOperationsRepository>();
builder.Services.AddScoped<IProductOperationsService, ProductOperationsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.HasStarted ||
        context.Response.StatusCode != StatusCodes.Status200OK ||
        (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method)) ||
        context.Response.Headers.ContainsKey("Cache-Control"))
    {
        return;
    }

    var extension = Path.GetExtension(context.Request.Path.Value);
    if (string.IsNullOrWhiteSpace(extension) || !cacheableAssetExtensions.Contains(extension))
    {
        return;
    }

    context.Response.Headers.CacheControl = "public,max-age=604800";
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
