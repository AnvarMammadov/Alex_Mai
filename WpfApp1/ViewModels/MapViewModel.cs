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

        public ObservableCollection<Location> HomeLocations { get; set; }
        public ObservableCollection<Location> OutLocations { get; set; }

        [ObservableProperty]
        private ObservableCollection<Location> _currentLocations;

        [ObservableProperty]
        private bool _isShowingHome = true;

        public MapViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;

            HomeLocations = new ObservableCollection<Location>
            {
                 new Location { Name = "Alex's Room", PlaceId = "alex_room" },
                 new Location { Name = "Mai's Room",  PlaceId = "mai_room"  },
                 new Location { Name = "Living Room", PlaceId = "living_room"}, // hələ səhnə yox
                 new Location { Name = "Kitchen",     PlaceId = "kitchen"   },
                 new Location { Name = "Bathroom",    PlaceId = "bathroom"  }
            };

            OutLocations = new ObservableCollection<Location>
            {
                new Location { Name = "Part-time Job", PlaceId = "part_time",},
                new Location { Name = "Market",        PlaceId = "market", },
                new Location { Name = "Park",          PlaceId = "park",   }
            };

            // Başlanğıcda Ev məkanlarını göstər
            CurrentLocations = HomeLocations;
        }

        [RelayCommand]
        private void ShowHome()
        {
            CurrentLocations = HomeLocations;
            IsShowingHome = true;
        }

        [RelayCommand]
        private void ShowOut()
        {
            CurrentLocations = OutLocations;
            IsShowingHome = false;
        }

        [RelayCommand]
        private void NavigateTo(Location location)
        {
            if (location == null || !location.IsEnabled) return;
            _parentViewModel.GoToPlace(location.PlaceId);
        }

        [RelayCommand]
        private void CloseMap()
        {
            _parentViewModel.ToggleMap();
        }
    }
}
