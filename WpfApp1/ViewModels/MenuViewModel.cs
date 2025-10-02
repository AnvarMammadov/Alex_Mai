using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace Alex_Mai.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        public MenuViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        private void StartGame()
        {
            // MainViewModel-a xəbər ver ki, oyun səhifəsinə keçsin
            _mainViewModel.NavigateToGame();
        }

        [RelayCommand]
        private void LoadGame()
        {
            _mainViewModel.LoadAndNavigateToGame();
        }

        [RelayCommand]
        private void Settings()
        {
            _mainViewModel.NavigateToSettings();
        }

        [RelayCommand]
        private void QuitGame()
        {
            Application.Current.Shutdown();
        }
    }
}
