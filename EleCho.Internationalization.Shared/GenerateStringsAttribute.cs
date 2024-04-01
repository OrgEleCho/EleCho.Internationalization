namespace EleCho.Internationalization
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateStringsAttribute : Attribute
    {
        public GenerateStringsAttribute(string languageCode, string resourcePath)
        {
            LanguageCode = languageCode;
            ResourcePath = resourcePath;
        }

        public string LanguageCode { get; }
        public string ResourcePath { get; }
    }
}
