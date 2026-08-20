using ProductManagement.Service.Shared.Abstract;

namespace ProductManagement.Service.Abstract
{
    public interface IServiceManager
    {
        IAuthenticationService AuthenticationService { get; }
        ICurrentUserService CurrentUserService { get; }
        IProductOperationsService ProductOperationsService { get; }
        ITokenService TokenService { get; }
    }
}
