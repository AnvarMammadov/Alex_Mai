using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class WalletViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        private readonly GameState _gameState; // Reference to GameState to get PlayerMoney

        // No need for [ObservableProperty] if GameState.PlayerMoney is already observable
        public int PlayerMoney => _gameState.PlayerMoney;

        // *** YENİ: Əməliyyat Tarixçəsi ***
        // *** YENİ: GameState-dəki kolleksiyaya birbaşa çıxış ***
        public ObservableCollection<Transaction> Transactions => _gameState.Transactions;


        public WalletViewModel(PhoneViewModel parent, GameState gameState)
        {
            _parentViewModel = parent;
            _gameState = gameState;

            // Listen for changes in PlayerMoney from GameState
            _gameState.PropertyChanged += GameState_PropertyChanged;


        }

        private void GameState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If the PlayerMoney property in GameState changes, notify the UI
            if (e.PropertyName == nameof(GameState.PlayerMoney))
            {
                OnPropertyChanged(nameof(PlayerMoney));
            }
        }
       


        [RelayCommand]
        private void NavigateToHome()
        {
            // Unsubscribe when leaving the view to prevent memory leaks
            _gameState.PropertyChanged -= GameState_PropertyChanged;
            _parentViewModel.NavigateToHome();
        }

        // Optional: Cleanup when ViewModel is no longer needed (might need better lifecycle management)
        public void Cleanup()
        {
            _gameState.PropertyChanged -= GameState_PropertyChanged;
        }
    }
}