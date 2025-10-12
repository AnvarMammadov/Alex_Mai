using Alex_Mai.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace Alex_Mai.ViewModels
{
    public partial class InventoryViewModel : ObservableObject
    {
        // Əsas pəncərənin ViewModel-ına referans
        private readonly GameViewModel _parentViewModel;

        public ObservableCollection<InventoryItem> Items { get; set; }

        public InventoryViewModel(GameViewModel parent)
        {
            _parentViewModel = parent; // Referansı yadda saxlayırıq
            Items = new ObservableCollection<InventoryItem>();
            LoadInitialItems();
        }

        private void LoadInitialItems()
        {
            Items.Add(new InventoryItem { ItemId = "cigarette", Name = "Cigarettes", IconPath = "/Assets/Icons/icon_cigarettes.jpeg" });
            Items.Add(new InventoryItem { ItemId = "zippo", Name = "Zippo", IconPath = "/Assets/Icons/icon_zippo.jpeg" });
            Items.Add(new InventoryItem { ItemId = "phone", Name = "SmartPhone", IconPath = "/Assets/Icons/icon_phone.jpeg" });
        }

        [RelayCommand]
        private void UseItem(InventoryItem item)
        {
            if (item == null) return;

            if (item.ItemId == "phone")
            {
                _parentViewModel.TogglePhoneView();
            }
            else
            {
                // Köhnə məntiq
                _parentViewModel.UseInventoryItem(item.ItemId);
            }
        }

        public void AddItem(InventoryItem newItem)
        {
            var existingItem = Items.FirstOrDefault(i => i.ItemId == newItem.ItemId);

            if (existingItem != null)
            {
                // Əgər əşya artıq varsa, sadəcə sayını artır
                existingItem.Quantity++;
            }
            else
            {
                // Əgər yoxdursa, siyahıya yeni əşya kimi əlavə et
                newItem.Quantity = 1;
                Items.Add(newItem);
            }
        }

        // BU YENİ METODU KLASSA ƏLAVƏ EDİN:
        public int GetItemCount(string itemId)
        {
            var existingItem = Items.FirstOrDefault(i => i.ItemId == itemId);
            return existingItem?.Quantity ?? 0;
        }

        // İnventarı bağlamaq üçün komanda
        [RelayCommand]
        private void Close()
        {
            _parentViewModel.ToggleInventory();
        }
    }
}