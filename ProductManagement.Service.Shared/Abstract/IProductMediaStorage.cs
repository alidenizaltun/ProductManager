namespace ProductManagement.Service.Shared.Abstract;

public sealed record StoredProductMediaFile(
    string Url,
    string FileName,
    string ContentType,
    long Size);

public interface IProductMediaStorage
{
    string RootPath { get; }
    string RequestPath { get; }

    Task<StoredProductMediaFile> SaveProductImageAsync(
        Guid productId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
