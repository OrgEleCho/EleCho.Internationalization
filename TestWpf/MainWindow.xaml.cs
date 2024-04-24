using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;

namespace TestWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = this;
            GlobalStrings.CurrentCulture = 
                GlobalStrings.AllCultures.FirstOrDefault() ?? CultureInfo.CurrentCulture;

            InitializeComponent();

            GlobalStrings.PropertyChanged += GlobalStrings_PropertyChanged;
            GlobalStrings.GetString("QWQ");
        }

        private void GlobalStrings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(GlobalStrings.StringHello))
            {
                return;
            }

            logs.Text += $"{e.PropertyName} Changed, New value: {GlobalStrings.StringHello}{Environment.NewLine}";
        }

        public GlobalStrings GlobalStrings { get; } = App.GlobalStrings;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.GlobalStrings.CurrentCulture = new CultureInfo("zh-CN");
            tb.Text = GlobalStrings.StringHello;
        }
    }
}