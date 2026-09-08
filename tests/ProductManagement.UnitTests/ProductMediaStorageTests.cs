using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProductManagement.Service.Concrete;
using ProductManagement.Service.Shared.Configuration;
using ProductManagement.Shared.Infrastructure.Exceptions;

namespace ProductManagement.UnitTests;

public sealed class ProductMediaStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "pm-media-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SaveProductImageAsync_dosyayi_diske_yazar_ve_url_doner()
    {
        var storage = CreateStorage();
        await using var content = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var productId = Guid.NewGuid();

        var stored = await storage.SaveProductImageAsync(productId, content, "kapak.PNG", "image/png");

        Assert.Equal("image/png", stored.ContentType);
        Assert.Equal(4, stored.Size);
        Assert.StartsWith($"/uploads/products/{productId:D}/", stored.Url);
        Assert.True(File.Exists(Path.Combine(_root, "products", productId.ToString("D"), stored.FileName)));
    }

    [Fact]
    public async Task SaveProductImageAsync_desteklenmeyen_uzantiyi_reddedir()
    {
        var storage = CreateStorage();
        await using var content = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        await Assert.ThrowsAsync<BadRequestException>(() =>
            storage.SaveProductImageAsync(Guid.NewGuid(), content, "notlar.txt", "text/plain"));
    }

    private ProductMediaStorage CreateStorage()
    {
        var options = Options.Create(new ProductMediaStorageOptions
        {
            UploadsRoot = _root,
            RequestPath = "/uploads",
            MaxFileSizeBytes = 8 * 1024 * 1024,
            MaxFilesPerRequest = 20
        });

        return new ProductMediaStorage(new TestHostEnvironment(), options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Tests";
        public string ApplicationName { get; set; } = "ProductManagement.UnitTests";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
