using ProductManager.API.Infrastructures.Localize;
using ProductManager.Shared.Abstract;

namespace ProductManager.API.Infrastructures.Helpers
{
    public class LocalizationHelper : ILocalization
    {
        private readonly ILocalize _l;

        public LocalizationHelper(ILocalize l)
        {
            _l = l;
        }

        public string GetValue(string key)
        {
            return _l[key];
        }
    }
}
