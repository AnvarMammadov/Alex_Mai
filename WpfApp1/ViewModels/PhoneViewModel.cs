using System;
using System.ComponentModel;
using System.Windows;
using Alex_Mai.Models;
using Alex_Mai.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class PhoneViewModel : ObservableObject , IDisposable
    {
        [ObservableProperty]
        private ObservableObject _currentScreenViewModel;

        private readonly GameViewModel _parentViewModel;

        private readonly GameState _gameState;


        private PhoneHomeScreenViewModel _homeScreenViewModel;
        private PhoneStatsViewModel _statsViewModel;
        private ChatViewModel _chatViewModel;
        private GalleryViewModel _galleryViewModel;
        private WalletViewModel _walletViewModel;
        private CalendarViewModel _calendarViewModel;

        public PhoneViewModel(GameViewModel parent, GameState gameState)
        {
            _parentViewModel = parent;
            // Telefon açılanda ilk olaraq Ana Ekranı göstər
            _gameState = gameState;

            NavigateToHome();
        }


        public void NavigateToStats()
        {
            _statsViewModel ??= new PhoneStatsViewModel(this, _parentViewModel.MainCharacterStats);
            CurrentScreenViewModel = new PhoneStatsViewModel(this, _parentViewModel.MainCharacterStats);
        }

        public void NavigateToChat()
        {
            _chatViewModel ??= new ChatViewModel(this, _parentViewModel.MainCharacterStats);
            // Pass the MainCharacterStats from GameViewModel to ChatViewModel
            CurrentScreenViewModel = new ChatViewModel(this, _parentViewModel.MainCharacterStats);
        }

        public void NavigateToHome()
        {
            if (_gameState != null)
            {
                // Cache-dən istifadə et və ya yarat
                _homeScreenViewModel ??= new PhoneHomeScreenViewModel(this, _gameState);
                CurrentScreenViewModel = _homeScreenViewModel;
            }
            else
            {
                // Xəta halını idarə et
                System.Windows.MessageBox.Show("Error: Cannot access game state for Phone Home Screen.");
            }

            // Opsional Cleanup
            _walletViewModel?.Cleanup();
            _chatViewModel?.Cleanup(); // ChatViewModel-də də Cleanup metodu olmalıdır
            _statsViewModel?.Cleanup(); // StatsViewModel-də də Cleanup metodu olmalıdır
            _galleryViewModel?.Cleanup(); // GalleryViewModel-də də Cleanup metodu olmalıdır 
            _calendarViewModel?.Cleanup();
        }
        public void NavigateToGallery()
        {
            _galleryViewModel ??= new GalleryViewModel(this);
            CurrentScreenViewModel = new GalleryViewModel(this);
        }

        public void NavigateToWallet()
        {
            if (_gameState != null)
            {
                _walletViewModel ??= new WalletViewModel(this, _gameState);
                CurrentScreenViewModel = _walletViewModel;
            }
            else
            {
                // Handle error: GameState not available
                System.Windows.MessageBox.Show("Error: Cannot access game state for Wallet.");
            }
        }

        // <-- BU YENİ METODU ƏLAVƏ EDİN -->
        public void NavigateToCalendar()
        {
            if (_gameState != null)
            {
                _calendarViewModel ??= new CalendarViewModel(this, _gameState);
                CurrentScreenViewModel = _calendarViewModel;
            }
            else
            {
                System.Windows.MessageBox.Show("Error: Cannot access game state for Calendar.");
            }
        }



        // *** YENİ: Cleanup metodu / IDisposable implementasiyası ***
        public void Cleanup()
        {
            Console.WriteLine("PhoneViewModel Cleanup called."); // Debugging
            // Cleanup cached ViewModels
            _homeScreenViewModel?.Cleanup();
            _statsViewModel?.Cleanup();
            _chatViewModel?.Cleanup();
            _galleryViewModel?.Cleanup();
            _walletViewModel?.Cleanup();
            _calendarViewModel?.Cleanup();

            // Clear cache references (optional, depends if PhoneViewModel itself is long-lived)
            _homeScreenViewModel = null;
            _statsViewModel = null;
            _chatViewModel = null;
            _galleryViewModel = null;
            _walletViewModel = null;
            _calendarViewModel = null;
        }


        [RelayCommand]
        private void ClosePhone()
        {
            Cleanup(); // Call Cleanup when phone is closed
            _parentViewModel.TogglePhoneView();
        }

        // Implement IDisposable if you want to ensure cleanup (optional but good practice)
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Cleanup(); // Call cleanup logic
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        // *** Cleanup / IDisposable sonu ***
    }
}