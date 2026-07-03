using ProductManager.Shared.Dtos.PriceEngine;

namespace ProductManager.Service.Shared.Abstract
{
    public interface IPriceEngineService
    {
        Task<LicenseOfferingPricingParametersDto?> GetLicenseOfferingPricingParametersAsync(
            Guid productId,
            Guid licenseOfferingId,
            CancellationToken cancellationToken = default);
    }
}
