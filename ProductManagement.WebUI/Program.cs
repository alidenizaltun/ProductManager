using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;
using ProductManagement.EfCore.Context;
using ProductManagement.Repository.Concrete;
using ProductManagement.Repository.Shared.Abstract;
using ProductManagement.Service.Concrete;
using ProductManagement.Service.Shared.Abstract;
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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 0;
        options.Password.RequiredLength = 6;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
    options.Cookie.Name = "ProductManagement.Auth";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
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

await EnsureSuperAdminAsync(app.Services);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static async Task EnsureSuperAdminAsync(IServiceProvider services)
{
    const string superAdminRole = "SuperAdmin";
    const string superAdminEmail = "pm@gmail.com";
    const string superAdminPassword = "123123";

    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync(superAdminRole))
    {
        var roleResult = await roleManager.CreateAsync(new ApplicationRole(superAdminRole)
        {
            Description = "Sistemdeki tek super admin roludur.",
            IsActive = true
        });

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Super admin rolu olusturulamadi: {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
        }
    }

    var user = await userManager.FindByEmailAsync(superAdminEmail);
    if (user is null)
    {
        user = new ApplicationUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            EmailConfirmed = true,
            FirstName = "Super",
            LastName = "Admin",
            IsActive = true
        };

        var createResult = await userManager.CreateAsync(user, superAdminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Super admin hesabi olusturulamadi: {string.Join(", ", createResult.Errors.Select(x => x.Description))}");
        }
    }
    else
    {
        user.IsActive = true;
        user.EmailConfirmed = true;
        user.UserName ??= superAdminEmail;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException($"Super admin hesabi guncellenemedi: {string.Join(", ", updateResult.Errors.Select(x => x.Description))}");
        }

        var hasPassword = await userManager.HasPasswordAsync(user);
        var hasExpectedPassword = hasPassword && await userManager.CheckPasswordAsync(user, superAdminPassword);

        if (!hasExpectedPassword)
        {
            IdentityResult passwordResult;
            if (hasPassword)
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                passwordResult = await userManager.ResetPasswordAsync(user, resetToken, superAdminPassword);
            }
            else
            {
                passwordResult = await userManager.AddPasswordAsync(user, superAdminPassword);
            }

            if (!passwordResult.Succeeded)
            {
                throw new InvalidOperationException($"Super admin sifresi ayarlanamadi: {string.Join(", ", passwordResult.Errors.Select(x => x.Description))}");
            }
        }
    }

    if (!await userManager.IsInRoleAsync(user, superAdminRole))
    {
        var addRoleResult = await userManager.AddToRoleAsync(user, superAdminRole);
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Super admin rol atamasi yapilamadi: {string.Join(", ", addRoleResult.Errors.Select(x => x.Description))}");
        }
    }
}
