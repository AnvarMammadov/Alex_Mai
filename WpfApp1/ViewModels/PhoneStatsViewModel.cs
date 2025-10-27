using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex_Mai.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Alex_Mai.ViewModels
{
    public partial class PhoneStatsViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        public string StatsText { get; }

        public PhoneStatsViewModel(PhoneViewModel parent, CharacterStats stats)
        {
            _parentViewModel = parent;
            // Statları formatlı bir mətnə çeviririk
            StatsText = $"Affection: {stats.Affection}\n" +
                        $"Trust: {stats.Trust}\n" +
                        $"Stress: {stats.Stress}\n";
                       // $"Corruption: {stats.Corruption}";
        }

        [RelayCommand]
        private void NavigateToHome() { _parentViewModel.NavigateToHome(); }

        public void Cleanup()
        {
            // Hazırda heç bir abunəlik yoxdur.
            Console.WriteLine("PhoneStatsViewModel cleaned up."); // Debug üçün mesaj (opsional)
        }
    }
}

