using ProductManagement.Shared.Infrastructure.Exceptions;

namespace ProductManagement.Service.Concrete;

internal static class ProductMediaUploadRules
{
    internal static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    };

    internal static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    internal static void EnsureImage(string? fileName, string? contentType, long length, long maxFileSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BadRequestException("Dosya adı boş olamaz.");
        }

        if (length <= 0)
        {
            throw new BadRequestException($"{fileName} boş bir dosya.");
        }

        if (length > maxFileSizeBytes)
        {
            var maxMb = Math.Max(1, maxFileSizeBytes / (1024 * 1024));
            throw new BadRequestException($"{fileName} dosya boyutu {maxMb} MB sınırını aşıyor.");
        }

        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException($"{fileName} için yalnızca jpg, jpeg, png, gif ve webp görselleri yüklenebilir.");
        }

        if (!string.IsNullOrWhiteSpace(contentType) && !AllowedContentTypes.Contains(contentType))
        {
            throw new BadRequestException($"{fileName} geçersiz bir görsel türü ({contentType}).");
        }
    }

    internal static string CreateStoredFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (extension == ".jpeg")
        {
            extension = ".jpg";
        }

        return $"{Guid.NewGuid():N}{extension}";
    }

    internal static string GuessContentType(string fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && AllowedContentTypes.Contains(contentType))
        {
            return contentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)
                ? "image/jpeg"
                : contentType;
        }

        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    internal static string BuildAltText(string originalFileName)
    {
        var name = Path.GetFileNameWithoutExtension(originalFileName);
        return string.IsNullOrWhiteSpace(name) ? "Ürün görseli" : name.Trim();
    }
}
