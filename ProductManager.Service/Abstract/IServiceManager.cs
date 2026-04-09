using ProductManager.Service.Shared.Abstract;

namespace ProductManager.Service.Abstract
{
    public interface IServiceManager
    {
        IAuthenticationService AuthenticationService { get; }
        ICurrentUserService CurrentUserService { get; }
        ITokenService TokenService { get; }
    }
}
