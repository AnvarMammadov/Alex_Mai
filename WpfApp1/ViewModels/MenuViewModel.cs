using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Alex_Mai.Services; // <-- 1. BU 'using' SƏTRİNİ ƏLAVƏ EDİN
using Alex_Mai.Models; // <-- YENİ: SaveData üçün əlavə edin


namespace Alex_Mai.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private readonly AudioService _audioService; // <-- 2. BU SAHƏNİ (FIELD) ƏLAVƏ EDİN

        private readonly SaveLoadService _saveLoadService;


        // YENİ: UI-a bağlanmaq üçün property-lər (8-ci bənd)
        [ObservableProperty]
        private string _loadGameButtonText = "Load Game";
        [ObservableProperty]
        private bool _isLoadGameEnabled = false;

        public MenuViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _audioService = AudioService.Instance; // <-- 3. BU KODU ƏLAVƏ EDİN
            _saveLoadService = new SaveLoadService(); // YENİ
            // Menyu açılan kimi fon musiqisini başlat
            _audioService.PlayBGM("morning_theme.mp3"); // <-- 4. BU KODU ƏLAVƏ EDİN

            CheckForSaveFile(); // YENİ: Yaddaşı yoxla
        }

        [RelayCommand]
        private void StartGame()
        {
            _audioService.PlaySFX("neutral_click.mp3"); // <-- 5. BU KODU ƏLAVƏ EDİN
            _audioService.StopBGM(); // Oyuna keçərkən menyu musiqisini dayandır
            _mainViewModel.NavigateToGame();
        }

        [RelayCommand]
        private void LoadGame()
        {
            _audioService.PlaySFX("neutral_click.mp3"); // <-- 6. BU KODU ƏLAVƏ EDİN
            _audioService.StopBGM(); // Oyuna keçərkən menyu musiqisini dayandır
            _mainViewModel.LoadAndNavigateToGame();
        }

        [RelayCommand]
        private void Settings()
        {
            _audioService.PlaySFX("neutral_click.mp3"); // <-- 7. BU KODU ƏLAVƏ EDİN
            _mainViewModel.NavigateToSettings();
        }

        [RelayCommand]
        private void QuitGame()
        {
            _audioService.PlaySFX("neutral_click.mp3"); // <-- 8. BU KODU ƏLAVƏ EDİN
            Application.Current.Shutdown();
        }




        // YENİ METOD: Yaddaş faylını yoxlayır
        private void CheckForSaveFile()
        {
            SaveData saveData = _saveLoadService.LoadGame();
            if (saveData != null && saveData.GameState != null)
            {
                // Yaddaş varsa, düyməni aktivləşdir və mətnini dəyiş
                LoadGameButtonText = $"Continue (Day {saveData.GameState.CurrentDay})"; //
                IsLoadGameEnabled = true;
            }
            else
            {
                // Yaddaş yoxdursa
                LoadGameButtonText = "Load Game";
                IsLoadGameEnabled = false;
            }
        }

    }
}