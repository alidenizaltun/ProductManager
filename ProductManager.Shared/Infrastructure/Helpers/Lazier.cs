using Microsoft.Extensions.DependencyInjection;

namespace ProductManager.Shared.Infrastructure.Helpers
{
    public class Lazier<T> : Lazy<T> where T : class
    {
        public Lazier(IServiceProvider provider)
        : base(() => provider.GetRequiredService<T>())
        { }

        public static implicit operator T(Lazier<T> lazy) => lazy.Value;
    }
}
