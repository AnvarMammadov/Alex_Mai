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
    public partial class GalleryViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        private readonly GalleryService _galleryService;

        // Display only unlocked items in the grid
        [ObservableProperty]
        private ObservableCollection<GalleryItem> _unlockedItems = new ObservableCollection<GalleryItem>();

        // Property to hold the path of the image currently being viewed full screen
        [ObservableProperty]
        private string _selectedFullImagePath = null;

        // Property to control the visibility of the full image viewer
        [ObservableProperty]
        private bool _isFullImageViewVisible = false;

        public GalleryViewModel(PhoneViewModel parent)
        {
            _parentViewModel = parent;
            _galleryService = new GalleryService(); // Create the service instance

            // TODO Later: Load actual unlocked status from save data before filtering
            // Example: var unlockedIds = GetUnlockedIdsFromSaveData();
            // _galleryService.ApplyUnlockedStatus(unlockedIds);

            LoadUnlockedItems();
        }

        private void LoadUnlockedItems()
        {
            UnlockedItems.Clear();
            var allItems = _galleryService.GetAllGalleryItems();

            // TEMPORARY: Unlock the first item for testing
            if (allItems.Count > 0)
            {
                // _galleryService.UnlockItem(allItems[0].Id); // Use service method if available
                allItems[0].IsUnlocked = true; // Directly set for now
            }


            foreach (var item in allItems.Where(item => item.IsUnlocked))
            {
                UnlockedItems.Add(item);
            }

            // If no items are unlocked, maybe show a message?
            if (!UnlockedItems.Any())
            {
                // Consider adding a property like IsEmpty to show a message in the View
                Console.WriteLine("Gallery is empty."); // Debug message
            }
        }

        [RelayCommand]
        private void ShowFullImage(GalleryItem item)
        {
            if (item != null && item.IsUnlocked)
            {
                SelectedFullImagePath = item.FullImagePath;
                IsFullImageViewVisible = true;
            }
        }

        [RelayCommand]
        private void CloseFullImage()
        {
            IsFullImageViewVisible = false;
            SelectedFullImagePath = null;
        }

        [RelayCommand]
        private void NavigateToHome()
        {
            // Reset state if needed when leaving
            CloseFullImage();
            _parentViewModel.NavigateToHome();
        }
    }
}
