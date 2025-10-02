using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class PhoneViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentScreenViewModel;

        private readonly GameViewModel _parentViewModel;

        public PhoneViewModel(GameViewModel parent)
        {
            _parentViewModel = parent;
            // Telefon açılanda ilk olaraq Ana Ekranı göstər
            CurrentScreenViewModel = new PhoneHomeScreenViewModel(this);
        }

        public void NavigateToStats()
        {
            CurrentScreenViewModel = new PhoneStatsViewModel(this, _parentViewModel.MainCharacterStats);
        }

        public void NavigateToHome()
        {
            CurrentScreenViewModel = new PhoneHomeScreenViewModel(this);
        }

        [RelayCommand]
        private void ClosePhone()
        {
            _parentViewModel.TogglePhoneView();
        }
    }
}