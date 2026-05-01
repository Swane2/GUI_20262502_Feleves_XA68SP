using Microsoft.Extensions.DependencyInjection;
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

namespace XA68SP_HSZF_2025261.WPFClient
{
    public partial class MainWindow : Window
    {
        // Itt kapcsoljuk össze az ablakot a MainViewModel-lel.
        public MainWindow()
        {
            InitializeComponent();
            // A ViewModel lekérése és beállítása
            DataContext = App.ServiceProvider.GetService<MainViewModel>();
        }
    }
}