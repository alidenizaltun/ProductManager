using ProductManagement.Repository.Shared.Abstract;
using ProductManagement.Service.Shared.Abstract;
using ProductManagement.Shared.Dtos.PriceEngine;
using ProductManagement.Shared.Dtos.ProductOperations;
using ProductManagement.Shared.Infrastructure.Exceptions;

namespace ProductManagement.Service.Concrete.PriceEngine
{
    public sealed class PriceEngineService : IPriceEngineService
    {
        private readonly IProductOperationsRepository _repository;

        public PriceEngineService(IProductOperationsRepository repository)
        {
            _repository = repository;
        }

        public async Task<LicenseOfferingPricingParametersDto?> GetLicenseOfferingPricingParametersAsync(
            Guid productId,
            Guid licenseOfferingId,
            CancellationToken cancellationToken = default)
        {
            var product = await _repository.GetProductDetailByIdAsync(productId, cancellationToken)
                ?? throw new NotFoundException("Product", productId);

            return PriceEngineCalculator.BuildPricingParameters(product, licenseOfferingId);
        }
    }
}
