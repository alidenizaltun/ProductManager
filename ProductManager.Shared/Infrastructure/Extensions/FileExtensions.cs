using System.Security.Cryptography;
using System.Text;

namespace ProductManager.Shared.Infrastructure.Extensions;

public static class FileExtensions
{
    public static string GenerateFileHash(this string content)
    {
        using var sha256 = SHA256.Create();
        byte[] contentBytes = Encoding.UTF8.GetBytes(content);
        byte[] hashBytes = sha256.ComputeHash(contentBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
