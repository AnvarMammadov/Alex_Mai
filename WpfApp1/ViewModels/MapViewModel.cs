using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Alex_Mai.Models;

namespace Alex_Mai.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly GameViewModel _parentViewModel;

        // Bu kolleksiyalar olduğu kimi qalır
        public ObservableCollection<Location> HomeLocations { get; set; }
        public ObservableCollection<Location> OutLocations { get; set; }

        [ObservableProperty]
        private ObservableCollection<Location> _currentLocations; // Bu, "OUT" üçün istifadə olunacaq

        [ObservableProperty]
        private bool _isShowingHome = true;

        public MapViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;

            HomeLocations = new ObservableCollection<Location>
            {
                 new Location { Name = "Alex's Room", PlaceId = "alex_room" },
                 new Location { Name = "Mai's Room",  PlaceId = "mai_room"  },
                 new Location { Name = "Living Room", PlaceId = "living_room"},
                 new Location { Name = "Kitchen",     PlaceId = "kitchen"   },
                 new Location { Name = "Bathroom",    PlaceId = "bathroom"  }
            };

            OutLocations = new ObservableCollection<Location>
            {
                new Location { Name = "Part-time Job", PlaceId = "part_time",},
                new Location { Name = "Market",        PlaceId = "market", },
                new Location { Name = "Park",          PlaceId = "park",   }
            };

            // "OUT" üçün siyahını ilkin olaraq təyin edirik
            CurrentLocations = OutLocations;
        }

        [RelayCommand]
        private void ShowHome()
        {
            // CurrentLocations-u dəyişmirik, sadəcə nəyin görünəcəyini idarə edirik
            IsShowingHome = true;
        }

        [RelayCommand]
        private void ShowOut()
        {
            // CurrentLocations artıq OutLocations-a bərabərdir
            IsShowingHome = false;
        }

        // --- DƏYİŞİKLİK BURADADIR ---
        // Metod artıq 'Location' obyekti yox, birbaşa 'string placeId' qəbul edir.
        [RelayCommand]
        private void NavigateTo(string placeId)
        {
            // Yoxlama string-ə görə aparılır
            if (string.IsNullOrEmpty(placeId)) return;

            // Model yoxlaması (opsional, amma yaxşıdır):
            var locationExists = HomeLocations.Any(l => l.PlaceId == placeId) ||
                                 OutLocations.Any(l => l.PlaceId == placeId);

            if (!locationExists) return; // Belə bir məkan yoxdursa

            // Birbaşa string placeId-ni GameViewModel-ə ötürürük
            _parentViewModel.GoToPlace(placeId);
        }
        // --- DƏYİŞİKLİK SONU ---

        [RelayCommand]
        private void CloseMap()
        {
            _parentViewModel.ToggleMap();
        }
    }
}
