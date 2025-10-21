using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alex_Mai.Models
{
    // Make it partial and ObservableObject if IsUnlocked needs to dynamically update the UI later
    public partial class GalleryItem : ObservableObject
    {
        public string Id { get; set; } // Unique ID for the CG
        public string Title { get; set; } // Optional title/description
        public string ThumbnailPath { get; set; } // Path to the small image shown in the grid
        public string FullImagePath { get; set; } // Path to the full-size image

        // We'll load the actual unlocked status from save data later
        // For now, the service will load the default from JSON
        [ObservableProperty] // Make it observable if UI should react to changes
        private bool _isUnlocked = false;
    }
}
