using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.WPFClient
{
    public partial class ProductDetailsWindow : Window
    {
        // Termék adatainak megjelenítése
        public ProductDetailsWindow(Product product)
        {
            InitializeComponent();
            DataContext = product;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
