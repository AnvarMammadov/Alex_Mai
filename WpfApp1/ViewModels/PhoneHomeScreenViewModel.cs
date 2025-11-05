using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Alex_Mai.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Alex_Mai.ViewModels
{
    public partial class PhoneHomeScreenViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        private readonly GameState _gameState; // *** YENİ: GameState referansı ***
                                               // *** YENİ: Konstruktoru GameState qəbul etmək üçün yeniləyin ***


        public PhoneHomeScreenViewModel(PhoneViewModel parent, GameState gameState)
        {
            _parentViewModel = parent;
            _gameState = gameState;

            if (_gameState != null)
            {
                _gameState.PropertyChanged += GameState_PropertyChanged;
            }
        }

        private void GameState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // *** DƏYİŞİKLİK: Dispatcher istifadə edərək OnPropertyChanged çağırırıq ***
            if (e.PropertyName == nameof(GameState.TimeOfDay))
            {
                Application.Current?.Dispatcher?.InvokeAsync(() => OnPropertyChanged(nameof(FormattedTime)));
                Console.WriteLine($"GameState TimeOfDay Changed - Firing OnPropertyChanged for FormattedTime on UI Thread"); // Debugging
            }
            else if (e.PropertyName == nameof(GameState.CurrentDay))
            {
                Application.Current?.Dispatcher?.InvokeAsync(() => OnPropertyChanged(nameof(FormattedDate)));
                Console.WriteLine($"GameState CurrentDay Changed - Firing OnPropertyChanged for FormattedDate on UI Thread"); // Debugging
            }
        }


        public string FormattedTime
        {
            get
            {
                switch (_gameState?.TimeOfDay) // Null yoxlaması əlavə edildi
                {
                    case TimeOfDay.Morning: return "09:00";
                    case TimeOfDay.Afternoon: return "14:00";
                    case TimeOfDay.Evening: return "20:00";
                    case TimeOfDay.Night: return "00:00";
                    default: return "--:--";
                }
            }
        }
      public string FormattedDate
        {
            get
            {
                try
                {
                    // Null yoxlaması əlavə edildi
                    return _gameState != null ? $"Sep {_gameState.CurrentDay}" : "Date Err";
                }
                catch { return "Date Error"; }
            }
        }

        [RelayCommand]
        private void NavigateToStats() { _parentViewModel.NavigateToStats(); }

        [RelayCommand]
        private void NavigateToSettings()
        {
            // Gələcəkdə bura telefonun öz ayarlar ekranına keçid məntiqi gələcək.
            MessageBox.Show("Settings tətbiqi hələ hazır deyil.");
        }

        [RelayCommand]
        private void NavigateToChat()
        {
            _parentViewModel.NavigateToChat();
        }

        [RelayCommand]
        private void NavigateToCG()
        {
            _parentViewModel.NavigateToGallery();
        }

        [RelayCommand]
        private void NavigateToCamera()
        {
            // Gələcəkdə bura Kamera funksiyasına keçid məntiqi gələcək (bəlkə bir mini-oyun?).
            MessageBox.Show("Camera tətbiqi hələ hazır deyil.");
        }

        [RelayCommand]
        private void NavigateToWallet()
        {
            _parentViewModel.NavigateToWallet(); // Call the method in PhoneViewModel
        }


        // *** YENİ: Cleanup metodu ***
        public void Cleanup()
        {
            // *** DƏYİŞİKLİK: Abunəlikdən çıxırıq ***
            if (_gameState != null)
            {
                _gameState.PropertyChanged -= GameState_PropertyChanged;
                Console.WriteLine("PhoneHomeScreenViewModel cleaned up."); // Debugging
            }
        }
    }
}
