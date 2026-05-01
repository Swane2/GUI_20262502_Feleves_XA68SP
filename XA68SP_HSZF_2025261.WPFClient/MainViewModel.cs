using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class MainViewModel
    {
        // Service az adatok kezelésére,
        // és a termékek listája, amit a felületen látunk
        private readonly IProductService productService;

        public ObservableCollection<Product> Products { get; set; }
        // Ezek kezelik a gombokat (hozzáadás, törlés, szerkesztés, részletek)
        public IRelayCommand AddProductCommand { get; }
        public IRelayCommand DeleteProductCommand { get; }
        public IRelayCommand ShowDetailsCommand { get; }
        public IRelayCommand EditProductCommand { get; }

        // adatok betöltése, gombok működésének beállítása
        public MainViewModel(IProductService productService)
        {
            this.productService = productService;

            Products = new ObservableCollection<Product>(productService.GetAllProducts());

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
            Products.Clear();
            foreach (var p in productService.GetAllProducts())
                Products.Add(p);
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
