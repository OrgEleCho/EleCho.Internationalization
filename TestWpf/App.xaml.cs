using System.Configuration;
using System.Data;
using System.Windows;
using EleCho.Internationalization.Wpf;

namespace TestWpf
{
    public partial class App : Application
    {
        public static GlobalStrings GlobalStrings { get; } = new();
    }
}
