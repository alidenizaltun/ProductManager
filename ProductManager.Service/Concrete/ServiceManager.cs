using ProductManager.Service.Abstract;
using ProductManager.Service.Shared.Abstract;

namespace ProductManager.Service.Concrete
{
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authenticationService;
        private readonly Lazy<ICurrentUserService> _currentUserService;
        private readonly Lazy<IProductOperationsService> _productOperationsService;
        private readonly Lazy<ITokenService> _tokenService;

        public ServiceManager(
            Lazy<IAuthenticationService> authenticationService, 
            Lazy<ICurrentUserService> currentUserService, 
            Lazy<IProductOperationsService> productOperationsService,
            Lazy<ITokenService> tokenService)
        {
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _productOperationsService = productOperationsService;
            _tokenService = tokenService;
        }

        public IAuthenticationService AuthenticationService => _authenticationService.Value;
        public ICurrentUserService CurrentUserService => _currentUserService.Value;
        public IProductOperationsService ProductOperationsService => _productOperationsService.Value;
        public ITokenService TokenService => _tokenService.Value;
    }
}
