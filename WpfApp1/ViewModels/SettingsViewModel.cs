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
        private readonly SettingsService _settingsService; // <-- Dəyişiklik yoxdur

        [ObservableProperty]
        private GameSettings _currentSettings;

        public SettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            // DƏYİŞİKLİK: 'new SettingsService()' əvəzinə 'Instance' istifadə edirik
            _settingsService = SettingsService.Instance;

            // DƏYİŞİKLİK: 'LoadSettings()' əvəzinə 'GetSettings()' çağırırıq
            CurrentSettings = _settingsService.GetSettings();
        }

        [RelayCommand]
        private void SaveAndBack()
        {
            // DƏYİŞİKLİK: 'SaveSettings' artıq parametri özü bilir
            _settingsService.SaveSettings();

            // Ana menyuya qayıt
            _mainViewModel.NavigateToMenu();
        }
    }
}
