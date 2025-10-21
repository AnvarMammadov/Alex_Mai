using Alex_Mai.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class PhoneViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentScreenViewModel;

        private readonly GameViewModel _parentViewModel;


        private PhoneHomeScreenViewModel _homeScreenViewModel;
        private PhoneStatsViewModel _statsViewModel;
        private ChatViewModel _chatViewModel;
        private GalleryViewModel _galleryViewModel;
        private WalletViewModel _walletViewModel;

        public PhoneViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;
            // Telefon açılanda ilk olaraq Ana Ekranı göstər
            CurrentScreenViewModel = new PhoneHomeScreenViewModel(this);
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
            _homeScreenViewModel ??= new PhoneHomeScreenViewModel(this);
        CurrentScreenViewModel = _homeScreenViewModel;
        // Opsional: Artıq istifadə olunmayan ViewModel-lərin Cleanup metodlarını çağır
         _walletViewModel?.Cleanup(); // WalletViewModel-dən çıxanda Cleanup çağırılır
         // Digər VM-lər üçün də oxşar Cleanup əlavə etmək olar
        }
        public void NavigateToGallery()
        {
            _galleryViewModel ??= new GalleryViewModel(this);
            CurrentScreenViewModel = new GalleryViewModel(this);
        }

        public void NavigateToWallet()
        {
            // Assuming _parentViewModel is GameViewModel and has a public GameState property
            if (_parentViewModel.CurrentGameState != null)
            {
                // Əgər yoxdursa yarat, varsa istifadə et
                _walletViewModel ??= new WalletViewModel(this, _parentViewModel.CurrentGameState);
                CurrentScreenViewModel = _walletViewModel;
            }
            else
            {
                // Handle error: GameState not available
                System.Windows.MessageBox.Show("Error: Cannot access game state for Wallet.");
            }
        }



        [RelayCommand]
        private void ClosePhone()
        {
            // Opsional: Telefon bağlandıqda bütün aktiv VM-lərin Cleanup metodlarını çağır
            _walletViewModel?.Cleanup();
            // _chatViewModel?.Cleanup(); // Chat üçün də əlavə etsəniz
            // ...
            _parentViewModel.TogglePhoneView();
        }
    }
}