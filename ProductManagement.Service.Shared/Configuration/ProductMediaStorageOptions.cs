namespace ProductManagement.Service.Shared.Configuration;

public sealed class ProductMediaStorageOptions
{
    public const string SectionName = "Storage";

    public string? UploadsRoot { get; set; }

    public string RequestPath { get; set; } = "/uploads";

    public long MaxFileSizeBytes { get; set; } = 8 * 1024 * 1024;

    public int MaxFilesPerRequest { get; set; } = 20;
}
