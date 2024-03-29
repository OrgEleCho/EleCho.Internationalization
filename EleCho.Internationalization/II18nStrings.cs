using System.Globalization;

namespace EleCho.Internationalization
{
    public interface II18nStrings
    {
        public CultureInfo CurrentCulture { get; set; }
        public IReadOnlyList<CultureInfo> AllCultures { get; }

        public string GetString(string name);
        public IReadOnlyList<string> AllStringNames { get; }
    }
}
