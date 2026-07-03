using ProductManager.Repository.Shared.Abstract;
using ProductManager.Service.Shared.Abstract;
using ProductManager.Shared.Dtos.PriceEngine;
using ProductManager.Shared.Dtos.ProductOperations;
using ProductManager.Shared.Infrastructure.Exceptions;

namespace ProductManager.Service.Concrete.PriceEngine
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
