using System.Reflection;

namespace ProductManagement.UnitTests;

/// <summary>
/// Faz 3 çıkış kapısı: katman kuralları yazılı ve testle zorlanan bir hal alır.
/// Yeni bir paket bağımlılığı eklemeden (ArchUnitNET yerine), assembly referanslarını
/// reflection ile denetleyen basit kurallar.
/// </summary>
public class ArchitectureTests
{
    private const string DomainAssemblyName = "ProductManagement.Domain";
    private const string EfCoreAssemblyName = "ProductManagement.EFCore";
    private const string RepositoryAssemblyName = "ProductManagement.Repository";
    private const string PresentationAssemblyName = "ProductManagement.API.Presentation";
    private const string ServiceAssemblyName = "ProductManagement.Service";

    [Fact]
    public void Domain_katmani_EFCore_e_referans_vermemeli()
    {
        var domain = Assembly.Load(DomainAssemblyName);

        var referencesEfCore = domain.GetReferencedAssemblies()
            .Any(a => a.Name == EfCoreAssemblyName || a.Name!.StartsWith("Microsoft.EntityFrameworkCore"));

        Assert.False(referencesEfCore,
            $"{DomainAssemblyName}, EF Core'a referans vermemeli — veri erişimi Domain'e sızmış olabilir.");
    }

    [Fact]
    public void Presentation_katmani_EFCore_e_dogrudan_referans_vermemeli()
    {
        var presentation = Assembly.Load(PresentationAssemblyName);

        var referencesEfCore = presentation.GetReferencedAssemblies()
            .Any(a => a.Name == EfCoreAssemblyName);

        Assert.False(referencesEfCore,
            $"{PresentationAssemblyName} (controller'lar), DbContext'e doğrudan erişmemeli — iş mantığı ve veri erişimi Service katmanında kalmalı.");
    }

    [Fact]
    public void Presentation_katmani_Repository_e_dogrudan_referans_vermemeli()
    {
        var presentation = Assembly.Load(PresentationAssemblyName);

        var referencesRepository = presentation.GetReferencedAssemblies()
            .Any(a => a.Name == RepositoryAssemblyName);

        Assert.False(referencesRepository,
            $"{PresentationAssemblyName} (controller'lar), repository katmanını atlayıp doğrudan çağırmamalı — Service üzerinden erişmeli.");
    }

    [Fact]
    public void Service_katmani_EFCore_e_dogrudan_referans_vermemeli()
    {
        var service = Assembly.Load(ServiceAssemblyName);

        var referencesEfCore = service.GetReferencedAssemblies()
            .Any(a => a.Name == EfCoreAssemblyName);

        Assert.False(referencesEfCore,
            $"{ServiceAssemblyName}, DbContext'e doğrudan referans vermemeli — veri erişimi Repository katmanı (ya da ASP.NET Identity UserManager/RoleManager) üzerinden olmalı.");
    }
}
