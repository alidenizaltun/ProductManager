using ProductManagement.Shared.Dtos.PriceEngine;

namespace ProductManagement.Service.Shared.Abstract
{
    public interface IPriceEngineService
    {
        Task<LicenseOfferingPricingParametersDto?> GetLicenseOfferingPricingParametersAsync(
            Guid productId,
            Guid licenseOfferingId,
            CancellationToken cancellationToken = default);
    }
}
