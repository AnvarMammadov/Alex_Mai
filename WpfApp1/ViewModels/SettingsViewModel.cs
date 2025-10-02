using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Models;
using Alex_Mai.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly SettingsService _settingsService;

        [ObservableProperty]
        private GameSettings _currentSettings;

        public SettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _settingsService = new SettingsService();
            CurrentSettings = _settingsService.LoadSettings();
        }

        [RelayCommand]
        private void SaveAndBack()
        {
            _settingsService.SaveSettings(CurrentSettings);
            // Ana menyuya qayıt
            _mainViewModel.NavigateToMenu();
        }
    }
}
