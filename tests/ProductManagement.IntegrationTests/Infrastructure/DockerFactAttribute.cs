namespace ProductManagement.IntegrationTests.Infrastructure;

/// <summary>
/// Docker çalışıyorsa koşan, çalışmıyorsa <b>atlanan</b> test.
///
/// Ortam eksikliği ile gerçek başarısızlığı ayırmak için: Docker kapalıyken
/// <c>dotnet test</c> kırmızı vermemeli, yalnızca veri testlerinin atlandığını bildirmeli.
/// </summary>
public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!DockerEnvironment.IsAvailable)
        {
            Skip = "Docker çalışmıyor — veri testleri atlandı. Docker Desktop'ı başlatıp tekrar koşun.";
        }
    }
}

/// <summary>Docker daemon'ının erişilebilir olup olmadığını ucuza kontrol eder.</summary>
public static class DockerEnvironment
{
    private static readonly Lazy<bool> Available = new(Probe);

    public static bool IsAvailable => Available.Value;

    private static bool Probe()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Named pipe'lar \\.\pipe\ altında dosya olarak listelenir.
                return Directory.GetFiles(@"\\.\pipe\")
                    .Any(p => p.Contains("docker", StringComparison.OrdinalIgnoreCase));
            }

            return File.Exists("/var/run/docker.sock");
        }
        catch
        {
            return false;
        }
    }
}
