using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        private readonly IFileSaverService _fileSaverService;

        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];


        public ObservableCollection<Product> FilteredProducts { get; set; } = [];
        //properties voor gefilterde producten in de zoekbalk.

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        [ObservableProperty]
        string myMessage;

        // property voor de zoekterm in de zoekbalk
        [ObservableProperty]
        string searchText = string.Empty;

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService, IFileSaverService fileSaverService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            _fileSaverService = fileSaverService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id))
                MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();
            FilteredProducts.Clear();

            foreach (Product p in _productService.GetAll())
            {
                if (MyGroceryListItems.FirstOrDefault(g => g.ProductId == p.Id) == null && p.Stock > 0)
                {
                    AvailableProducts.Add(p);
                    FilteredProducts.Add(p);
                }
            }
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        // de implementatie van SearchCommand en wordt gebonden aan de functie Search().
        [RelayCommand]
        public void Search(string searchTerm)
        {
            SearchText = searchTerm ?? string.Empty;

            FilteredProducts.Clear();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {

                foreach (var product in AvailableProducts)
                {
                    FilteredProducts.Add(product);
                }//als er geen zoekterm wordt ingevoerd, worden alle beschikbare producten getoond.
            }
            else
            {
                //de producten worden gefilterd op basis van de zoekterm.
                //de zoekterm wordt gebruikt als parameter (hoofdletter gevoelig).
                var filteredList = AvailableProducts
                    .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()))
                    .ToList();

                foreach (var product in filteredList)
                {
                    FilteredProducts.Add(product);
                }
            }
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }

        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null) return;

            GroceryListItem item = new(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(item);
            product.Stock--;
            _productService.Update(product);


            AvailableProducts.Remove(product);
            FilteredProducts.Remove(product);

            OnGroceryListChanged(GroceryList);
        }

        [RelayCommand]
        public async Task ShareGroceryList(CancellationToken cancellationToken)
        {
            if (GroceryList == null || MyGroceryListItems == null) return;

            string jsonString = JsonSerializer.Serialize(MyGroceryListItems);
            try
            {
                await _fileSaverService.SaveFileAsync("Boodschappen.json", jsonString, cancellationToken);
                await Toast.Make("Boodschappenlijst is opgeslagen.").Show(cancellationToken);
            }
            catch (Exception ex)
            {
                await Toast.Make($"Opslaan mislukt: {ex.Message}").Show(cancellationToken);
            }
        }
    }
}