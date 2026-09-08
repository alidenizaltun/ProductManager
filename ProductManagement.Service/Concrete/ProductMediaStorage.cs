using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Service.Shared.Configuration;

namespace ProductManagement.Service.Concrete;

public sealed class ProductMediaStorage : IProductMediaStorage
{
    private readonly ProductMediaStorageOptions _options;
    private readonly string _rootPath;

    public ProductMediaStorage(IHostEnvironment environment, IOptions<ProductMediaStorageOptions> options)
    {
        _options = options.Value;
        var configuredRoot = _options.UploadsRoot;
        _rootPath = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.GetFullPath(Path.Combine(environment.ContentRootPath, "App_Data", "uploads"))
            : Path.GetFullPath(configuredRoot);

        Directory.CreateDirectory(_rootPath);
    }

    public string RootPath => _rootPath;

    public string RequestPath
    {
        get
        {
            var path = string.IsNullOrWhiteSpace(_options.RequestPath) ? "/uploads" : _options.RequestPath.Trim();
            return path.StartsWith('/') ? path.TrimEnd('/') : $"/{path.TrimEnd('/')}";
        }
    }

    public async Task<StoredProductMediaFile> SaveProductImageAsync(
        Guid productId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (content.CanSeek)
        {
            ProductMediaUploadRules.EnsureImage(originalFileName, contentType, content.Length, _options.MaxFileSizeBytes);
        }
        else
        {
            ProductMediaUploadRules.EnsureImage(originalFileName, contentType, 1, _options.MaxFileSizeBytes);
        }

        var storedName = ProductMediaUploadRules.CreateStoredFileName(originalFileName);
        var relativeFolder = Path.Combine("products", productId.ToString("D"));
        var physicalFolder = Path.Combine(_rootPath, relativeFolder);
        Directory.CreateDirectory(physicalFolder);

        var physicalPath = Path.Combine(physicalFolder, storedName);
        long storedLength;
        try
        {
            await using (var fileStream = File.Create(physicalPath))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            storedLength = new FileInfo(physicalPath).Length;
            ProductMediaUploadRules.EnsureImage(originalFileName, contentType, storedLength, _options.MaxFileSizeBytes);
        }
        catch
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            throw;
        }

        var url = $"{RequestPath}/products/{productId:D}/{storedName}".Replace('\\', '/');
        return new StoredProductMediaFile(
            url,
            storedName,
            ProductMediaUploadRules.GuessContentType(originalFileName, contentType),
            storedLength);
    }
}
