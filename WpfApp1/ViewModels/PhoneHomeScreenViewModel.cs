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
    public partial class PhoneHomeScreenViewModel : ObservableObject
    {
        private readonly PhoneViewModel _parentViewModel;
        public PhoneHomeScreenViewModel(PhoneViewModel parent) { _parentViewModel = parent; }

        [RelayCommand]
        private void NavigateToStats() { _parentViewModel.NavigateToStats(); }

        [RelayCommand]
        private void NavigateToSettings()
        {
            // Gələcəkdə bura telefonun öz ayarlar ekranına keçid məntiqi gələcək.
            MessageBox.Show("Settings tətbiqi hələ hazır deyil.");
        }

        [RelayCommand]
        private void NavigateToChat()
        {
            // Gələcəkdə bura Chat/Mesajlaşma ekranına keçid məntiqi gələcək.
            MessageBox.Show("Chat tətbiqi hələ hazır deyil.");
        }

        [RelayCommand]
        private void NavigateToCG()
        {
            // Gələcəkdə bura CG (Qalereya) ekranına keçid məntiqi gələcək.
            MessageBox.Show("Gallery tətbiqi hələ hazır deyil.");
        }

        [RelayCommand]
        private void NavigateToCamera()
        {
            // Gələcəkdə bura Kamera funksiyasına keçid məntiqi gələcək (bəlkə bir mini-oyun?).
            MessageBox.Show("Camera tətbiqi hələ hazır deyil.");
        }
    }
}
