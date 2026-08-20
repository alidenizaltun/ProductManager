namespace ProductManagement.Service.Shared.Abstract
{
    public interface IStartupSeedService
    {
        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}
