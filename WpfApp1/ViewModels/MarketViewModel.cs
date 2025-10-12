using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Models;
using Alex_Mai.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class MarketViewModel : ObservableObject
    {
        private readonly GameViewModel _parentViewModel;
        private readonly List<MarketItem> _allItems;

        [ObservableProperty]
        private ObservableCollection<MarketItem> _displayItems;

        public MarketViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;
            var marketService = new MarketService("Data/market_items.json");
            _allItems = marketService.GetAllItems();
            DisplayItems = new ObservableCollection<MarketItem>(_allItems);
        }

        [RelayCommand]
        private void FilterCategory(string category)
        {
            if (string.IsNullOrEmpty(category) || category == "All")
            {
                DisplayItems = new ObservableCollection<MarketItem>(_allItems);
            }
            else
            {
                DisplayItems = new ObservableCollection<MarketItem>(_allItems.Where(item => item.Category == category));
            }
        }

        [RelayCommand]
        private void BuyItem(MarketItem item)
        {
            if (item == null) return;
            // Alış-veriş məntiqini GameViewModel-ə ötürürük
            _parentViewModel.PurchaseItem(item);
        }

        [RelayCommand]
        private void Close()
        {
            _parentViewModel.ToggleMarket(false);
        }
    }
}
