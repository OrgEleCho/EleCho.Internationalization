using System.Globalization;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            App app = new()
            {
                AllowFuzzyMatching = true,
                AllowFallback = true,
            };

            app.CurrentCulture = new CultureInfo("zh-CN");

            Console.WriteLine(app.StringHello);
        }
    }
}
