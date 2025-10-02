using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Alex_Mai.Models
{
    public partial class GameState : ObservableObject
    {
        [NotifyPropertyChangedFor(nameof(DayOfWeek))]
        [NotifyPropertyChangedFor(nameof(FullDateString))]
        [ObservableProperty]
        private int _currentDay = 1;

        [ObservableProperty]
        private TimeOfDay _timeOfDay = TimeOfDay.Morning;

        [ObservableProperty]
        private int _playerMoney = 450;

        [ObservableProperty]
        private int _currentEnergy = 74;

        public int MaxEnergy { get; } = 100; // Maksimum enerji

        // Həftənin gününü hesablayan yeni xüsusiyyət
        public string DayOfWeek
        {
            get
            {
                string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                // Oyun 1-ci gündən başladığı üçün (index 0) (_currentDay - 1) istifadə edirik
                return days[(CurrentDay - 1) % 7];
            }
        }


        // Ayın gününü mətn formatında göstərən yeni xüsusiyyət (Oyunun Sentyabrda başladığını fərz edirik)
        public string FullDateString => $"September {CurrentDay}";
    }
}
