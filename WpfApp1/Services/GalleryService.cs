using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Alex_Mai.Models;

namespace Alex_Mai.Services
{
    public class GalleryService
    {
        private List<GalleryItem> _galleryItems = new List<GalleryItem>();
        private readonly string _filePath;

        public GalleryService(string filePath = "Data/gallery_items.json")
        {
            _filePath = filePath;
            LoadGalleryItems();
        }

        private void LoadGalleryItems()
        {
            try
            {
                var jsonText = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loadedItems = JsonSerializer.Deserialize<List<GalleryItem>>(jsonText, options);

                if (loadedItems != null)
                {
                    _galleryItems = loadedItems;
                    // TODO LATER: Merge loaded default status with actual unlocked status from save data
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading gallery items from {_filePath}: {ex.Message}");
                _galleryItems = new List<GalleryItem>(); // Return empty list on error
            }
        }

        // Gets all items (including locked ones, view model will filter)
        public List<GalleryItem> GetAllGalleryItems()
        {
            // Return a copy if modification is a concern, for now return direct list
            return _galleryItems;
        }

        // TODO LATER: Add methods to update IsUnlocked status based on game events
        // and interact with SaveLoadService
        public void UnlockItem(string itemId)
        {
            var item = _galleryItems.Find(i => i.Id.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.IsUnlocked = true;
                // TODO: Notify SaveLoadService or GameState that an item was unlocked
                Console.WriteLine($"Gallery Item Unlocked: {itemId}"); // Debug message
            }
        }

        // TODO LATER: Method to load unlocked status from save data
        public void ApplyUnlockedStatus(List<string> unlockedIds)
        {
            foreach (var item in _galleryItems)
            {
                item.IsUnlocked = unlockedIds?.Contains(item.Id, StringComparer.OrdinalIgnoreCase) ?? false;
            }
        }

        // TODO LATER: Method to get unlocked IDs for saving
        public List<string> GetUnlockedItemIds()
        {
            return _galleryItems.Where(item => item.IsUnlocked).Select(item => item.Id).ToList();
        }
    }
}