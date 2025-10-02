using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Alex_Mai.ViewModels;

namespace Alex_Mai.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject _currentViewModel;

        public MainViewModel()
        {
            // Proqram başlayanda menyunu göstər
            CurrentViewModel = new MenuViewModel(this);
        }

        public void NavigateToGame()
        {
            CurrentViewModel = new GameViewModel();
        }

        public void NavigateToSettings()
        {
            CurrentViewModel = new SettingsViewModel(this);
        }
        // Həmçinin geriyə qayıtmaq üçün bu metodu da əlavə edin
        public void NavigateToMenu()
        {
            CurrentViewModel = new MenuViewModel(this);
        }

        public void LoadAndNavigateToGame()
        {
            var saveLoadService = new SaveLoadService();
            var saveData = saveLoadService.LoadGame();

            if (saveData != null)
            {
                // Yüklənmiş data ilə yeni GameViewModel yaradırıq
                CurrentViewModel = new GameViewModel(saveData.GameState, saveData.CharacterStats);
            }
            else
            {
                // Save faylı yoxdursa, yeni oyun başla
                NavigateToGame();
            }
        }
    }
}
