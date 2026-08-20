using ProductManagement.Shared.Dtos.ProductOperations;

namespace ProductManagement.Service.Concrete
{
    public sealed partial class ProductOperationsService
    {
        // ─── ProductModule ────────────────────────────────────────────────────────────

        public Task<IReadOnlyList<ProductModuleDto>> GetProductModulesAsync(Guid productId, CancellationToken cancellationToken = default)
        => _repository.GetProductModulesAsync(productId, cancellationToken);

        public Task<ProductModuleDto?> GetProductModuleByIdAsync(Guid moduleId, CancellationToken cancellationToken = default)
        => _repository.GetProductModuleByIdAsync(moduleId, cancellationToken);

        public Task<ProductModuleDto> CreateProductModuleAsync(CreateProductModuleRequestDto request, CancellationToken cancellationToken = default)
        => _repository.CreateProductModuleAsync(request, cancellationToken);

        public Task<bool> UpdateProductModuleAsync(Guid moduleId, UpdateProductModuleRequestDto request, CancellationToken cancellationToken = default)
        => _repository.UpdateProductModuleAsync(moduleId, request, cancellationToken);

        public Task<bool> DeleteProductModuleAsync(Guid moduleId, CancellationToken cancellationToken = default)
        => _repository.DeleteProductModuleAsync(moduleId, cancellationToken);

        // ─── ProductModuleOfferingPrice ───────────────────────────────────────────────

        public Task<IReadOnlyList<ProductModuleOfferingPriceDto>> GetModuleOfferingPricesAsync(Guid moduleId, CancellationToken cancellationToken = default)
        => _repository.GetModuleOfferingPricesAsync(moduleId, cancellationToken);

        public Task<ProductModuleOfferingPriceDto?> GetModuleOfferingPriceByIdAsync(Guid priceId, CancellationToken cancellationToken = default)
        => _repository.GetModuleOfferingPriceByIdAsync(priceId, cancellationToken);

        public Task<ProductModuleOfferingPriceDto> CreateModuleOfferingPriceAsync(CreateProductModuleOfferingPriceRequestDto request, CancellationToken cancellationToken = default)
        => _repository.CreateModuleOfferingPriceAsync(request, cancellationToken);

        public Task<bool> UpdateModuleOfferingPriceAsync(Guid priceId, UpdateProductModuleOfferingPriceRequestDto request, CancellationToken cancellationToken = default)
        => _repository.UpdateModuleOfferingPriceAsync(priceId, request, cancellationToken);

        public Task<bool> DeleteModuleOfferingPriceAsync(Guid priceId, CancellationToken cancellationToken = default)
        => _repository.DeleteModuleOfferingPriceAsync(priceId, cancellationToken);

        // ─── ProductLicenseOffering ───────────────────────────────────────────────────

        public Task<IReadOnlyList<ProductLicenseOfferingDto>> GetProductLicenseOfferingsAsync(Guid productId, CancellationToken cancellationToken = default)
        => _repository.GetProductLicenseOfferingsAsync(productId, cancellationToken);

        public Task<ProductLicenseOfferingDto?> GetProductLicenseOfferingByIdAsync(Guid offeringId, CancellationToken cancellationToken = default)
        => _repository.GetProductLicenseOfferingByIdAsync(offeringId, cancellationToken);

        public Task<ProductLicenseOfferingDto> CreateProductLicenseOfferingAsync(CreateProductLicenseOfferingRequestDto request, CancellationToken cancellationToken = default)
        => _repository.CreateProductLicenseOfferingAsync(request, cancellationToken);

        public Task<bool> UpdateProductLicenseOfferingAsync(Guid offeringId, UpdateProductLicenseOfferingRequestDto request, CancellationToken cancellationToken = default)
        => _repository.UpdateProductLicenseOfferingAsync(offeringId, request, cancellationToken);

        public Task<bool> DeleteProductLicenseOfferingAsync(Guid offeringId, CancellationToken cancellationToken = default)
        => _repository.DeleteProductLicenseOfferingAsync(offeringId, cancellationToken);
    }
}
