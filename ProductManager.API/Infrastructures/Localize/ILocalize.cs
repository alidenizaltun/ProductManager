using ProductManager.API.Localize;
using Microsoft.Extensions.Localization;

namespace ProductManager.API.Infrastructures.Localize
{
    public interface ILocalize : IStringLocalizer<Resources>
    {
        #pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string this[string name] { get; }
        #pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        #pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string this[string name, params object[] arguments] { get; }
        #pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    }
}
