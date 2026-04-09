using ProductManager.Service.Abstract;
using ProductManager.Service.Shared.Abstract;

namespace ProductManager.Service.Concrete
{
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authenticationService;
        private readonly Lazy<ICurrentUserService> _currentUserService;
        private readonly Lazy<ITokenService> _tokenService;

        public ServiceManager(
            Lazy<IAuthenticationService> authenticationService, 
            Lazy<ICurrentUserService> currentUserService, 
            Lazy<ITokenService> tokenService)
        {
            _authenticationService = authenticationService;
            _currentUserService = currentUserService;
            _tokenService = tokenService;
        }

        public IAuthenticationService AuthenticationService => _authenticationService.Value;
        public ICurrentUserService CurrentUserService => _currentUserService.Value;
        public ITokenService TokenService => _tokenService.Value;
    }
}
