using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using XA68SP_HSZF_2025261.Application;
using XA68SP_HSZF_2025261.Models;

namespace XA68SP_HSZF_2025261.WPFClient
{
    // Ez kezeli a fő ablak működését.
    // Itt vannak a termékek és a gombokhoz tartozó műveletek.
    public class MainViewModel : INotifyPropertyChanged
    {
        // Service az adatok kezelésére,
        // és a termékek listája, amit a felületen látunk
        private readonly IProductService productService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Product> AllProducts { get; set; }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                Refresh();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public int ProductCount => Products.Count;
        public int TotalStock => Products.Sum(p => p.Stock);
        public decimal AveragePrice => Products.Count == 0 ? 0 : Products.Average(p => p.Price);
        // Ezek kezelik a gombokat (hozzáadás, törlés, szerkesztés, részletek)
        public IRelayCommand AddProductCommand { get; }
        public IRelayCommand DeleteProductCommand { get; }
        public IRelayCommand ShowDetailsCommand { get; }
        public IRelayCommand EditProductCommand { get; }

        // adatok betöltése, gombok működésének beállítása
        public MainViewModel(IProductService productService)
        {
            this.productService = productService;

            AllProducts = new ObservableCollection<Product>(productService.GetAllProducts());
            Products = new ObservableCollection<Product>(AllProducts);

            AddProductCommand = new RelayCommand(OpenAddWindow);
            DeleteProductCommand = new RelayCommand<object>(
                    execute: param => { if (param is Product p) DeleteProduct(p); },
                    canExecute: param => param is Product);
            ShowDetailsCommand = new RelayCommand<object>(
                    execute: param => { if (param is Product p) OpenDetailsWindow(p); },
                    canExecute: param => param is Product);
            EditProductCommand = new RelayCommand<object>(
                    execute: param => { if (param is Product p) OpenEditWindow(p); },
                    canExecute: param => param is Product);
        }

        // Új termék ablak megnyitása
        private void OpenAddWindow()
        {
            var window = App.ServiceProvider.GetService<AddProductWindow>();
            window.ShowDialog();

            Refresh();
        }
        // Termék törlése
        private void DeleteProduct(Product product)
        {
            productService.DeleteProduct(product.Id);
            Refresh();
        }
        // Lista frissítése
        private void Refresh()
        {
            AllProducts.Clear();

            foreach (var p in productService.GetAllProducts())
                AllProducts.Add(p);

            Products.Clear();

            var filteredProducts = AllProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredProducts = filteredProducts.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var p in filteredProducts)
                Products.Add(p);
            OnPropertyChanged(nameof(ProductCount));
            OnPropertyChanged(nameof(TotalStock));
            OnPropertyChanged(nameof(AveragePrice));
        }
        // Részletek megjelenítése
        private void OpenDetailsWindow(Product product)
        {
            var window = new ProductDetailsWindow(product);
            window.ShowDialog();
        }
        // Szerkesztő ablak megnyitása
        private void OpenEditWindow(Product product)
        {
            var window = new EditProductWindow(product);
            window.ShowDialog();
            Refresh();
        }
    }
}
