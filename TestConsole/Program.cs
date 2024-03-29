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

            app.PropertyChanged += App_PropertyChanged;
            app.CurrentCulture = new CultureInfo("zh-CN");

            Console.WriteLine(app.StringHello);

        }

        private static void App_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(App.CurrentCulture))
                return;

            Console.WriteLine($"CultureChanged");
        }
    }
}
